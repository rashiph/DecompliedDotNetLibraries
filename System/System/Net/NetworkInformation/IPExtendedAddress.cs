namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IPExtendedAddress
    {
        internal IPAddress mask;
        internal IPAddress address;
        internal IPExtendedAddress(IPAddress address, IPAddress mask)
        {
            this.address = address;
            this.mask = mask;
        }
    }
}

