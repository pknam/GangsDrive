using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.Threading.Tasks;

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

namespace GangsDrive.connector
{
    class GangsGoogleDriver : IDokanOperations, IGangsDriver
    {
        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                 FileAccess.Execute |
                                 FileAccess.GenericExecute | FileAccess.GenericWrite | FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        private readonly string _mountPoint;
        private readonly string _driverName = "Google";
        private bool _isMounted;
        public event EventHandler<connector.MountChangedArgs> OnMountChangedEvent;

        private const string CredentialPath = "../credentials";
        private string[] Scopes = { 
                                      DriveService.Scope.Drive, 
                                      DriveService.Scope.DriveFile, 
                                      DriveService.Scope.DriveMetadata 
                                  };
        private const string ApplicationName = "GangsDrive";

        private UserCredential _userCredential;
        private DriveService _driveService;

        public GangsGoogleDriver(string mountPoint)
        {
            this._mountPoint = mountPoint;
        }

        #region Implementation of IDokanOperations
        public void Cleanup(string fileName, DokanFileInfo info)
        {
            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
            }
            info.Context = null;
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
            }
            info.Context = null;
        }

        public NtStatus CreateDirectory(string fileName, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return DokanResult.Success;
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
            files = null;
            return DokanResult.Success;
        }

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            if (info.Context != null)
            {
                (info.Context as FileStream).Flush();

                return DokanResult.Success;
            }
            else return DokanResult.Unsuccessful;
            
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
            fileInfo = new FileInformation();            
            return DokanResult.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            security = null;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = _userCredential.UserId;
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
            return DokanResult.Success;
        }

        public NtStatus OpenDirectory(string fileName, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            try
            {
                if (info.Context != null)
                {
                    using (var stream = new FileStream(fileName, FileMode.Open, System.IO.FileAccess.Read))
                    {
                        stream.Position = offset;
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    FileStream stream = info.Context as FileStream;

                    lock (stream)
                    {
                        stream.Position = offset;
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                    }
                }

                return DokanResult.Success;
            }
            catch (FileNotFoundException e)
            {
                GangsDrive.util.DriverError.DebugError(e, _driverName, _isMounted);
                bytesRead = 0;
                return DokanResult.FileNotFound;
            }
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            if (info.Context != null)
            {
                (info.Context as FileStream).SetLength(length);
                return DokanResult.Success;
            }
            else return DokanResult.Unsuccessful;
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            if (info.Context != null)
            {
                (info.Context as FileStream).SetLength(length);
                return DokanResult.Success;
            }
            else return DokanResult.Unsuccessful;
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
            return DokanResult.Success;
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

            using (var stream = new FileStream("google_secret.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(CredentialPath, true));
                _userCredential = credentials.Result;
                if (credentials.IsCanceled || credentials.IsFaulted)
                    throw new Exception("cannot connect");

                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _userCredential,
                    ApplicationName = ApplicationName,
                });
            }

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

            _driveService.Dispose();
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
