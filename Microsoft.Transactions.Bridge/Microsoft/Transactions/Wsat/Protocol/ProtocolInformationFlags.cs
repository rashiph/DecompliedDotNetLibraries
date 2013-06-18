namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal enum ProtocolInformationFlags : byte
    {
        IsClustered = 0x10,
        IssuedTokensEnabled = 1,
        NetworkClientAccess = 2,
        NetworkInboundAccess = 4,
        NetworkOutboundAccess = 8
    }
}

