namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class PartialDnsRecord
    {
        public IntPtr next;
        public string name;
        public short type;
        public short dataLength;
        public int flags;
        public int ttl;
        public int reserved;
        public IntPtr data;
    }
}

