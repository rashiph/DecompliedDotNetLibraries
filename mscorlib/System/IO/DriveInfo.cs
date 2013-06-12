namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true)]
    public sealed class DriveInfo : ISerializable
    {
        private string _name;
        private const string NameField = "_name";

        [SecuritySafeCritical]
        public DriveInfo(string driveName)
        {
            if (driveName == null)
            {
                throw new ArgumentNullException("driveName");
            }
            if (driveName.Length == 1)
            {
                this._name = driveName + @":\";
            }
            else
            {
                Path.CheckInvalidPathChars(driveName);
                this._name = Path.GetPathRoot(driveName);
                if (((this._name == null) || (this._name.Length == 0)) || this._name.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDriveLetterOrRootDir"));
                }
            }
            if ((this._name.Length == 2) && (this._name[1] == ':'))
            {
                this._name = this._name + @"\";
            }
            char ch = driveName[0];
            if (((ch < 'A') || (ch > 'Z')) && ((ch < 'a') || (ch > 'z')))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDriveLetterOrRootDir"));
            }
            string path = this._name + '.';
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
        }

        [SecurityCritical]
        private DriveInfo(SerializationInfo info, StreamingContext context)
        {
            this._name = (string) info.GetValue("_name", typeof(string));
            string path = this._name + '.';
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
        }

        [SecuritySafeCritical]
        public static DriveInfo[] GetDrives()
        {
            string[] logicalDrives = Directory.GetLogicalDrives();
            DriveInfo[] infoArray = new DriveInfo[logicalDrives.Length];
            for (int i = 0; i < logicalDrives.Length; i++)
            {
                infoArray[i] = new DriveInfo(logicalDrives[i]);
            }
            return infoArray;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_name", this._name, typeof(string));
        }

        public override string ToString()
        {
            return this.Name;
        }

        public long AvailableFreeSpace
        {
            [SecuritySafeCritical]
            get
            {
                long num;
                int newMode = Win32Native.SetErrorMode(1);
                try
                {
                    long num2;
                    long num3;
                    if (!Win32Native.GetDiskFreeSpaceEx(this.Name, out num, out num2, out num3))
                    {
                        __Error.WinIODriveError(this.Name);
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(newMode);
                }
                return num;
            }
        }

        public string DriveFormat
        {
            [SecuritySafeCritical]
            get
            {
                StringBuilder volumeName = new StringBuilder(50);
                StringBuilder fileSystemName = new StringBuilder(50);
                int newMode = Win32Native.SetErrorMode(1);
                try
                {
                    int num;
                    int num2;
                    int num3;
                    if (!Win32Native.GetVolumeInformation(this.Name, volumeName, 50, out num, out num2, out num3, fileSystemName, 50))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        __Error.WinIODriveError(this.Name, errorCode);
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(newMode);
                }
                return fileSystemName.ToString();
            }
        }

        public System.IO.DriveType DriveType
        {
            [SecuritySafeCritical]
            get
            {
                return (System.IO.DriveType) Win32Native.GetDriveType(this.Name);
            }
        }

        public bool IsReady
        {
            [SecuritySafeCritical]
            get
            {
                return Directory.InternalExists(this.Name);
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public DirectoryInfo RootDirectory
        {
            [SecuritySafeCritical]
            get
            {
                return new DirectoryInfo(this.Name);
            }
        }

        public long TotalFreeSpace
        {
            [SecuritySafeCritical]
            get
            {
                long num3;
                int newMode = Win32Native.SetErrorMode(1);
                try
                {
                    long num;
                    long num2;
                    if (!Win32Native.GetDiskFreeSpaceEx(this.Name, out num, out num2, out num3))
                    {
                        __Error.WinIODriveError(this.Name);
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(newMode);
                }
                return num3;
            }
        }

        public long TotalSize
        {
            [SecuritySafeCritical]
            get
            {
                long num2;
                int newMode = Win32Native.SetErrorMode(1);
                try
                {
                    long num;
                    long num3;
                    if (!Win32Native.GetDiskFreeSpaceEx(this.Name, out num, out num2, out num3))
                    {
                        __Error.WinIODriveError(this.Name);
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(newMode);
                }
                return num2;
            }
        }

        public string VolumeLabel
        {
            [SecuritySafeCritical]
            get
            {
                StringBuilder volumeName = new StringBuilder(50);
                StringBuilder fileSystemName = new StringBuilder(50);
                int newMode = Win32Native.SetErrorMode(1);
                try
                {
                    int num;
                    int num2;
                    int num3;
                    if (!Win32Native.GetVolumeInformation(this.Name, volumeName, 50, out num, out num2, out num3, fileSystemName, 50))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == 13)
                        {
                            errorCode = 15;
                        }
                        __Error.WinIODriveError(this.Name, errorCode);
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(newMode);
                }
                return volumeName.ToString();
            }
            [SecuritySafeCritical]
            set
            {
                string path = this._name + '.';
                new FileIOPermission(FileIOPermissionAccess.Write, path).Demand();
                int newMode = Win32Native.SetErrorMode(1);
                try
                {
                    if (!Win32Native.SetVolumeLabel(this.Name, value))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == 5)
                        {
                            throw new UnauthorizedAccessException(Environment.GetResourceString("InvalidOperation_SetVolumeLabelFailed"));
                        }
                        __Error.WinIODriveError(this.Name, errorCode);
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(newMode);
                }
            }
        }
    }
}

