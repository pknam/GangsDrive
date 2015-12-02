using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using DokanNet;
using FileAccess = DokanNet.FileAccess;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;
using System.Windows.Forms;
using System.Net.Sockets;


namespace GangsDrive
{
    class GangsSFTPDriver : GangsDriver, IDokanOperations
    {
        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                         FileAccess.Execute |
                                         FileAccess.GenericExecute | FileAccess.GenericWrite | FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        private string host;
        private int port;
        private SftpClient sftpClient;

        public GangsSFTPDriver(string host, int port, string username, string password, string mountPoint)
            :base(mountPoint, "SFTP")
        {
            this.host = host;
            this.port = port;
            this.sftpClient = new SftpClient(host, port, username, password);
        }

        private string ToUnixStylePath(string winPath)
        {
            return string.Format(@"/{0}", winPath.Replace(@"\", @"/").Replace("//", "/"));
        }

        #region Implementation of IDokanOperations
        public void Cleanup(string fileName, DokanFileInfo info)
        {
            if(info.Context != null && info.Context is SftpFileStream)
            {
                (info.Context as SftpFileStream).Dispose();
            }
            info.Context = null;
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
            if (info.Context != null && info.Context is SftpFileStream)
            {
                (info.Context as SftpFileStream).Dispose();
            }
            info.Context = null;
        }

        public NtStatus CreateDirectory(string fileName, DokanFileInfo info)
        {
            //Debug.Print("CreateDirectory. filename : {0}", fileName);

            fileName = ToUnixStylePath(fileName);

            if(sftpClient.Exists(fileName))
                return DokanResult.FileExists;

            try
            {
                sftpClient.CreateDirectory(fileName);
                return DokanResult.Success;
            }
            catch(Renci.SshNet.Common.SshException)
            {
                return DokanResult.AccessDenied;
            }
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            Debug.WriteLine(@"{0} : {1} {2}", fileName, mode.ToString(), access.ToString());

            fileName = ToUnixStylePath(fileName);

            if (fileName.EndsWith("desktop.ini", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("autorun.inf", StringComparison.OrdinalIgnoreCase))
            {
                return DokanResult.FileNotFound;
            }

            // todo : add to memory cache
            bool exists = sftpClient.Exists(fileName);
            SftpFileAttributes attr = sftpClient.GetAttributes(fileName);
            bool readWriteAttributes = (access & DataAccess) == 0;
            bool readAccess = (access & DataWriteAccess) == 0;
            System.IO.FileAccess acs = readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite;

            switch(mode)
            {
                case FileMode.Open:

                    if (!exists)
                        return DokanResult.FileNotFound;

                    if (((uint)access & 0xe0000027) == 0 || attr.IsDirectory)
                    {
                        info.IsDirectory = attr.IsDirectory;
                        info.Context = new object();
                        return DokanResult.Success;
                    }

                    break;

                case FileMode.CreateNew:
                    if (exists)
                        return DokanResult.AlreadyExists;


                    // cache invalidate

                    break;

                case FileMode.Truncate:
                    if (!exists)
                        return DokanResult.FileNotFound;
                    // cache invalidate
                    break;

                default:
                    // cache invalidate
                    break;
            }

            try
            {
                info.Context = sftpClient.Open(fileName, mode, acs) as SftpFileStream;
            }
            catch(Renci.SshNet.Common.SshException)
            {
                return DokanResult.AccessDenied;
            }

            return DokanResult.Success;
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            fileName = ToUnixStylePath(fileName);

            if(!sftpClient.Exists(fileName))
            {
                return DokanResult.FileNotFound;
            }

            try
            {
                sftpClient.DeleteDirectory(fileName);
            }
            catch(SftpPermissionDeniedException)
            {
                return DokanResult.AccessDenied;
            }
            catch (SshException)
            {
                return DokanResult.InvalidParameter;
            }

            return DokanResult.Success;
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            fileName = ToUnixStylePath(fileName);

            if(!sftpClient.Exists(fileName))
            {
                return DokanResult.FileNotFound;
            }

            try
            {
                sftpClient.DeleteFile(fileName);
            }
            catch(SftpPermissionDeniedException)
            {
                return DokanResult.AccessDenied;
            }
            catch(SshException)
            {
                return DokanResult.InvalidParameter;
            }

            return DokanResult.Success;
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            streamName = String.Empty;
            streamSize = 0;
            return DokanResult.NotImplemented;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            List<SftpFile> fileList;
            
            try
            {
                fileList = sftpClient.ListDirectory(ToUnixStylePath(fileName)).ToList();
            }
            catch(Renci.SshNet.Common.SftpPermissionDeniedException)
            {
                files = null;
                return DokanResult.AccessDenied;
            }

            files = new List<FileInformation>();

            foreach(var file in fileList)
            {
                FileInformation finfo = new FileInformation();
                SftpFileAttributes attr = sftpClient.GetAttributes(file.FullName);

                finfo.FileName = file.Name;
                finfo.Attributes = FileAttributes.NotContentIndexed;
                finfo.CreationTime = file.LastWriteTime;
                finfo.LastAccessTime = file.LastAccessTime;
                finfo.LastWriteTime = file.LastWriteTime;
                finfo.Length = file.Length;

                if(file.IsDirectory)
                {
                    finfo.Attributes |= FileAttributes.Directory;
                    finfo.Length = 0;
                }
                else
                {
                    finfo.Attributes |= FileAttributes.Normal;
                }

                if(finfo.FileName.StartsWith("."))
                {
                    finfo.Attributes |= FileAttributes.Hidden;
                }

                // todo :
                // readonly check

                files.Add(finfo);
            }

            return DokanResult.Success;
        }

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            (info.Context as SftpFileStream).Flush();
            return DokanResult.Success;
        }

        public NtStatus GetDiskFreeSpace(out long free, out long total, out long used, DokanFileInfo info)
        {
            free = 512 * 1024 * 1024;
            total = 1024 * 1024 * 1024;
            used = 512 * 1024 * 1024;

            return DokanResult.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            SftpFile file = sftpClient.Get(ToUnixStylePath(fileName));

            fileInfo = new FileInformation()
            {
                FileName = file.Name,
                Attributes = FileAttributes.NotContentIndexed,
                CreationTime = file.LastWriteTime,
                LastAccessTime = file.LastAccessTime,
                LastWriteTime = file.LastWriteTime,
                Length = file.Length
            };

            if (file.IsDirectory)
            {
                fileInfo.Attributes |= FileAttributes.Directory;
                fileInfo.Length = 0;
            }
            else
            {
                fileInfo.Attributes |= FileAttributes.Normal;
            }

            if (fileInfo.FileName.StartsWith("."))
            {
                fileInfo.Attributes |= FileAttributes.Hidden;
            }

            // todo :
            // readonly check

            return DokanResult.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            security = null;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = host;
            fileSystemName = "GangsDrive";

            features = FileSystemFeatures.None;

            return DokanResult.Success;
        }

        public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            if (!sftpClient.Exists(newName))
            {
                info.Context = null;
                sftpClient.RenameFile(oldName, newName);
            }
            else if(replace)
            {
                info.Context = null;

                sftpClient.Delete(newName);
                sftpClient.RenameFile(oldName, newName);
            }
            else
            {
                return DokanResult.FileExists;
            }

            return DokanResult.Success;
        }

        public NtStatus OpenDirectory(string fileName, DokanFileInfo info)
        {
            if (!sftpClient.Exists(ToUnixStylePath(fileName)))
                return DokanResult.PathNotFound;

            return DokanResult.Success;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            if(info.Context == null)
            {
                using (SftpFileStream stream = sftpClient.Open(ToUnixStylePath(fileName), FileMode.Open, System.IO.FileAccess.Read))
                {
                    stream.Position = offset;
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }
            else
            {
                SftpFileStream stream = info.Context as SftpFileStream;
                lock (stream)
                {
                    stream.Position = offset;
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }

            return DokanResult.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            (info.Context as SftpFileStream).SetLength(length);
            return DokanResult.Success;
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            (info.Context as SftpFileStream).SetLength(length);
            return DokanResult.Success;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus SetFileSecurity(string fileName, System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus Unmount(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            if(info.Context == null)
            {
                using(SftpFileStream stream = sftpClient.Open(fileName, FileMode.Open, System.IO.FileAccess.Write))
                {
                    stream.Position = offset;
                    stream.Write(buffer, 0, buffer.Length);
                    bytesWritten = buffer.Length;
                }
            }
            else
            {
                var stream = info.Context as SftpFileStream;
                stream.Write(buffer, 0, buffer.Length);
                bytesWritten = buffer.Length;
            }

            return DokanResult.Success;
        } 
        #endregion

        #region Overriding of GangsDriver

        public override void Mount()
        {
            if (IsMounted)
                return;

            try
            {
                sftpClient.Connect();
            }
            catch(SocketException)
            {
                MessageBox.Show(string.Format("Cannot Connect to {0}:{1}", host, port));
                return;
            }
            catch(SshAuthenticationException)
            {
                MessageBox.Show("Invalid username or password");
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return;
            }


            if (!sftpClient.IsConnected)
            {
                MessageBox.Show("cannot connect");
                return;
            }

            base.Mount();
        }

        public override void ClearMountPoint()
        {
            base.ClearMountPoint();

            if (!IsMounted)
                return;


            if (sftpClient.IsConnected)
            {
                sftpClient.Disconnect();
                sftpClient.Dispose();
            }
        }
        #endregion
    }
}
