namespace System.Transactions
{
    using System;
    using System.Threading;

    internal class TransactionStatePromotedPhase0 : TransactionStatePromotedCommitting
    {
        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStatePromotedP0Aborting.EnterState(tx);
        }

        internal override bool ContinuePhase0Prepares()
        {
            return true;
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            int volatileEnlistmentCount = tx.phase0Volatiles.volatileEnlistmentCount;
            int dependentClones = tx.phase0Volatiles.dependentClones;
            tx.phase0VolatileWaveCount = volatileEnlistmentCount;
            if (tx.phase0Volatiles.preparedVolatileEnlistments < (volatileEnlistmentCount + dependentClones))
            {
                for (int i = 0; i < volatileEnlistmentCount; i++)
                {
                    tx.phase0Volatiles.volatileEnlistments[i].twoPhaseState.ChangeStatePreparing(tx.phase0Volatiles.volatileEnlistments[i]);
                    if (!tx.State.ContinuePhase0Prepares())
                    {
                        return;
                    }
                }
            }
            else
            {
                this.Phase0VolatilePrepareDone(tx);
            }
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
            Monitor.Exit(tx);
            try
            {
                tx.phase0Volatiles.VolatileDemux.oletxEnlistment.Prepared();
            }
            finally
            {
                Monitor.Enter(tx);
            }
        }
    }
}

