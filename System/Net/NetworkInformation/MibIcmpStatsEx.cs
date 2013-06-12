namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIcmpStatsEx
    {
        internal uint dwMsgs;
        internal uint dwErrors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
        internal uint[] rgdwTypeCount;
    }
}

