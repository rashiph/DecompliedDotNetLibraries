namespace System.Net.NetworkInformation
{
    using System;

    internal enum IcmpV6StatType
    {
        DestinationUnreachable = 1,
        EchoReply = 0x81,
        EchoRequest = 0x80,
        MembershipQuery = 130,
        MembershipReduction = 0x84,
        MembershipReport = 0x83,
        NeighborAdvertisement = 0x88,
        NeighborSolict = 0x87,
        PacketTooBig = 2,
        ParameterProblem = 4,
        Redirect = 0x89,
        RouterAdvertisement = 0x86,
        RouterSolicit = 0x85,
        TimeExceeded = 3
    }
}

