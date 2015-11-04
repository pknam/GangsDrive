using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        private bool _isMounted;

        public GangsSFTPDriver(string host, int port, string username, string password, string mountPoint)
        {
            this.host = host;
            this.sftpClient = new SftpClient(host, port, username, password);
            this._mountPoint = mountPoint;
            this._isMounted = false;
        }

        #region Implementation of IDokanOperations
        void Cleanup(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        void CloseFile(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus CreateDirectory(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            if (fileName.EndsWith("desktop.ini", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("autorun.inf", StringComparison.OrdinalIgnoreCase)) //....
            {
                return DokanResult.FileNotFound;
            }

            // todo : add to memory cache
            SftpFileAttributes attr = sftpClient.GetAttributes(fileName);
            bool readWriteAttributes = (access & DataAccess) == 0;
            bool readAccess = (access & DataWriteAccess) == 0;
            System.IO.FileAccess acs = readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite;

            switch(mode)
            {
                case FileMode.Open:
                    if (attr == null)
                        return DokanResult.FileNotFound;

                    if (readWriteAttributes || attr.IsDirectory)
                    {
                        info.IsDirectory = attr.IsDirectory;
                        info.Context = sftpClient.Open(fileName, FileMode.Open);
                        return DokanResult.Success;
                    }

                    break;

                case FileMode.CreateNew:
                    if (attr != null)
                        return DokanResult.AlreadyExists;

                    // cache invalidate

                    break;

                case FileMode.Truncate:
                    if (attr == null)
                        return DokanResult.FileNotFound;
                    // cache invalidate
                    break;

                default:
                    // cache invalidate
                    break;
            }

            try
            {
                info.Context = sftpClient.Open(fileName, mode, acs) as Stream;
            }
            catch(Renci.SshNet.Common.SshException)
            {
                return DokanResult.AccessDenied;
            }

            return DokanResult.Success;
        }

        NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus GetFileSecurity(string fileName, out System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus OpenDirectory(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus SetFileSecurity(string fileName, System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus Unmount(DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            throw new NotImplementedException();
        } 
        #endregion

        #region Implementation of IGangsDriver
        string MountPoint
        {
            get { return _mountPoint; }
        }

        bool IsMounted
        {
            get { return _isMounted; }
        }

        void Mount()
        {
            if (IsMounted)
                return;

            sftpClient.Connect();
            if (!sftpClient.IsConnected)
                throw new Exception("cannot connect");

            this._isMounted = true;
            this.Mount(this.MountPoint, DokanOptions.DebugMode, 5);
        }

        void ClearMountPoint()
        {
            if (!IsMounted)
                return;

            Dokan.RemoveMountPoint(this.MountPoint);
            this._isMounted = false;

            if (sftpClient.IsConnected)
            {
                sftpClient.Disconnect();
                sftpClient.Dispose();
            }
        } 
        #endregion
    }
}
