namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FIXED_INFO
    {
        internal const int MAX_HOSTNAME_LEN = 0x80;
        internal const int MAX_DOMAIN_NAME_LEN = 0x80;
        internal const int MAX_SCOPE_ID_LEN = 0x100;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x84)]
        internal string hostName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x84)]
        internal string domainName;
        internal uint currentDnsServer;
        internal IpAddrString DnsServerList;
        internal NetBiosNodeType nodeType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
        internal string scopeId;
        internal bool enableRouting;
        internal bool enableProxy;
        internal bool enableDns;
    }
}

