namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct hostent
    {
        public IntPtr h_name;
        public IntPtr h_aliases;
        public short h_addrtype;
        public short h_length;
        public IntPtr h_addr_list;
    }
}

