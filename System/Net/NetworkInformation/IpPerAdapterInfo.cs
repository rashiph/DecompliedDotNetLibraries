namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IpPerAdapterInfo
    {
        internal bool autoconfigEnabled;
        internal bool autoconfigActive;
        internal IntPtr currentDnsServer;
        internal IpAddrString dnsServerList;
    }
}

