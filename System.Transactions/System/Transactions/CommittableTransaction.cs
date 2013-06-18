namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    [Serializable]
    public sealed class CommittableTransaction : Transaction, IAsyncResult
    {
        internal bool completedSynchronously;

        public CommittableTransaction() : this(TransactionManager.DefaultIsolationLevel, TransactionManager.DefaultTimeout)
        {
        }

        public CommittableTransaction(TimeSpan timeout) : this(TransactionManager.DefaultIsolationLevel, timeout)
        {
        }

        public CommittableTransaction(TransactionOptions options) : this(options.IsolationLevel, options.Timeout)
        {
        }

        internal CommittableTransaction(IsolationLevel isoLevel, TimeSpan timeout) : base(isoLevel, (InternalTransaction) null)
        {
            base.internalTransaction = new InternalTransaction(timeout, this);
            base.internalTransaction.cloneCount = 1;
            base.cloneId = 1;
            if (DiagnosticTrace.Information)
            {
                TransactionCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.TransactionTraceId);
            }
        }

        public IAsyncResult BeginCommit(AsyncCallback asyncCallback, object asyncState)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "CommittableTransaction.BeginCommit");
                TransactionCommitCalledTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.TransactionTraceId);
            }
            if (base.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            lock (base.internalTransaction)
            {
                if (base.complete)
                {
                    throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
                }
                base.internalTransaction.State.BeginCommit(base.internalTransaction, true, asyncCallback, asyncState);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "CommittableTransaction.BeginCommit");
            }
            return this;
        }

        public void Commit()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "CommittableTransaction.Commit");
                TransactionCommitCalledTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.TransactionTraceId);
            }
            if (base.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            lock (base.internalTransaction)
            {
                if (base.complete)
                {
                    throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
                }
                base.internalTransaction.State.BeginCommit(base.internalTransaction, false, null, null);
                while (!base.internalTransaction.State.IsCompleted(base.internalTransaction) && Monitor.Wait(base.internalTransaction))
                {
                }
                base.internalTransaction.State.EndCommit(base.internalTransaction);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "CommittableTransaction.Commit");
            }
        }

        public void EndCommit(IAsyncResult asyncResult)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "CommittableTransaction.EndCommit");
            }
            if (asyncResult != this)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("BadAsyncResult"), "asyncResult");
            }
            lock (base.internalTransaction)
            {
                while (!base.internalTransaction.State.IsCompleted(base.internalTransaction) && Monitor.Wait(base.internalTransaction))
                {
                }
                base.internalTransaction.State.EndCommit(base.internalTransaction);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "CommittableTransaction.EndCommit");
            }
        }

        internal override void InternalDispose()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "IDisposable.Dispose");
            }
            if (Interlocked.Exchange(ref this.disposed, 1) != 1)
            {
                if (base.internalTransaction.State.get_Status(base.internalTransaction) == TransactionStatus.Active)
                {
                    lock (base.internalTransaction)
                    {
                        base.internalTransaction.State.DisposeRoot(base.internalTransaction);
                    }
                }
                long num = Interlocked.Decrement(ref base.internalTransaction.cloneCount);
                if (num == 0L)
                {
                    base.internalTransaction.Dispose();
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "IDisposable.Dispose");
                }
            }
        }

        object IAsyncResult.AsyncState
        {
            get
            {
                return base.internalTransaction.asyncState;
            }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get
            {
                if (base.internalTransaction.asyncResultEvent == null)
                {
                    lock (base.internalTransaction)
                    {
                        if (base.internalTransaction.asyncResultEvent == null)
                        {
                            ManualResetEvent event2 = new ManualResetEvent(base.internalTransaction.State.get_Status(base.internalTransaction) != TransactionStatus.Active);
                            Thread.MemoryBarrier();
                            base.internalTransaction.asyncResultEvent = event2;
                        }
                    }
                }
                return base.internalTransaction.asyncResultEvent;
            }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get
            {
                return this.completedSynchronously;
            }
        }

        bool IAsyncResult.IsCompleted
        {
            get
            {
                lock (base.internalTransaction)
                {
                    return (base.internalTransaction.State.get_Status(base.internalTransaction) != TransactionStatus.Active);
                }
            }
        }
    }
}

