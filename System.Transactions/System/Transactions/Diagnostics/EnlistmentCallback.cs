namespace System.Transactions.Diagnostics
{
    using System;

    internal enum EnlistmentCallback
    {
        Done,
        Prepared,
        ForceRollback,
        Committed,
        Aborted,
        InDoubt
    }
}

