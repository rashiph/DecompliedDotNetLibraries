namespace Microsoft.Transactions.Bridge
{
    using System;

    internal enum ProtocolProviderState
    {
        Uninitialized,
        Initialized,
        Starting,
        Started,
        Stopping,
        Stopped
    }
}

