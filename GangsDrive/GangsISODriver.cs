using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DokanNet;
using System.IO;
using FileAccess = DokanNet.FileAccess;

namespace GangsDrive
{
    class GangsISODriver : IDokanOperations
    {
        private string isoPath;

        public GangsISODriver(string isoPath)
        {
            if (!File.Exists(isoPath))
                throw new ArgumentException("file not found");

            this.isoPath = isoPath;
        }

        #region Implementation of IDokanOperations

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus OpenDirectory(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus CreateDirectory(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public void Cleanup(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetFileSecurity(string fileName, out System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileSecurity(string fileName, System.Security.AccessControl.FileSystemSecurity security, System.Security.AccessControl.AccessControlSections sections, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus Unmount(DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}
