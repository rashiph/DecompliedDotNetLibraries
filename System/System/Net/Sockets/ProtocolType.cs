namespace System.Net.Sockets
{
    using System;

    public enum ProtocolType
    {
        Ggp = 3,
        Icmp = 1,
        IcmpV6 = 0x3a,
        Idp = 0x16,
        Igmp = 2,
        IP = 0,
        IPSecAuthenticationHeader = 0x33,
        IPSecEncapsulatingSecurityPayload = 50,
        IPv4 = 4,
        IPv6 = 0x29,
        IPv6DestinationOptions = 60,
        IPv6FragmentHeader = 0x2c,
        IPv6HopByHopOptions = 0,
        IPv6NoNextHeader = 0x3b,
        IPv6RoutingHeader = 0x2b,
        Ipx = 0x3e8,
        ND = 0x4d,
        Pup = 12,
        Raw = 0xff,
        Spx = 0x4e8,
        SpxII = 0x4e9,
        Tcp = 6,
        Udp = 0x11,
        Unknown = -1,
        Unspecified = 0
    }
}

