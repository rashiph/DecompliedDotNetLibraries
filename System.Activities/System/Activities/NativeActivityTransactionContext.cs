namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Transactions;

    public sealed class NativeActivityTransactionContext : NativeActivityContext
    {
        private ActivityExecutor executor;
        private RuntimeTransactionHandle transactionHandle;

        internal NativeActivityTransactionContext(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarks, RuntimeTransactionHandle handle) : base(instance, executor, bookmarks)
        {
            this.executor = executor;
            this.transactionHandle = handle;
        }

        public void SetRuntimeTransaction(Transaction transaction)
        {
            base.ThrowIfDisposed();
            if (transaction == null)
            {
                throw FxTrace.Exception.ArgumentNull("transaction");
            }
            this.executor.SetTransaction(this.transactionHandle, transaction, this.transactionHandle.Owner, base.CurrentInstance);
        }
    }
}

