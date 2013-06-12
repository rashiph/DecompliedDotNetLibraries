namespace System.Data.SqlClient
{
    using System;

    internal enum TransactionState
    {
        Pending,
        Active,
        Aborted,
        Committed,
        Unknown
    }
}

