namespace System.Transactions
{
    using System;
    using System.Threading;

    internal class TransactionStatePromotedPhase1 : TransactionStatePromotedCommitting
    {
        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStatePromotedP1Aborting.EnterState(tx);
        }

        internal override bool ContinuePhase1Prepares()
        {
            return true;
        }

        internal override void CreateAbortingClone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void CreateBlockingClone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw new TransactionException(System.Transactions.SR.GetString("TooLate"));
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw new TransactionException(System.Transactions.SR.GetString("TooLate"));
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw new TransactionException(System.Transactions.SR.GetString("TooLate"));
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw new TransactionException(System.Transactions.SR.GetString("TooLate"));
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            if (tx.committableTransaction != null)
            {
                tx.committableTransaction.complete = true;
            }
            if (tx.phase1Volatiles.dependentClones != 0)
            {
                tx.State.ChangeStateTransactionAborted(tx, null);
            }
            else
            {
                int volatileEnlistmentCount = tx.phase1Volatiles.volatileEnlistmentCount;
                if (tx.phase1Volatiles.preparedVolatileEnlistments < volatileEnlistmentCount)
                {
                    for (int i = 0; i < volatileEnlistmentCount; i++)
                    {
                        tx.phase1Volatiles.volatileEnlistments[i].twoPhaseState.ChangeStatePreparing(tx.phase1Volatiles.volatileEnlistments[i]);
                        if (!tx.State.ContinuePhase1Prepares())
                        {
                            return;
                        }
                    }
                }
                else
                {
                    this.Phase1VolatilePrepareDone(tx);
                }
            }
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
            Monitor.Exit(tx);
            try
            {
                tx.phase1Volatiles.VolatileDemux.oletxEnlistment.Prepared();
            }
            finally
            {
                Monitor.Enter(tx);
            }
        }
    }
}

