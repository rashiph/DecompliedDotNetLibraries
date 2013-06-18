namespace System.ServiceModel.Transactions
{
    using System;

    [Flags]
    internal enum IsolationFlags
    {
        Optimistic = 0x10,
        ReadOnly = 0x20,
        RetainAbort = 8,
        RetainAbortDC = 4,
        RetainAbortNo = 12,
        RetainBoth = 10,
        RetainCommit = 2,
        RetainCommitDC = 1,
        RetainCommitNo = 3,
        RetainDoNotCare = 5,
        RetainNone = 15
    }
}

