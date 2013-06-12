namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IpAdapterPrefix
    {
        internal uint length;
        internal uint ifIndex;
        internal IntPtr next;
        internal IpSocketAddress address;
        internal uint prefixLength;
    }
}

