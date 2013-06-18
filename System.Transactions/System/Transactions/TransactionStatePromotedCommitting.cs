namespace System.Transactions
{
    using System;
    using System.Transactions.Oletx;

    internal class TransactionStatePromotedCommitting : TransactionStatePromotedBase
    {
        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void ChangeStatePromotedPhase0(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedPhase0.EnterState(tx);
        }

        internal override void ChangeStatePromotedPhase1(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedPhase1.EnterState(tx);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            ((OletxCommittableTransaction) tx.PromotedTransaction).BeginCommit(tx);
        }
    }
}

