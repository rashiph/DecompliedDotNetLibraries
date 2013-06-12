namespace System.Net.Sockets
{
    using System;

    public enum SocketOptionName
    {
        AcceptConnection = 2,
        AddMembership = 12,
        AddSourceMembership = 15,
        BlockSource = 0x11,
        Broadcast = 0x20,
        BsdUrgent = 2,
        ChecksumCoverage = 20,
        Debug = 1,
        DontFragment = 14,
        DontLinger = -129,
        DontRoute = 0x10,
        DropMembership = 13,
        DropSourceMembership = 0x10,
        Error = 0x1007,
        ExclusiveAddressUse = -5,
        Expedited = 2,
        HeaderIncluded = 2,
        HopLimit = 0x15,
        IPOptions = 1,
        IPProtectionLevel = 0x17,
        IpTimeToLive = 4,
        IPv6Only = 0x1b,
        KeepAlive = 8,
        Linger = 0x80,
        MaxConnections = 0x7fffffff,
        MulticastInterface = 9,
        MulticastLoopback = 11,
        MulticastTimeToLive = 10,
        NoChecksum = 1,
        NoDelay = 1,
        OutOfBandInline = 0x100,
        PacketInformation = 0x13,
        ReceiveBuffer = 0x1002,
        ReceiveLowWater = 0x1004,
        ReceiveTimeout = 0x1006,
        ReuseAddress = 4,
        SendBuffer = 0x1001,
        SendLowWater = 0x1003,
        SendTimeout = 0x1005,
        Type = 0x1008,
        TypeOfService = 3,
        UnblockSource = 0x12,
        UpdateAcceptContext = 0x700b,
        UpdateConnectContext = 0x7010,
        UseLoopback = 0x40
    }
}

