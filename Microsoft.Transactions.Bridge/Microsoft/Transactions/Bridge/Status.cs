namespace Microsoft.Transactions.Bridge
{
    using System;

    internal enum Status
    {
        Success,
        Error,
        Committed,
        Aborted,
        Prepared,
        Readonly,
        PrePrepared,
        InDoubt,
        DuplicateTransaction,
        TooLate,
        TransactionNotFound,
        TooManySubordinateEnlistments,
        InvalidRecoveryData,
        LocalPropagation,
        DuplicatePropagation
    }
}

