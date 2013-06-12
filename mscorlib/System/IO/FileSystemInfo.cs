namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true), FileIOPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public abstract class FileSystemInfo : MarshalByRefObject, ISerializable
    {
        [SecurityCritical]
        internal Win32Native.WIN32_FILE_ATTRIBUTE_DATA _data;
        internal int _dataInitialised;
        private string _displayPath;
        internal const int ERROR_ACCESS_DENIED = 5;
        private const int ERROR_INVALID_PARAMETER = 0x57;
        protected string FullPath;
        protected string OriginalPath;

        protected FileSystemInfo()
        {
            this._dataInitialised = -1;
            this._displayPath = "";
        }

        protected FileSystemInfo(SerializationInfo info, StreamingContext context)
        {
            this._dataInitialised = -1;
            this._displayPath = "";
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.FullPath = Path.GetFullPathInternal(info.GetString("FullPath"));
            this.OriginalPath = info.GetString("OriginalPath");
            this._dataInitialised = -1;
        }

        public abstract void Delete();
        [SecurityCritical, ComVisible(false)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, this.FullPath).Demand();
            info.AddValue("OriginalPath", this.OriginalPath, typeof(string));
            info.AddValue("FullPath", this.FullPath, typeof(string));
        }

        [SecurityCritical]
        internal void InitializeFrom(Win32Native.WIN32_FIND_DATA findData)
        {
            this._data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            this._data.PopulateFrom(findData);
            this._dataInitialised = 0;
        }

        [SecuritySafeCritical]
        public void Refresh()
        {
            this._dataInitialised = File.FillAttributeInfo(this.FullPath, ref this._data, false, false);
        }

        public FileAttributes Attributes
        {
            [SecuritySafeCritical]
            get
            {
                if (this._dataInitialised == -1)
                {
                    this._data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
                    this.Refresh();
                }
                if (this._dataInitialised != 0)
                {
                    __Error.WinIOError(this._dataInitialised, this.DisplayPath);
                }
                return (FileAttributes) this._data.fileAttributes;
            }
            [SecuritySafeCritical]
            set
            {
                new FileIOPermission(FileIOPermissionAccess.Write, this.FullPath).Demand();
                if (!Win32Native.SetFileAttributes(this.FullPath, (int) value))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    switch (errorCode)
                    {
                        case 0x57:
                            throw new ArgumentException(Environment.GetResourceString("Arg_InvalidFileAttrs"));

                        case 5:
                            throw new ArgumentException(Environment.GetResourceString("UnauthorizedAccess_IODenied_NoPathName"));
                    }
                    __Error.WinIOError(errorCode, this.DisplayPath);
                }
                this._dataInitialised = -1;
            }
        }

        public DateTime CreationTime
        {
            [SecuritySafeCritical]
            get
            {
                return this.CreationTimeUtc.ToLocalTime();
            }
            set
            {
                this.CreationTimeUtc = value.ToUniversalTime();
            }
        }

        [ComVisible(false)]
        public DateTime CreationTimeUtc
        {
            [SecuritySafeCritical]
            get
            {
                if (this._dataInitialised == -1)
                {
                    this._data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
                    this.Refresh();
                }
                if (this._dataInitialised != 0)
                {
                    __Error.WinIOError(this._dataInitialised, this.DisplayPath);
                }
                long fileTime = (this._data.ftCreationTimeHigh << 0x20) | this._data.ftCreationTimeLow;
                return DateTime.FromFileTimeUtc(fileTime);
            }
            set
            {
                if (this is DirectoryInfo)
                {
                    Directory.SetCreationTimeUtc(this.FullPath, value);
                }
                else
                {
                    File.SetCreationTimeUtc(this.FullPath, value);
                }
                this._dataInitialised = -1;
            }
        }

        internal string DisplayPath
        {
            get
            {
                return this._displayPath;
            }
            set
            {
                this._displayPath = value;
            }
        }

        public abstract bool Exists { get; }

        public string Extension
        {
            get
            {
                int length = this.FullPath.Length;
                int startIndex = length;
                while (--startIndex >= 0)
                {
                    char ch = this.FullPath[startIndex];
                    if (ch == '.')
                    {
                        return this.FullPath.Substring(startIndex, length - startIndex);
                    }
                    if (((ch == Path.DirectorySeparatorChar) || (ch == Path.AltDirectorySeparatorChar)) || (ch == Path.VolumeSeparatorChar))
                    {
                        break;
                    }
                }
                return string.Empty;
            }
        }

        public virtual string FullName
        {
            [SecuritySafeCritical]
            get
            {
                string demandDir;
                if (this is DirectoryInfo)
                {
                    demandDir = Directory.GetDemandDir(this.FullPath, true);
                }
                else
                {
                    demandDir = this.FullPath;
                }
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, demandDir).Demand();
                return this.FullPath;
            }
        }

        public DateTime LastAccessTime
        {
            [SecuritySafeCritical]
            get
            {
                return this.LastAccessTimeUtc.ToLocalTime();
            }
            set
            {
                this.LastAccessTimeUtc = value.ToUniversalTime();
            }
        }

        [ComVisible(false)]
        public DateTime LastAccessTimeUtc
        {
            [SecuritySafeCritical]
            get
            {
                if (this._dataInitialised == -1)
                {
                    this._data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
                    this.Refresh();
                }
                if (this._dataInitialised != 0)
                {
                    __Error.WinIOError(this._dataInitialised, this.DisplayPath);
                }
                long fileTime = (this._data.ftLastAccessTimeHigh << 0x20) | this._data.ftLastAccessTimeLow;
                return DateTime.FromFileTimeUtc(fileTime);
            }
            set
            {
                if (this is DirectoryInfo)
                {
                    Directory.SetLastAccessTimeUtc(this.FullPath, value);
                }
                else
                {
                    File.SetLastAccessTimeUtc(this.FullPath, value);
                }
                this._dataInitialised = -1;
            }
        }

        public DateTime LastWriteTime
        {
            [SecuritySafeCritical]
            get
            {
                return this.LastWriteTimeUtc.ToLocalTime();
            }
            set
            {
                this.LastWriteTimeUtc = value.ToUniversalTime();
            }
        }

        [ComVisible(false)]
        public DateTime LastWriteTimeUtc
        {
            [SecuritySafeCritical]
            get
            {
                if (this._dataInitialised == -1)
                {
                    this._data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
                    this.Refresh();
                }
                if (this._dataInitialised != 0)
                {
                    __Error.WinIOError(this._dataInitialised, this.DisplayPath);
                }
                long fileTime = (this._data.ftLastWriteTimeHigh << 0x20) | this._data.ftLastWriteTimeLow;
                return DateTime.FromFileTimeUtc(fileTime);
            }
            set
            {
                if (this is DirectoryInfo)
                {
                    Directory.SetLastWriteTimeUtc(this.FullPath, value);
                }
                else
                {
                    File.SetLastWriteTimeUtc(this.FullPath, value);
                }
                this._dataInitialised = -1;
            }
        }

        public abstract string Name { get; }
    }
}

