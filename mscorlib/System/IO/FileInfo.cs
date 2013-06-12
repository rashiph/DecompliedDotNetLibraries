namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true)]
    public sealed class FileInfo : FileSystemInfo
    {
        private string _name;

        [SecuritySafeCritical]
        public FileInfo(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            base.OriginalPath = fileName;
            string fullPathInternal = Path.GetFullPathInternal(fileName);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            this._name = Path.GetFileName(fileName);
            base.FullPath = fullPathInternal;
            base.DisplayPath = this.GetDisplayPath(fileName);
        }

        [SecurityCritical]
        private FileInfo(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { base.FullPath }, false, false).Demand();
            this._name = Path.GetFileName(base.OriginalPath);
            base.DisplayPath = this.GetDisplayPath(base.OriginalPath);
        }

        internal FileInfo(string fullPath, bool ignoreThis)
        {
            this._name = Path.GetFileName(fullPath);
            base.OriginalPath = this._name;
            base.FullPath = fullPath;
            base.DisplayPath = this._name;
        }

        [SecuritySafeCritical]
        public StreamWriter AppendText()
        {
            return new StreamWriter(base.FullPath, true);
        }

        public FileInfo CopyTo(string destFileName)
        {
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destFileName");
            }
            destFileName = File.InternalCopy(base.FullPath, destFileName, false);
            return new FileInfo(destFileName, false);
        }

        public FileInfo CopyTo(string destFileName, bool overwrite)
        {
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destFileName");
            }
            destFileName = File.InternalCopy(base.FullPath, destFileName, overwrite);
            return new FileInfo(destFileName, false);
        }

        [SecuritySafeCritical]
        public FileStream Create()
        {
            return File.Create(base.FullPath);
        }

        [SecuritySafeCritical]
        public StreamWriter CreateText()
        {
            return new StreamWriter(base.FullPath, false);
        }

        [ComVisible(false)]
        public void Decrypt()
        {
            File.Decrypt(base.FullPath);
        }

        [SecuritySafeCritical]
        public override void Delete()
        {
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { base.FullPath }, false, false).Demand();
            if (!Win32Native.DeleteFile(base.FullPath))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 2)
                {
                    __Error.WinIOError(errorCode, base.DisplayPath);
                }
            }
        }

        [ComVisible(false)]
        public void Encrypt()
        {
            File.Encrypt(base.FullPath);
        }

        public FileSecurity GetAccessControl()
        {
            return File.GetAccessControl(base.FullPath, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        public FileSecurity GetAccessControl(AccessControlSections includeSections)
        {
            return File.GetAccessControl(base.FullPath, includeSections);
        }

        private string GetDisplayPath(string originalPath)
        {
            return originalPath;
        }

        [SecuritySafeCritical]
        public void MoveTo(string destFileName)
        {
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destFileName");
            }
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { base.FullPath }, false, false).Demand();
            string fullPathInternal = Path.GetFullPathInternal(destFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { fullPathInternal }, false, false).Demand();
            if (!Win32Native.MoveFile(base.FullPath, fullPathInternal))
            {
                __Error.WinIOError();
            }
            base.FullPath = fullPathInternal;
            base.OriginalPath = destFileName;
            this._name = Path.GetFileName(fullPathInternal);
            base.DisplayPath = this.GetDisplayPath(destFileName);
            base._dataInitialised = -1;
        }

        [SecuritySafeCritical]
        public FileStream Open(FileMode mode)
        {
            return this.Open(mode, FileAccess.ReadWrite, FileShare.None);
        }

        [SecuritySafeCritical]
        public FileStream Open(FileMode mode, FileAccess access)
        {
            return this.Open(mode, access, FileShare.None);
        }

        [SecuritySafeCritical]
        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(base.FullPath, mode, access, share);
        }

        [SecuritySafeCritical]
        public FileStream OpenRead()
        {
            return new FileStream(base.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        [SecuritySafeCritical]
        public StreamReader OpenText()
        {
            return new StreamReader(base.FullPath, Encoding.UTF8, true, 0x400);
        }

        [SecuritySafeCritical]
        public FileStream OpenWrite()
        {
            return new FileStream(base.FullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        [ComVisible(false), SecuritySafeCritical]
        public FileInfo Replace(string destinationFileName, string destinationBackupFileName)
        {
            return this.Replace(destinationFileName, destinationBackupFileName, false);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public FileInfo Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            File.Replace(base.FullPath, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
            return new FileInfo(destinationFileName);
        }

        [SecuritySafeCritical]
        public void SetAccessControl(FileSecurity fileSecurity)
        {
            File.SetAccessControl(base.FullPath, fileSecurity);
        }

        public override string ToString()
        {
            return base.DisplayPath;
        }

        public DirectoryInfo Directory
        {
            [SecuritySafeCritical]
            get
            {
                string directoryName = this.DirectoryName;
                if (directoryName == null)
                {
                    return null;
                }
                return new DirectoryInfo(directoryName);
            }
        }

        public string DirectoryName
        {
            [SecuritySafeCritical]
            get
            {
                string directoryName = Path.GetDirectoryName(base.FullPath);
                if (directoryName != null)
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { directoryName }, false, false).Demand();
                }
                return directoryName;
            }
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
                    return ((this._data.fileAttributes & 0x10) == 0);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsReadOnly
        {
            [SecuritySafeCritical]
            get
            {
                return ((base.Attributes & FileAttributes.ReadOnly) != 0);
            }
            [SecuritySafeCritical]
            set
            {
                if (value)
                {
                    base.Attributes |= FileAttributes.ReadOnly;
                }
                else
                {
                    base.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
        }

        public long Length
        {
            [SecuritySafeCritical]
            get
            {
                if (base._dataInitialised == -1)
                {
                    base.Refresh();
                }
                if (base._dataInitialised != 0)
                {
                    __Error.WinIOError(base._dataInitialised, base.DisplayPath);
                }
                if ((this._data.fileAttributes & 0x10) != 0)
                {
                    __Error.WinIOError(2, base.DisplayPath);
                }
                return ((this._data.fileSizeHigh << 0x20) | (this._data.fileSizeLow & ((long) 0xffffffffL)));
            }
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

