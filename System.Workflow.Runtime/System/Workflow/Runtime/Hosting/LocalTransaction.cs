namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Runtime;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.Runtime;

    internal sealed class LocalTransaction : IPromotableSinglePhaseNotification, ITransactionPromoter
    {
        private DbConnection connection;
        private ManualResetEvent handle;
        private object syncRoot = new object();
        private readonly DbTransaction transaction;

        internal LocalTransaction(DbResourceAllocator dbHelper, ManualResetEvent handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException("handle");
            }
            this.connection = dbHelper.OpenNewConnectionNoEnlist();
            this.transaction = this.connection.BeginTransaction();
            this.handle = handle;
        }

        public void Initialize()
        {
        }

        public byte[] Promote()
        {
            throw new TransactionPromotionException(ExecutionStringManager.PromotionNotSupported);
        }

        public void Rollback(SinglePhaseEnlistment en)
        {
            if (en == null)
            {
                throw new ArgumentNullException("en");
            }
            try
            {
                this.handle.WaitOne();
            }
            catch (ObjectDisposedException)
            {
            }
            lock (this.syncRoot)
            {
                if (this.transaction != null)
                {
                    this.transaction.Dispose();
                }
                if ((this.connection != null) && (this.connection.State != ConnectionState.Closed))
                {
                    this.connection.Close();
                    this.connection = null;
                }
                en.Aborted();
            }
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment en)
        {
            if (en == null)
            {
                throw new ArgumentNullException("en");
            }
            try
            {
                this.handle.WaitOne();
            }
            catch (ObjectDisposedException)
            {
            }
            lock (this.syncRoot)
            {
                try
                {
                    this.transaction.Commit();
                    en.Committed();
                }
                catch (Exception exception)
                {
                    en.Aborted(exception);
                }
                finally
                {
                    if ((this.connection != null) && (this.connection.State != ConnectionState.Closed))
                    {
                        this.connection.Close();
                        this.connection = null;
                    }
                }
            }
        }

        public DbConnection Connection
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.connection;
            }
        }

        public DbTransaction Transaction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transaction;
            }
        }
    }
}

