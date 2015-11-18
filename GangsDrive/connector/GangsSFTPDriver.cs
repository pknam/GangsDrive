﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using DokanNet;
using FileAccess = DokanNet.FileAccess;
using Renci.SshNet;
using Renci.SshNet.Sftp;


namespace GangsDrive
{
    class GangsSFTPDriver : IDokanOperations, IGangsDriver
    {
        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                         FileAccess.Execute |
                                         FileAccess.GenericExecute | FileAccess.GenericWrite | FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        private string host;
        private SftpClient sftpClient;

        private readonly string _mountPoint;
        private readonly string _driverName = "SFTP";
        private bool _isMounted;
        public event EventHandler<connector.MountChangedArgs> OnMountChangedEvent;

        public GangsSFTPDriver(string host, int port, string username, string password, string mountPoint)
        {
            this.host = host;
            this.sftpClient = new SftpClient(host, port, username, password);
            this._mountPoint = mountPoint;
            this._isMounted = false;
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
            return DokanResult.Error;
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
            return DokanResult.Error;
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            return DokanResult.Error;
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
            return DokanResult.Error;
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
            bytesWritten = 0;
            return DokanResult.Error;
        } 
        #endregion

        #region Implementation of IGangsDriver
        public string MountPoint
        {
            get { return _mountPoint; }
        }

        public bool IsMounted
        {
            get { return _isMounted; }
        }

        public string DriverName
        {
            get { return _driverName; }
        }

        public void Mount()
        {
            if (IsMounted)
                return;

            sftpClient.Connect();
            if (!sftpClient.IsConnected)
                throw new Exception("cannot connect");

            this._isMounted = true;
            OnMountChanged(new connector.MountChangedArgs(_isMounted));
            this.Mount(this.MountPoint, DokanOptions.DebugMode, 5);
        }

        public void ClearMountPoint()
        {
            if (!IsMounted)
                return;

            Dokan.RemoveMountPoint(this.MountPoint);
            this._isMounted = false;
            OnMountChanged(new connector.MountChangedArgs(_isMounted));

            if (sftpClient.IsConnected)
            {
                sftpClient.Disconnect();
                sftpClient.Dispose();
            }
        } 
        #endregion

        protected virtual void OnMountChanged(connector.MountChangedArgs e)
        {
            EventHandler<connector.MountChangedArgs> handler = OnMountChangedEvent;

            if (handler != null)
                handler(this, e);
        }
    }
}
