namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Icmp6EchoReply
    {
        internal Ipv6Address Address;
        internal uint Status;
        internal uint RoundTripTime;
        internal IntPtr data;
    }
}

