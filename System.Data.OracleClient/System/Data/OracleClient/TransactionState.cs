namespace System.Data.OracleClient
{
    using System;

    internal enum TransactionState
    {
        AutoCommit,
        LocalStarted,
        GlobalStarted
    }
}

