namespace System.Web.Util
{
    using System;

    internal enum TransactedExecState
    {
        CommitPending,
        AbortPending,
        Error
    }
}

