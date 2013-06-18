namespace System.Transactions
{
    using System;

    internal class TransactionStateVolatileSPC : ActiveStates
    {
        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStateAborted.EnterState(tx);
        }

        internal override void ChangeStateTransactionCommitted(InternalTransaction tx)
        {
            TransactionState._TransactionStateCommitted.EnterState(tx);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            tx.phase1Volatiles.volatileEnlistments[0].twoPhaseState.ChangeStateSinglePhaseCommit(tx.phase1Volatiles.volatileEnlistments[0]);
        }

        internal override void InDoubtFromEnlistment(InternalTransaction tx)
        {
            TransactionState._TransactionStateInDoubt.EnterState(tx);
        }
    }
}

