namespace System.Transactions.Diagnostics
{
    using System;

    internal enum NotificationCall
    {
        Prepare,
        Commit,
        Rollback,
        InDoubt,
        SinglePhaseCommit,
        Promote
    }
}

