namespace System.Transactions
{
    using System;

    internal class TransactionStateSPC : ActiveStates
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
            if (tx.durableEnlistment != null)
            {
                tx.durableEnlistment.State.ChangeStateCommitting(tx.durableEnlistment);
            }
            else
            {
                TransactionState._TransactionStateCommitted.EnterState(tx);
            }
        }

        internal override void InDoubtFromEnlistment(InternalTransaction tx)
        {
            TransactionState._TransactionStateInDoubt.EnterState(tx);
        }
    }
}

