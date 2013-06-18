namespace Microsoft.Transactions.Bridge
{
    using System;

    internal enum TransactionManagerState
    {
        Uninitialized,
        Initialized,
        Starting,
        Started,
        Stopping,
        Stopped
    }
}

