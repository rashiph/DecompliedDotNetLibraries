namespace System.Messaging
{
    using System;
    using System.Messaging.Interop;
    using System.Runtime;
    using System.Threading;

    public class MessageQueueTransaction : IDisposable
    {
        private bool disposed;
        private ITransaction internalTransaction;
        private MessageQueueTransactionStatus transactionStatus = MessageQueueTransactionStatus.Initialized;

        public void Abort()
        {
            lock (this)
            {
                if (this.internalTransaction == null)
                {
                    throw new InvalidOperationException(Res.GetString("TransactionNotStarted"));
                }
                this.AbortInternalTransaction();
            }
        }

        private void AbortInternalTransaction()
        {
            int num = this.internalTransaction.Abort(0, 0, 0);
            if (MessageQueue.IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
            this.internalTransaction = null;
            this.transactionStatus = MessageQueueTransactionStatus.Aborted;
        }

        public void Begin()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            lock (this)
            {
                if (this.internalTransaction != null)
                {
                    throw new InvalidOperationException(Res.GetString("TransactionStarted"));
                }
                int num = SafeNativeMethods.MQBeginTransaction(out this.internalTransaction);
                if (MessageQueue.IsFatalError(num))
                {
                    this.internalTransaction = null;
                    throw new MessageQueueException(num);
                }
                this.transactionStatus = MessageQueueTransactionStatus.Pending;
            }
        }

        internal ITransaction BeginQueueOperation()
        {
            Monitor.Enter(this);
            return this.internalTransaction;
        }

        public void Commit()
        {
            lock (this)
            {
                if (this.internalTransaction == null)
                {
                    throw new InvalidOperationException(Res.GetString("TransactionNotStarted"));
                }
                int num = this.internalTransaction.Commit(0, 0, 0);
                if (MessageQueue.IsFatalError(num))
                {
                    throw new MessageQueueException(num);
                }
                this.internalTransaction = null;
                this.transactionStatus = MessageQueueTransactionStatus.Committed;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (this.internalTransaction != null)
                    {
                        this.AbortInternalTransaction();
                    }
                }
            }
            this.disposed = true;
        }

        internal void EndQueueOperation()
        {
            Monitor.Exit(this);
        }

        ~MessageQueueTransaction()
        {
            this.Dispose(false);
        }

        internal ITransaction InnerTransaction
        {
            get
            {
                return this.internalTransaction;
            }
        }

        public MessageQueueTransactionStatus Status
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionStatus;
            }
        }
    }
}

