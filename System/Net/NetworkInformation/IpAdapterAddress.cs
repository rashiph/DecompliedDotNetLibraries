namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IpAdapterAddress
    {
        internal uint length;
        internal AdapterAddressFlags flags;
        internal IntPtr next;
        internal IpSocketAddress address;
    }
}

