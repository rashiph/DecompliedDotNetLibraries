namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Transactions;

    internal sealed class TransactedBatchContext : IEnlistmentNotification
    {
        private bool batchFinished;
        private DateTime commitNotLaterThan;
        private int commits;
        private bool inDispatch;
        private SharedTransactedBatchContext shared;
        private CommittableTransaction transaction;

        internal TransactedBatchContext(SharedTransactedBatchContext shared)
        {
            this.shared = shared;
            this.transaction = TransactionBehavior.CreateTransaction(shared.IsolationLevel, shared.TransactionTimeout);
            this.transaction.EnlistVolatile(this, EnlistmentOptions.None);
            if (shared.TransactionTimeout <= TimeSpan.Zero)
            {
                this.commitNotLaterThan = DateTime.MaxValue;
            }
            else
            {
                this.commitNotLaterThan = DateTime.UtcNow + TimeSpan.FromMilliseconds((shared.TransactionTimeout.TotalMilliseconds * 4.0) / 5.0);
            }
            this.commits = 0;
            this.batchFinished = false;
            this.inDispatch = false;
        }

        internal void Complete()
        {
            this.commits++;
            if ((this.commits >= this.shared.CurrentBatchSize) || (DateTime.UtcNow >= this.commitNotLaterThan))
            {
                this.ForceCommit();
            }
        }

        internal void ForceCommit()
        {
            try
            {
                this.transaction.Commit();
            }
            catch (ObjectDisposedException exception)
            {
                MsmqDiagnostics.ExpectedException(exception);
            }
            catch (TransactionException exception2)
            {
                MsmqDiagnostics.ExpectedException(exception2);
            }
            this.batchFinished = true;
        }

        internal void ForceRollback()
        {
            try
            {
                this.transaction.Rollback();
            }
            catch (ObjectDisposedException exception)
            {
                MsmqDiagnostics.ExpectedException(exception);
            }
            catch (TransactionException exception2)
            {
                MsmqDiagnostics.ExpectedException(exception2);
            }
            this.batchFinished = true;
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            this.shared.ReportCommit();
            this.shared.BatchDone();
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            this.shared.ReportAbort();
            this.shared.BatchDone();
            enlistment.Done();
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            this.shared.ReportAbort();
            this.shared.BatchDone();
            enlistment.Done();
        }

        internal bool AboutToExpire
        {
            get
            {
                return (DateTime.UtcNow > this.commitNotLaterThan);
            }
        }

        internal bool InDispatch
        {
            get
            {
                return this.inDispatch;
            }
            set
            {
                bool inDispatch = this.inDispatch;
                this.inDispatch = value;
                if (this.inDispatch)
                {
                    this.shared.DispatchStarted();
                }
                else
                {
                    this.shared.DispatchEnded();
                }
            }
        }

        internal bool IsActive
        {
            get
            {
                if (this.batchFinished)
                {
                    return false;
                }
                try
                {
                    return (TransactionStatus.Active == this.transaction.TransactionInformation.Status);
                }
                catch (ObjectDisposedException exception)
                {
                    MsmqDiagnostics.ExpectedException(exception);
                    return false;
                }
            }
        }

        internal SharedTransactedBatchContext Shared
        {
            get
            {
                return this.shared;
            }
        }

        internal System.Transactions.Transaction Transaction
        {
            get
            {
                return this.transaction;
            }
        }
    }
}

