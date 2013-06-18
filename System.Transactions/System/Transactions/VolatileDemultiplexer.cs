namespace System.Transactions
{
    using System;
    using System.Threading;

    internal abstract class VolatileDemultiplexer : IEnlistmentNotificationInternal
    {
        private static object classSyncObject;
        private static WaitCallback commitCallback;
        private static WaitCallback inDoubtCallback;
        internal IPromotedEnlistment oletxEnlistment;
        private static WaitCallback prepareCallback;
        internal IPromotedEnlistment preparingEnlistment;
        private static WaitCallback rollbackCallback;
        protected InternalTransaction transaction;

        public VolatileDemultiplexer(InternalTransaction transaction)
        {
            this.transaction = transaction;
        }

        internal void BroadcastCommitted(ref VolatileEnlistmentSet volatiles)
        {
            for (int i = 0; i < volatiles.volatileEnlistmentCount; i++)
            {
                volatiles.volatileEnlistments[i].twoPhaseState.InternalCommitted(volatiles.volatileEnlistments[i]);
            }
        }

        internal void BroadcastInDoubt(ref VolatileEnlistmentSet volatiles)
        {
            for (int i = 0; i < volatiles.volatileEnlistmentCount; i++)
            {
                volatiles.volatileEnlistments[i].twoPhaseState.InternalIndoubt(volatiles.volatileEnlistments[i]);
            }
        }

        internal void BroadcastRollback(ref VolatileEnlistmentSet volatiles)
        {
            for (int i = 0; i < volatiles.volatileEnlistmentCount; i++)
            {
                volatiles.volatileEnlistments[i].twoPhaseState.InternalAborted(volatiles.volatileEnlistments[i]);
            }
        }

        public abstract void Commit(IPromotedEnlistment en);
        public abstract void InDoubt(IPromotedEnlistment en);
        protected abstract void InternalCommit();
        protected abstract void InternalInDoubt();
        protected abstract void InternalPrepare();
        protected abstract void InternalRollback();
        protected static void PoolableCommit(object state)
        {
            VolatileDemultiplexer demultiplexer = (VolatileDemultiplexer) state;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(demultiplexer.transaction, 250, ref lockTaken);
                if (lockTaken)
                {
                    demultiplexer.InternalCommit();
                }
                else if (!ThreadPool.QueueUserWorkItem(CommitCallback, demultiplexer))
                {
                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedFailureOfThreadPool"), null);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(demultiplexer.transaction);
                }
            }
        }

        protected static void PoolableInDoubt(object state)
        {
            VolatileDemultiplexer demultiplexer = (VolatileDemultiplexer) state;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(demultiplexer.transaction, 250, ref lockTaken);
                if (lockTaken)
                {
                    demultiplexer.InternalInDoubt();
                }
                else if (!ThreadPool.QueueUserWorkItem(InDoubtCallback, demultiplexer))
                {
                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedFailureOfThreadPool"), null);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(demultiplexer.transaction);
                }
            }
        }

        protected static void PoolablePrepare(object state)
        {
            VolatileDemultiplexer demultiplexer = (VolatileDemultiplexer) state;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(demultiplexer.transaction, 250, ref lockTaken);
                if (lockTaken)
                {
                    demultiplexer.InternalPrepare();
                }
                else if (!ThreadPool.QueueUserWorkItem(PrepareCallback, demultiplexer))
                {
                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedFailureOfThreadPool"), null);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(demultiplexer.transaction);
                }
            }
        }

        protected static void PoolableRollback(object state)
        {
            VolatileDemultiplexer demultiplexer = (VolatileDemultiplexer) state;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(demultiplexer.transaction, 250, ref lockTaken);
                if (lockTaken)
                {
                    demultiplexer.InternalRollback();
                }
                else if (!ThreadPool.QueueUserWorkItem(RollbackCallback, demultiplexer))
                {
                    throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedFailureOfThreadPool"), null);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(demultiplexer.transaction);
                }
            }
        }

        public abstract void Prepare(IPromotedEnlistment en);
        public abstract void Rollback(IPromotedEnlistment en);

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

        private static WaitCallback CommitCallback
        {
            get
            {
                if (commitCallback == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (commitCallback == null)
                        {
                            WaitCallback callback = new WaitCallback(VolatileDemultiplexer.PoolableCommit);
                            Thread.MemoryBarrier();
                            commitCallback = callback;
                        }
                    }
                }
                return commitCallback;
            }
        }

        private static WaitCallback InDoubtCallback
        {
            get
            {
                if (inDoubtCallback == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (inDoubtCallback == null)
                        {
                            WaitCallback callback = new WaitCallback(VolatileDemultiplexer.PoolableInDoubt);
                            Thread.MemoryBarrier();
                            inDoubtCallback = callback;
                        }
                    }
                }
                return inDoubtCallback;
            }
        }

        private static WaitCallback PrepareCallback
        {
            get
            {
                if (prepareCallback == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (prepareCallback == null)
                        {
                            WaitCallback callback = new WaitCallback(VolatileDemultiplexer.PoolablePrepare);
                            Thread.MemoryBarrier();
                            prepareCallback = callback;
                        }
                    }
                }
                return prepareCallback;
            }
        }

        private static WaitCallback RollbackCallback
        {
            get
            {
                if (rollbackCallback == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (rollbackCallback == null)
                        {
                            WaitCallback callback = new WaitCallback(VolatileDemultiplexer.PoolableRollback);
                            Thread.MemoryBarrier();
                            rollbackCallback = callback;
                        }
                    }
                }
                return rollbackCallback;
            }
        }
    }
}

