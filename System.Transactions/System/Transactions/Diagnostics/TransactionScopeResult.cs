namespace System.Transactions.Diagnostics
{
    using System;

    internal enum TransactionScopeResult
    {
        CreatedTransaction,
        UsingExistingCurrent,
        TransactionPassed,
        DependentTransactionPassed,
        NoTransaction
    }
}

