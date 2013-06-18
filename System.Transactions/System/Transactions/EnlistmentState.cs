namespace System.Transactions
{
    using System;
    using System.Threading;

    internal abstract class EnlistmentState
    {
        internal static EnlistmentStatePromoted _enlistmentStatePromoted;
        private static object classSyncObject;

        protected EnlistmentState()
        {
        }

        internal virtual void Aborted(InternalEnlistment enlistment, Exception e)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void ChangeStateCommitting(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void ChangeStateDelegated(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void ChangeStatePreparing(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void ChangeStatePromoted(InternalEnlistment enlistment, IPromotedEnlistment promotedEnlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void ChangeStateSinglePhaseCommit(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void Committed(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void EnlistmentDone(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal abstract void EnterState(InternalEnlistment enlistment);
        internal virtual void ForceRollback(InternalEnlistment enlistment, Exception e)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void InDoubt(InternalEnlistment enlistment, Exception e)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void InternalAborted(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void InternalCommitted(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void InternalIndoubt(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual void Prepared(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal virtual byte[] RecoveryInformation(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceLtm"), null);
        }

        internal static EnlistmentStatePromoted _EnlistmentStatePromoted
        {
            get
            {
                if (_enlistmentStatePromoted == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_enlistmentStatePromoted == null)
                        {
                            EnlistmentStatePromoted promoted = new EnlistmentStatePromoted();
                            Thread.MemoryBarrier();
                            _enlistmentStatePromoted = promoted;
                        }
                    }
                }
                return _enlistmentStatePromoted;
            }
        }

        private static object ClassSyncObject
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
    }
}

