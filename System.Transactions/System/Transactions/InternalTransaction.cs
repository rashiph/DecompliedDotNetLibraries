namespace System.Transactions
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Transactions.Oletx;

    internal class InternalTransaction : IDisposable
    {
        internal OletxDependentTransaction abortingDependentClone;
        internal int abortingDependentCloneCount;
        private long absoluteTimeout;
        internal AsyncCallback asyncCallback;
        internal bool asyncCommit;
        internal ManualResetEvent asyncResultEvent;
        internal object asyncState;
        internal int bucketIndex;
        private static object classSyncObject;
        internal int cloneCount;
        internal CommittableTransaction committableTransaction;
        private long creationTime;
        internal InternalEnlistment durableEnlistment;
        internal int enlistmentCount;
        internal FinalizedObject finalizedObject;
        internal Exception innerException;
        private static string instanceIdentifier;
        internal bool needPulse;
        internal static int nextHash;
        internal Transaction outcomeSource;
        internal VolatileEnlistmentSet phase0Volatiles;
        internal int phase0VolatileWaveCount;
        internal OletxDependentTransaction phase0WaveDependentClone;
        internal int phase0WaveDependentCloneCount;
        internal VolatileEnlistmentSet phase1Volatiles;
        private OletxTransaction promotedTransaction;
        internal ITransactionPromoter promoter;
        internal TransactionState promoteState;
        internal Bucket tableBucket;
        private TransactionTraceIdentifier traceIdentifier;
        internal TransactionCompletedEventHandler transactionCompletedDelegate;
        internal int transactionHash;
        internal TransactionInformation transactionInformation;
        protected TransactionState transactionState;
        internal const int volatileArrayIncrement = 8;

        internal InternalTransaction(TimeSpan timeout, CommittableTransaction committableTransaction)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            this.absoluteTimeout = TransactionManager.TransactionTable.TimeoutTicks(timeout);
            TransactionState._TransactionStateActive.EnterState(this);
            this.promoteState = TransactionState._TransactionStatePromoted;
            this.committableTransaction = committableTransaction;
            this.outcomeSource = committableTransaction;
            this.transactionHash = TransactionManager.TransactionTable.Add(this);
        }

        internal InternalTransaction(Transaction outcomeSource, ITransactionPromoter promoter)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            this.absoluteTimeout = 0x7fffffffffffffffL;
            this.outcomeSource = outcomeSource;
            this.transactionHash = TransactionManager.TransactionTable.Add(this);
            this.promoter = promoter;
            TransactionState._TransactionStateSubordinateActive.EnterState(this);
            this.promoteState = TransactionState._TransactionStateDelegatedSubordinate;
        }

        internal InternalTransaction(Transaction outcomeSource, OletxTransaction distributedTx)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            this.promotedTransaction = distributedTx;
            this.absoluteTimeout = 0x7fffffffffffffffL;
            this.outcomeSource = outcomeSource;
            this.transactionHash = TransactionManager.TransactionTable.Add(this);
            TransactionState._TransactionStateNonCommittablePromoted.EnterState(this);
            this.promoteState = TransactionState._TransactionStateNonCommittablePromoted;
        }

        public void Dispose()
        {
            if (this.promotedTransaction != null)
            {
                this.promotedTransaction.Dispose();
            }
        }

        internal static void DistributedTransactionOutcome(InternalTransaction tx, TransactionStatus status)
        {
            FinalizedObject finalizedObject = null;
            lock (tx)
            {
                if (tx.innerException == null)
                {
                    tx.innerException = tx.PromotedTransaction.InnerException;
                }
                switch (status)
                {
                    case TransactionStatus.Committed:
                        tx.State.ChangeStatePromotedCommitted(tx);
                        break;

                    case TransactionStatus.Aborted:
                        tx.State.ChangeStatePromotedAborted(tx);
                        break;

                    case TransactionStatus.InDoubt:
                        tx.State.InDoubtFromDtc(tx);
                        break;

                    default:
                        TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), "", null);
                        break;
                }
                finalizedObject = tx.finalizedObject;
            }
            if (finalizedObject != null)
            {
                finalizedObject.Dispose();
            }
        }

        internal void FireCompletion()
        {
            TransactionCompletedEventHandler transactionCompletedDelegate = this.transactionCompletedDelegate;
            if (transactionCompletedDelegate != null)
            {
                TransactionEventArgs e = new TransactionEventArgs {
                    transaction = this.outcomeSource.InternalClone()
                };
                transactionCompletedDelegate(e.transaction, e);
            }
        }

        internal void SignalAsyncCompletion()
        {
            if (this.asyncResultEvent != null)
            {
                this.asyncResultEvent.Set();
            }
            if (this.asyncCallback != null)
            {
                Monitor.Exit(this);
                try
                {
                    this.asyncCallback(this.committableTransaction);
                }
                finally
                {
                    Monitor.Enter(this);
                }
            }
        }

        internal long AbsoluteTimeout
        {
            get
            {
                return this.absoluteTimeout;
            }
        }

        internal static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }

        internal long CreationTime
        {
            get
            {
                return this.creationTime;
            }
            set
            {
                this.creationTime = value;
            }
        }

        internal static string InstanceIdentifier
        {
            get
            {
                if (instanceIdentifier == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (instanceIdentifier == null)
                        {
                            string str = Guid.NewGuid().ToString() + ":";
                            Thread.MemoryBarrier();
                            instanceIdentifier = str;
                        }
                    }
                }
                return instanceIdentifier;
            }
        }

        internal OletxTransaction PromotedTransaction
        {
            get
            {
                return this.promotedTransaction;
            }
            set
            {
                this.promotedTransaction = value;
            }
        }

        internal TransactionState State
        {
            get
            {
                return this.transactionState;
            }
            set
            {
                this.transactionState = value;
            }
        }

        internal int TransactionHash
        {
            get
            {
                return this.transactionHash;
            }
        }

        internal TransactionTraceIdentifier TransactionTraceId
        {
            get
            {
                if (this.traceIdentifier == TransactionTraceIdentifier.Empty)
                {
                    lock (this)
                    {
                        if (this.traceIdentifier == TransactionTraceIdentifier.Empty)
                        {
                            TransactionTraceIdentifier identifier = new TransactionTraceIdentifier(InstanceIdentifier + Convert.ToString(this.transactionHash, CultureInfo.InvariantCulture), 0);
                            Thread.MemoryBarrier();
                            this.traceIdentifier = identifier;
                        }
                    }
                }
                return this.traceIdentifier;
            }
        }
    }
}

