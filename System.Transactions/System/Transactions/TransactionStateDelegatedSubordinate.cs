namespace System.Transactions
{
    using System;

    internal class TransactionStateDelegatedSubordinate : TransactionStateDelegatedBase
    {
        internal override void ChangeStatePromotedPhase0(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedPhase0.EnterState(tx);
        }

        internal override void ChangeStatePromotedPhase1(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedPhase1.EnterState(tx);
        }

        internal override bool PromoteDurable(InternalTransaction tx)
        {
            return true;
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            tx.PromotedTransaction.Rollback();
            TransactionState._TransactionStatePromotedAborted.EnterState(tx);
        }
    }
}

