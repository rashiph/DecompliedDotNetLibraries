namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Transactions;

    public interface IPendingWork
    {
        void Commit(Transaction transaction, ICollection items);
        void Complete(bool succeeded, ICollection items);
        bool MustCommit(ICollection items);
    }
}

