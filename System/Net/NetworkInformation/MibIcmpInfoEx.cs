namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIcmpInfoEx
    {
        internal MibIcmpStatsEx inStats;
        internal MibIcmpStatsEx outStats;
    }
}

