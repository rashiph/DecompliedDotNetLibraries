namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public sealed class DirectoryInfo : FileSystemInfo
    {
        private string[] demandDir;

        [SecuritySafeCritical]
        public DirectoryInfo(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if ((path.Length == 2) && (path[1] == ':'))
            {
                base.OriginalPath = ".";
            }
            else
            {
                base.OriginalPath = path;
            }
            string fullPathInternal = Path.GetFullPathInternal(path);
            this.demandDir = new string[] { Directory.GetDemandDir(fullPathInternal, true) };
            new FileIOPermission(FileIOPermissionAccess.Read, this.demandDir, false, false).Demand();
            base.FullPath = fullPathInternal;
            base.DisplayPath = GetDisplayName(base.OriginalPath, base.FullPath);
        }

        [SecurityCritical]
        private DirectoryInfo(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.demandDir = new string[] { Directory.GetDemandDir(base.FullPath, true) };
            new FileIOPermission(FileIOPermissionAccess.Read, this.demandDir, false, false).Demand();
            base.DisplayPath = GetDisplayName(base.OriginalPath, base.FullPath);
        }

        internal DirectoryInfo(string fullPath, bool junk)
        {
            base.OriginalPath = Path.GetFileName(fullPath);
            base.FullPath = fullPath;
            base.DisplayPath = GetDisplayName(base.OriginalPath, base.FullPath);
            this.demandDir = new string[] { Directory.GetDemandDir(fullPath, true) };
        }

        public void Create()
        {
            Directory.InternalCreateDirectory(base.FullPath, base.OriginalPath, null);
        }

        public void Create(DirectorySecurity directorySecurity)
        {
            Directory.InternalCreateDirectory(base.FullPath, base.OriginalPath, directorySecurity);
        }

        [SecuritySafeCritical]
        public DirectoryInfo CreateSubdirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return this.CreateSubdirectory(path, null);
        }

        [SecuritySafeCritical]
        public DirectoryInfo CreateSubdirectory(string path, DirectorySecurity directorySecurity)
        {
            return this.CreateSubdirectoryHelper(path, directorySecurity);
        }

        [SecurityCritical]
        private DirectoryInfo CreateSubdirectoryHelper(string path, object directorySecurity)
        {
            string fullPathInternal = Path.GetFullPathInternal(Path.InternalCombine(base.FullPath, path));
            if (string.Compare(base.FullPath, 0, fullPathInternal, 0, base.FullPath.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                string displayablePath = __Error.GetDisplayablePath(base.DisplayPath, false);
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSubPath", new object[] { path, displayablePath }));
            }
            string demandDir = Directory.GetDemandDir(fullPathInternal, true);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { demandDir }, false, false).Demand();
            Directory.InternalCreateDirectory(fullPathInternal, path, directorySecurity);
            return new DirectoryInfo(fullPathInternal);
        }

        [SecuritySafeCritical]
        public override void Delete()
        {
            Directory.Delete(base.FullPath, base.OriginalPath, false);
        }

        [SecuritySafeCritical]
        public void Delete(bool recursive)
        {
            Directory.Delete(base.FullPath, base.OriginalPath, recursive);
        }

        [SecuritySafeCritical]
        public IEnumerable<DirectoryInfo> EnumerateDirectories()
        {
            return this.InternalEnumerateDirectories("*", SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            return this.InternalEnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            return this.InternalEnumerateDirectories(searchPattern, searchOption);
        }

        [SecuritySafeCritical]
        public IEnumerable<FileInfo> EnumerateFiles()
        {
            return this.InternalEnumerateFiles("*", SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            return this.InternalEnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            return this.InternalEnumerateFiles(searchPattern, searchOption);
        }

        [SecuritySafeCritical]
        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
        {
            return this.InternalEnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            return this.InternalEnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            return this.InternalEnumerateFileSystemInfos(searchPattern, searchOption);
        }

        public DirectorySecurity GetAccessControl()
        {
            return Directory.GetAccessControl(base.FullPath, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
        {
            return Directory.GetAccessControl(base.FullPath, includeSections);
        }

        [SecuritySafeCritical]
        public DirectoryInfo[] GetDirectories()
        {
            return this.InternalGetDirectories("*", SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public DirectoryInfo[] GetDirectories(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            return this.InternalGetDirectories(searchPattern, SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            return this.InternalGetDirectories(searchPattern, searchOption);
        }

        private static string GetDirName(string fullPath)
        {
            if (fullPath.Length > 3)
            {
                string path = fullPath;
                if (fullPath.EndsWith(Path.DirectorySeparatorChar))
                {
                    path = fullPath.Substring(0, fullPath.Length - 1);
                }
                return Path.GetFileName(path);
            }
            return fullPath;
        }

        private static string GetDisplayName(string originalPath, string fullPath)
        {
            if ((originalPath.Length == 2) && (originalPath[1] == ':'))
            {
                return ".";
            }
            return originalPath;
        }

        [SecuritySafeCritical]
        public FileInfo[] GetFiles()
        {
            return this.InternalGetFiles("*", SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public FileInfo[] GetFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            return this.InternalGetFiles(searchPattern, SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            return this.InternalGetFiles(searchPattern, searchOption);
        }

        [SecuritySafeCritical]
        public FileSystemInfo[] GetFileSystemInfos()
        {
            return this.InternalGetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            return this.InternalGetFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
        }

        [SecuritySafeCritical]
        public FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            return this.InternalGetFileSystemInfos(searchPattern, searchOption);
        }

        private IEnumerable<DirectoryInfo> InternalEnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            return FileSystemEnumerableFactory.CreateDirectoryInfoIterator(base.FullPath, base.OriginalPath, searchPattern, searchOption);
        }

        private IEnumerable<FileInfo> InternalEnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            return FileSystemEnumerableFactory.CreateFileInfoIterator(base.FullPath, base.OriginalPath, searchPattern, searchOption);
        }

        private IEnumerable<FileSystemInfo> InternalEnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return FileSystemEnumerableFactory.CreateFileSystemInfoIterator(base.FullPath, base.OriginalPath, searchPattern, searchOption);
        }

        private DirectoryInfo[] InternalGetDirectories(string searchPattern, SearchOption searchOption)
        {
            List<DirectoryInfo> list = new List<DirectoryInfo>(FileSystemEnumerableFactory.CreateDirectoryInfoIterator(base.FullPath, base.OriginalPath, searchPattern, searchOption));
            return list.ToArray();
        }

        private FileInfo[] InternalGetFiles(string searchPattern, SearchOption searchOption)
        {
            List<FileInfo> list = new List<FileInfo>(FileSystemEnumerableFactory.CreateFileInfoIterator(base.FullPath, base.OriginalPath, searchPattern, searchOption));
            return list.ToArray();
        }

        private FileSystemInfo[] InternalGetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            List<FileSystemInfo> list = new List<FileSystemInfo>(FileSystemEnumerableFactory.CreateFileSystemInfoIterator(base.FullPath, base.OriginalPath, searchPattern, searchOption));
            return list.ToArray();
        }

        [SecuritySafeCritical]
        public void MoveTo(string destDirName)
        {
            string fullPath;
            if (destDirName == null)
            {
                throw new ArgumentNullException("destDirName");
            }
            if (destDirName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destDirName");
            }
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, this.demandDir, false, false).Demand();
            string fullPathInternal = Path.GetFullPathInternal(destDirName);
            if (!fullPathInternal.EndsWith(Path.DirectorySeparatorChar))
            {
                fullPathInternal = fullPathInternal + Path.DirectorySeparatorChar;
            }
            string path = fullPathInternal + '.';
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, path).Demand();
            if (base.FullPath.EndsWith(Path.DirectorySeparatorChar))
            {
                fullPath = base.FullPath;
            }
            else
            {
                fullPath = base.FullPath + Path.DirectorySeparatorChar;
            }
            if (string.Compare(fullPath, fullPathInternal, StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustBeDifferent"));
            }
            string pathRoot = Path.GetPathRoot(fullPath);
            string strB = Path.GetPathRoot(fullPathInternal);
            if (string.Compare(pathRoot, strB, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustHaveSameRoot"));
            }
            if (!Win32Native.MoveFile(base.FullPath, destDirName))
            {
                int errorCode = Marshal.GetLastWin32Error();
                switch (errorCode)
                {
                    case 2:
                        errorCode = 3;
                        __Error.WinIOError(errorCode, base.DisplayPath);
                        break;

                    case 5:
                        throw new IOException(Environment.GetResourceString("UnauthorizedAccess_IODenied_Path", new object[] { base.DisplayPath }));
                }
                __Error.WinIOError(errorCode, string.Empty);
            }
            base.FullPath = fullPathInternal;
            base.OriginalPath = destDirName;
            base.DisplayPath = GetDisplayName(base.OriginalPath, base.FullPath);
            this.demandDir = new string[] { Directory.GetDemandDir(base.FullPath, true) };
            base._dataInitialised = -1;
        }

        [SecuritySafeCritical]
        public void SetAccessControl(DirectorySecurity directorySecurity)
        {
            Directory.SetAccessControl(base.FullPath, directorySecurity);
        }

        public override string ToString()
        {
            return base.DisplayPath;
        }

        public override bool Exists
        {
            [SecuritySafeCritical]
            get
            {
                try
                {
                    if (base._dataInitialised == -1)
                    {
                        base.Refresh();
                    }
                    if (base._dataInitialised != 0)
                    {
                        return false;
                    }
                    return ((this._data.fileAttributes != -1) && ((this._data.fileAttributes & 0x10) != 0));
                }
                catch
                {
                    return false;
                }
            }
        }

        public override string Name
        {
            get
            {
                return GetDirName(base.FullPath);
            }
        }

        public DirectoryInfo Parent
        {
            [SecuritySafeCritical]
            get
            {
                string fullPath = base.FullPath;
                if ((fullPath.Length > 3) && fullPath.EndsWith(Path.DirectorySeparatorChar))
                {
                    fullPath = base.FullPath.Substring(0, base.FullPath.Length - 1);
                }
                string directoryName = Path.GetDirectoryName(fullPath);
                if (directoryName == null)
                {
                    return null;
                }
                DirectoryInfo info = new DirectoryInfo(directoryName, false);
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, info.demandDir, false, false).Demand();
                return info;
            }
        }

        public DirectoryInfo Root
        {
            [SecuritySafeCritical]
            get
            {
                int rootLength = Path.GetRootLength(base.FullPath);
                string fullPath = base.FullPath.Substring(0, rootLength);
                string demandDir = Directory.GetDemandDir(fullPath, true);
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { demandDir }, false, false).Demand();
                return new DirectoryInfo(fullPath);
            }
        }
    }
}

