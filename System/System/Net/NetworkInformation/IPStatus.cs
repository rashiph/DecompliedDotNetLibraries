namespace System.Net.NetworkInformation
{
    using System;

    public enum IPStatus
    {
        BadDestination = 0x2b0a,
        BadHeader = 0x2b22,
        BadOption = 0x2aff,
        BadRoute = 0x2b04,
        DestinationHostUnreachable = 0x2afb,
        DestinationNetworkUnreachable = 0x2afa,
        DestinationPortUnreachable = 0x2afd,
        DestinationProhibited = 0x2afc,
        DestinationProtocolUnreachable = 0x2afc,
        DestinationScopeMismatch = 0x2b25,
        DestinationUnreachable = 0x2b20,
        HardwareError = 0x2b00,
        IcmpError = 0x2b24,
        NoResources = 0x2afe,
        PacketTooBig = 0x2b01,
        ParameterProblem = 0x2b07,
        SourceQuench = 0x2b08,
        Success = 0,
        TimedOut = 0x2b02,
        TimeExceeded = 0x2b21,
        TtlExpired = 0x2b05,
        TtlReassemblyTimeExceeded = 0x2b06,
        Unknown = -1,
        UnrecognizedNextHeader = 0x2b23
    }
}

