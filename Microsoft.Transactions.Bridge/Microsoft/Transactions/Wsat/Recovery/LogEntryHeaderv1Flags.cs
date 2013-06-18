namespace Microsoft.Transactions.Wsat.Recovery
{
    using System;

    [Flags]
    internal enum LogEntryHeaderv1Flags : byte
    {
        StandardRemoteTransactionId = 1
    }
}

