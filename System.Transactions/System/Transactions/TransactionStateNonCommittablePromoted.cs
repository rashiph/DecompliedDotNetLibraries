namespace System.Transactions
{
    using System;

    internal class TransactionStateNonCommittablePromoted : TransactionStatePromotedBase
    {
        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            tx.PromotedTransaction.realOletxTransaction.InternalTransaction = tx;
        }
    }
}

