namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IpSocketAddress
    {
        internal IntPtr address;
        internal int addressLength;
    }
}

