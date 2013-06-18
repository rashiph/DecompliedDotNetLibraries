namespace Microsoft.Transactions.Bridge
{
    using System;

    [Flags]
    internal enum ProtocolMarshalCapabilities
    {
        ExplicitMarshalRequest = 1,
        IncludeAsDefault = 2,
        UseStaticProtocolInformation = 4
    }
}

