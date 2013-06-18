namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Data.Common;
    using System.Runtime;
    using System.Threading;
    using System.Transactions;

    internal sealed class SharedConnectionInfo : IDisposable
    {
        private readonly DbConnection connection;
        private bool disposed;
        private ManualResetEvent handle;
        private readonly DbTransaction localTransaction;

        internal SharedConnectionInfo(DbResourceAllocator dbResourceAllocator, Transaction transaction, bool wantPromotable, ManualResetEvent handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException("handle");
            }
            this.handle = handle;
            if (wantPromotable)
            {
                this.connection = dbResourceAllocator.OpenNewConnection();
                this.connection.EnlistTransaction(transaction);
            }
            else
            {
                LocalTransaction promotableSinglePhaseNotification = new LocalTransaction(dbResourceAllocator, handle);
                transaction.EnlistPromotableSinglePhase(promotableSinglePhaseNotification);
                this.connection = promotableSinglePhaseNotification.Connection;
                this.localTransaction = promotableSinglePhaseNotification.Transaction;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if ((!this.disposed && (this.localTransaction == null)) && (this.connection != null))
            {
                this.connection.Dispose();
            }
            this.disposed = true;
        }

        internal DbConnection DBConnection
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.connection;
            }
        }

        internal DbTransaction DBTransaction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localTransaction;
            }
        }
    }
}

