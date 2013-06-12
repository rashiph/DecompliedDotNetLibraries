namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IcmpEchoReply
    {
        internal uint address;
        internal uint status;
        internal uint roundTripTime;
        internal ushort dataSize;
        internal ushort reserved;
        internal IntPtr data;
        internal IPOptions options;
    }
}

