using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DokanNet;
using System.IO;
using FileAccess = DokanNet.FileAccess;
using DiscUtils.Iso9660;

namespace GangsDrive
{
    class GangsISODriver : IDokanOperations, IGangsDriver
    {
        private string isoPath;
        private FileStream isoFileStream;
        private CDReader isoReader;

        private readonly string mountPoint;
        private bool _isMounted;

        public GangsISODriver(string isoPath, string mountPoint)
        {
            if (!File.Exists(isoPath))
                throw new ArgumentException("file not found");

            this.isoPath = isoPath;
            this.isoFileStream = File.Open(isoPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.None);
            this.isoReader = new CDReader(this.isoFileStream, true);
            this.mountPoint = mountPoint;
        }

        #region Implementation of IDokanOperations

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            bool pathExists = this.isoReader.Exists(fileName);
            bool pathIsDirectory = this.isoReader.DirectoryExists(fileName);

            if(mode == FileMode.Open)
            {
                if (pathExists)
                {
                    if (pathIsDirectory)
                    {
                        info.IsDirectory = true;
                        info.Context = new object();
                    }
                    else
                    {
                        info.IsDirectory = false;
                        info.Context = this.isoReader.OpenFile(fileName, FileMode.Open) as Stream;
                    }
                }
                else
                {
                    return DokanResult.FileNotFound;
                }
            }
            else
            {
                return DokanResult.Error;
            }

            return DokanResult.Success;
        }

        public NtStatus OpenDirectory(string fileName, DokanFileInfo info)
        {
            if (!this.isoReader.DirectoryExists(fileName))
            {
                return DokanResult.PathNotFound;
            }

            return DokanResult.Success;
        }

        public NtStatus CreateDirectory(string fileName, DokanFileInfo info)
        {
            // read-only
            return DokanResult.AccessDenied;
        }

        public void Cleanup(string fileName, DokanFileInfo info)
        {
            if (info.Context != null && info.Context is Stream)
            {
                (info.Context as Stream).Dispose();
            }
            info.Context = null;

            if (info.DeleteOnClose)
            {
                // do nothig
            }
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
            if (info.Context != null && info.Context is Stream)
            {
                (info.Context as Stream).Dispose();
            }
            info.Context = null;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            if (info.Context == null)
            {
                using (Stream stream = this.isoReader.OpenFile(fileName, FileMode.Open, System.IO.FileAccess.Read))
                {
                    stream.Position = offset;
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }
            else
            {
                Stream stream = info.Context as Stream;
                stream.Position = offset;
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }

            return DokanResult.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanResult.Error;
        }

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        // check
        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            if(this.isoReader.DirectoryExists(fileName))
            {
                DiscUtils.DiscDirectoryInfo discDirInfo = this.isoReader.GetDirectoryInfo(fileName);

                fileInfo = new FileInformation
                {
                    FileName = fileName,
                    Attributes = discDirInfo.Attributes,
                    CreationTime = discDirInfo.CreationTime,
                    LastAccessTime = discDirInfo.LastAccessTime,
                    LastWriteTime = discDirInfo.LastAccessTime,
                    Length = 0,
                };
            }
            else if(this.isoReader.FileExists(fileName))
            {
                DiscUtils.DiscFileInfo discFileInfo = this.isoReader.GetFileInfo(fileName);

                fileInfo = new FileInformation
                {
                    FileName = fileName,
                    Attributes = discFileInfo.Attributes,
                    CreationTime = discFileInfo.CreationTime,
                    LastAccessTime = discFileInfo.LastAccessTime,
                    LastWriteTime = discFileInfo.LastAccessTime,
                    Length = discFileInfo.Length,
                };
            }
            else
            {
                fileInfo = new FileInformation();
                return DokanResult.FileNotFound;
            }

            return DokanResult.Success;
        }

        // check
        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            string[] fileList = this.isoReader.GetFiles(fileName);
            string[] dirList = this.isoReader.GetDirectories(fileName);

            files = new List<FileInformation>();

            foreach(var file in fileList)
            {
                FileInformation finfo = new FileInformation();

                finfo.FileName = Path.GetFileName(file);
                finfo.Attributes = FileAttributes.Normal;
                finfo.CreationTime = DateTime.Now;
                finfo.LastAccessTime = DateTime.Now;
                finfo.LastWriteTime = DateTime.Now;

                files.Add(finfo);
            }

            foreach(var dir in dirList)
            {
                FileInformation finfo = new FileInformation();

                finfo.FileName = Path.GetFileName(dir);
                finfo.Attributes = FileAttributes.Directory;
                finfo.CreationTime = DateTime.Now;
                finfo.LastAccessTime = DateTime.Now;
                finfo.LastWriteTime = DateTime.Now;

                files.Add(finfo);
            }



            return DokanResult.Success;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
        {
            freeBytesAvailable = 512 * 1024 * 1024;
            totalNumberOfBytes = 1024 * 1024 * 1024;
            totalNumberOfFreeBytes= 512 * 1024 * 1024;

            return DokanResult.Success;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "Gangs";
            fileSystemName = "Gangs";
            features = FileSystemFeatures.None;

            return DokanResult.Error;
        }

        public NtStatus GetFileSecurity(string fileName, out System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            security = null;
            return DokanResult.Error;
        }

        public NtStatus SetFileSecurity(string fileName, System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus Unmount(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            streamName = String.Empty;
            streamSize = 0;
            return DokanResult.NotImplemented;
        } 
        #endregion

        #region Implementation of IGangsDriver

        public bool IsMounted
        {
            get
            {
                return _isMounted;
            }
        }

        public string MountPoint
        {
            get
            {
                return this.mountPoint;
            }
        }

        public void ClearMountPoint()
        {
            if (IsMounted)
            {
                Dokan.RemoveMountPoint(this.MountPoint);

                if(this.isoReader != null)
                    this.isoReader.Dispose();
                
                if(this.isoFileStream != null)
                    this.isoFileStream.Dispose();

                this._isMounted = false;
            }
        }

        public void Mount()
        {
            if (IsMounted)
                return;

            this._isMounted = true;
            this.Mount("i:\\", DokanOptions.DebugMode, 5);
        }

        #endregion
    }

}
