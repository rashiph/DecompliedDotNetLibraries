namespace System.Transactions
{
    using System;

    public enum DependentCloneOption
    {
        BlockCommitUntilComplete,
        RollbackIfNotComplete
    }
}

