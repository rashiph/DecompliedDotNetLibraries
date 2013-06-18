namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal enum TransactionOutcome
    {
        Committed,
        Aborted,
        InDoubt
    }
}

