namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class OSVersionInfoEx
    {
        public int osVersionInfoSize;
        public int majorVersion;
        public int minorVersion;
        public int buildNumber;
        public int platformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
        public string csdVersion;
        public short servicePackMajor;
        public short servicePackMinor;
        public short suiteMask;
        public byte productType;
        public byte reserved;
        public OSVersionInfoEx()
        {
            this.osVersionInfoSize = Marshal.SizeOf(this);
        }
    }
}

