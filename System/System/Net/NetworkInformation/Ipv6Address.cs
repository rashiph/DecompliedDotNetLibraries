namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct Ipv6Address
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
        internal byte[] Goo;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal byte[] Address;
        internal uint ScopeID;
    }
}

