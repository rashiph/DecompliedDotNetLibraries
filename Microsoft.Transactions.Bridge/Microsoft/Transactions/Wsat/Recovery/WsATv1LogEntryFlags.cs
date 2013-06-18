namespace Microsoft.Transactions.Wsat.Recovery
{
    using System;

    [Flags]
    internal enum WsATv1LogEntryFlags : byte
    {
        OptimizedEndpointRepresentation = 1,
        UsesDefaultPort = 2,
        UsesStandardCoordinatorAddressPath = 4,
        UsesStandardParticipantAddressPath = 8
    }
}

