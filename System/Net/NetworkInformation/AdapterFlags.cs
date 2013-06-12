namespace System.Net.NetworkInformation
{
    using System;

    [Flags]
    internal enum AdapterFlags
    {
        DhcpEnabled = 4,
        DnsEnabled = 1,
        Ipv6OtherStatefulConfig = 0x20,
        NoMulticast = 0x10,
        ReceiveOnly = 8,
        RegisterAdapterSuffix = 2
    }
}

