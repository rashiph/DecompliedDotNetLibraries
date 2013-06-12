namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIcmpInfo
    {
        internal MibIcmpStats inStats;
        internal MibIcmpStats outStats;
    }
}

