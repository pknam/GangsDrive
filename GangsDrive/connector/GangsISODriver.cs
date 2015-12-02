using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DokanNet;
using System.IO;
using FileAccess = DokanNet.FileAccess;
using DiscUtils.Iso9660;
using System.Windows.Forms;

namespace GangsDrive
{
    class GangsISODriver : GangsDriver, IDokanOperations
    {
        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute | FileAccess.GenericWrite | FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        private string isoPath;
        private FileStream isoFileStream;
        private CDReader isoReader;

        public GangsISODriver(string isoPath, string mountPoint)
            :base(mountPoint, "ISO")
        {

            if (!File.Exists(isoPath))
                throw new FileNotFoundException();

            this.isoPath = isoPath;
            this.isoFileStream = null;
            this.isoReader = null;
        }

        #region Implementation of IDokanOperations

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            bool pathExists = this.isoReader.Exists(fileName);
            bool pathIsDirectory = this.isoReader.DirectoryExists(fileName);

            bool readWriteAttributes = (access & DataAccess) == 0;
            bool readAccess = (access & DataWriteAccess) == 0;

            switch(mode)
            {
                case FileMode.Append:
                case FileMode.OpenOrCreate:
                case FileMode.Open:
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
                            info.Context = this.isoReader.OpenFile(fileName, mode, System.IO.FileAccess.Read) as Stream;
                        }
                    }
                    else
                    {
                        return DokanResult.FileNotFound;
                    }

                    break;

                case FileMode.Truncate:
                    if (!pathExists)
                        return DokanResult.FileNotFound;
                    
                    return DokanResult.Error;

                case FileMode.CreateNew:
                    if (pathExists)
                        return DokanResult.AlreadyExists;

                    return DokanResult.Error;

                case FileMode.Create:
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
            if (!this.isoReader.DirectoryExists(fileName))
                return DokanResult.FileExists;

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
            ((Stream)(info.Context)).Flush();
            return DokanResult.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            DiscUtils.DiscFileSystemInfo finfo;

            if (this.isoReader.DirectoryExists(fileName))
                finfo = this.isoReader.GetDirectoryInfo(fileName);
            else if (this.isoReader.FileExists(fileName))
                finfo = this.isoReader.GetFileInfo(fileName);
            else
            {
                fileInfo = new FileInformation();
                return DokanResult.FileNotFound;
            }

            fileInfo = new FileInformation
            {
                FileName = fileName,
                Attributes = finfo.Attributes,
                CreationTime = finfo.CreationTime,
                LastAccessTime = finfo.LastAccessTime,
                LastWriteTime = finfo.LastAccessTime,
                Length = (finfo is DiscUtils.DiscDirectoryInfo) ? 0 : ((DiscUtils.DiscFileInfo)finfo).Length,
            };

            return DokanResult.Success;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            string[] fileList = this.isoReader.GetFiles(fileName);
            string[] dirList = this.isoReader.GetDirectories(fileName);
            files = new List<FileInformation>();

            foreach(var file in fileList)
            {
                DiscUtils.DiscFileInfo srcFileInfo = this.isoReader.GetFileInfo(file);
                FileInformation finfo = new FileInformation();

                finfo.FileName = Path.GetFileName(file);
                finfo.Attributes = srcFileInfo.Attributes;
                finfo.CreationTime = srcFileInfo.CreationTime;
                finfo.LastAccessTime = srcFileInfo.LastAccessTime;
                finfo.LastWriteTime = srcFileInfo.LastWriteTime;
                finfo.Length = srcFileInfo.Length;

                files.Add(finfo);
            }

            foreach(var dir in dirList)
            {
                DiscUtils.DiscDirectoryInfo srcDirInfo = this.isoReader.GetDirectoryInfo(dir);
                FileInformation finfo = new FileInformation();

                finfo.FileName = Path.GetFileName(dir);
                finfo.Attributes = srcDirInfo.Attributes;
                finfo.CreationTime = srcDirInfo.CreationTime;
                finfo.LastAccessTime = srcDirInfo.LastAccessTime;
                finfo.LastWriteTime = srcDirInfo.LastWriteTime;

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
            volumeLabel = Path.GetFileNameWithoutExtension(isoPath);
            fileSystemName = "GangsDrive";

            features = FileSystemFeatures.None;

            return DokanResult.Success;
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

        #region Overriding of GangsDriver


        public override void ClearMountPoint()
        {
            if (!IsMounted)
                return;

            base.ClearMountPoint();


            if(this.isoReader != null)
                this.isoReader.Dispose();
                
            if(this.isoFileStream != null)
                this.isoFileStream.Dispose();
        }

        public override void Mount()
        {
            if (IsMounted)
                return;

            if (!File.Exists(this.isoPath))
                throw new FileNotFoundException();

            this.isoFileStream = File.Open(isoPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.None);
            this.isoReader = new CDReader(this.isoFileStream, true);

            base.Mount();
        }

        #endregion
    }
}
