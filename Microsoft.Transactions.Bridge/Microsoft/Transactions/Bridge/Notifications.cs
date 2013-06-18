namespace Microsoft.Transactions.Bridge
{
    using System;

    [Flags]
    internal enum Notifications
    {
        All = 0x1f,
        AllProtocols = 7,
        InDoubt = 8,
        Outcome = 4,
        Phase0 = 1,
        TwoPhaseCommit = 2,
        Volatile = 0x10
    }
}

