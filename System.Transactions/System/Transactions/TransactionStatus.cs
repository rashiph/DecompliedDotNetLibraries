namespace System.Transactions
{
    using System;

    public enum TransactionStatus
    {
        Active,
        Committed,
        Aborted,
        InDoubt
    }
}

