namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;

    [Flags]
    internal enum CoordinationServiceMode
    {
        Formatter = 1,
        ProtocolService = 2
    }
}

