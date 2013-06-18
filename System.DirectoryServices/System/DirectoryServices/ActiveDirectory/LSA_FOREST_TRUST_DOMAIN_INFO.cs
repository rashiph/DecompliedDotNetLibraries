namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LSA_FOREST_TRUST_DOMAIN_INFO
    {
        public IntPtr sid;
        public short DNSNameLength;
        public short DNSNameMaximumLength;
        public IntPtr DNSNameBuffer;
        public short NetBIOSNameLength;
        public short NetBIOSNameMaximumLength;
        public IntPtr NetBIOSNameBuffer;
    }
}

