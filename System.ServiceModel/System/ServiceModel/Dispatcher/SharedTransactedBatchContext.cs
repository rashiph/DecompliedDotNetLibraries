namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;

    internal sealed class SharedTransactedBatchContext
    {
        private int currentBatchSize;
        private int currentConcurrentBatches;
        private int currentConcurrentDispatches;
        private ChannelHandler handler;
        private bool isBatching;
        private readonly System.Transactions.IsolationLevel isolationLevel;
        private readonly int maxBatchSize;
        private readonly int maxConcurrentBatches;
        private object receiveLock = new object();
        private int successfullCommits;
        private object thisLock = new object();
        private readonly TimeSpan txTimeout;

        internal SharedTransactedBatchContext(ChannelHandler handler, ChannelDispatcher dispatcher, int maxConcurrentBatches)
        {
            this.handler = handler;
            this.maxBatchSize = dispatcher.MaxTransactedBatchSize;
            this.maxConcurrentBatches = maxConcurrentBatches;
            this.currentBatchSize = dispatcher.MaxTransactedBatchSize;
            this.currentConcurrentBatches = 0;
            this.currentConcurrentDispatches = 0;
            this.successfullCommits = 0;
            this.isBatching = true;
            this.isolationLevel = dispatcher.TransactionIsolationLevel;
            this.txTimeout = TransactionBehavior.NormalizeTimeout(dispatcher.TransactionTimeout);
            this.BatchingStateChanged(this.isBatching);
        }

        internal void BatchDone()
        {
            lock (this.thisLock)
            {
                this.currentConcurrentBatches--;
                int currentConcurrentBatches = this.currentConcurrentBatches;
            }
        }

        private void BatchingStateChanged(bool batchingNow)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, batchingNow ? 0x40057 : 0x4005a, batchingNow ? System.ServiceModel.SR.GetString("TraceCodeMsmqEnteredBatch") : System.ServiceModel.SR.GetString("TraceCodeMsmqLeftBatch"), null, null, null);
            }
        }

        internal TransactedBatchContext CreateTransactedBatchContext()
        {
            lock (this.thisLock)
            {
                TransactedBatchContext context = new TransactedBatchContext(this);
                this.currentConcurrentBatches++;
                return context;
            }
        }

        internal void DispatchEnded()
        {
            lock (this.thisLock)
            {
                this.currentConcurrentDispatches--;
                int currentConcurrentDispatches = this.currentConcurrentDispatches;
            }
        }

        internal void DispatchStarted()
        {
            lock (this.thisLock)
            {
                this.currentConcurrentDispatches++;
                if ((this.currentConcurrentDispatches == this.currentConcurrentBatches) && (this.currentConcurrentBatches < this.maxConcurrentBatches))
                {
                    TransactedBatchContext context = new TransactedBatchContext(this);
                    this.currentConcurrentBatches++;
                    ChannelHandler handler = new ChannelHandler(this.handler, context);
                    ChannelHandler.Register(handler);
                }
            }
        }

        internal void ReportAbort()
        {
            lock (this.thisLock)
            {
                if (this.isBatching)
                {
                    this.successfullCommits = 0;
                    this.currentBatchSize = 1;
                    this.isBatching = false;
                    this.BatchingStateChanged(this.isBatching);
                }
            }
        }

        internal void ReportCommit()
        {
            lock (this.thisLock)
            {
                if (++this.successfullCommits >= (this.maxBatchSize * 2))
                {
                    this.successfullCommits = 0;
                    if (!this.isBatching)
                    {
                        this.currentBatchSize = this.maxBatchSize;
                        this.isBatching = true;
                        this.BatchingStateChanged(this.isBatching);
                    }
                }
            }
        }

        internal int CurrentBatchSize
        {
            get
            {
                lock (this.thisLock)
                {
                    return this.currentBatchSize;
                }
            }
        }

        internal System.Transactions.IsolationLevel IsolationLevel
        {
            get
            {
                return this.isolationLevel;
            }
        }

        internal object ReceiveLock
        {
            get
            {
                return this.receiveLock;
            }
        }

        internal TimeSpan TransactionTimeout
        {
            get
            {
                return this.txTimeout;
            }
        }
    }
}

