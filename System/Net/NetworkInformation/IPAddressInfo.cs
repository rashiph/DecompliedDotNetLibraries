namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IPAddressInfo
    {
        internal IPAddress addr;
        internal IPAddress mask;
        internal uint context;
    }
}

