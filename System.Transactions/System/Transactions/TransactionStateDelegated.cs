namespace System.Transactions
{
    using System;

    internal class TransactionStateDelegated : TransactionStateDelegatedBase
    {
        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            tx.asyncCommit = asyncCommit;
            tx.asyncCallback = asyncCallback;
            tx.asyncState = asyncState;
            TransactionState._TransactionStateDelegatedCommitting.EnterState(tx);
        }

        internal override bool PromoteDurable(InternalTransaction tx)
        {
            tx.durableEnlistment.State.ChangeStateDelegated(tx.durableEnlistment);
            return true;
        }

        internal override void RestartCommitIfNeeded(InternalTransaction tx)
        {
            TransactionState._TransactionStateDelegatedP0Wave.EnterState(tx);
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStateDelegatedAborting.EnterState(tx);
        }
    }
}

