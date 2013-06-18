namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Transactions;

    internal class WorkflowPersistenceContext
    {
        private Transaction clonedTransaction;
        private CommittableTransaction contextOwnedTransaction;

        public WorkflowPersistenceContext(bool transactionRequired, TimeSpan transactionTimeout) : this(transactionRequired, CloneAmbientTransaction(), transactionTimeout)
        {
        }

        public WorkflowPersistenceContext(bool transactionRequired, Transaction transactionToUse, TimeSpan transactionTimeout)
        {
            if (transactionToUse != null)
            {
                this.clonedTransaction = transactionToUse;
            }
            else if (transactionRequired)
            {
                this.contextOwnedTransaction = new CommittableTransaction(transactionTimeout);
                this.clonedTransaction = this.contextOwnedTransaction.Clone();
            }
        }

        public void Abort()
        {
            if (this.contextOwnedTransaction != null)
            {
                try
                {
                    this.contextOwnedTransaction.Rollback();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
            }
        }

        private static Transaction CloneAmbientTransaction()
        {
            Transaction current = Transaction.Current;
            if (current != null)
            {
                return current.Clone();
            }
            return null;
        }

        public void Complete()
        {
            if (this.contextOwnedTransaction != null)
            {
                this.contextOwnedTransaction.Commit();
            }
        }

        public void EndComplete(IAsyncResult result)
        {
            this.contextOwnedTransaction.EndCommit(result);
        }

        public bool TryBeginComplete(AsyncCallback callback, object state, out IAsyncResult result)
        {
            if (this.contextOwnedTransaction != null)
            {
                result = this.contextOwnedTransaction.BeginCommit(callback, state);
                return true;
            }
            result = null;
            return false;
        }

        public Transaction PublicTransaction
        {
            get
            {
                return this.clonedTransaction;
            }
        }
    }
}

