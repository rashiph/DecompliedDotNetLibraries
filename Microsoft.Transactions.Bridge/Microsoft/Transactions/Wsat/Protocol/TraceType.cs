namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal enum TraceType : byte
    {
        Resume = 11,
        Start = 1,
        Stop = 2,
        Suspend = 10,
        Trace = 0,
        Transfer = 5
    }
}

