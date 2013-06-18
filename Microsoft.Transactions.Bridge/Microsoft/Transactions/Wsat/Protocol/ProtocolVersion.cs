namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    [Flags]
    internal enum ProtocolVersion : ushort
    {
        Version10 = 1,
        Version11 = 2
    }
}

