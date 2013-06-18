namespace System.Transactions
{
    using System;

    internal abstract class TransactionStatePromotedAborting : TransactionStatePromotedBase
    {
        protected TransactionStatePromotedAborting()
        {
        }

        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void ChangeStatePromotedAborted(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedAborted.EnterState(tx);
        }

        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
        }

        internal override void CreateAbortingClone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void CreateBlockingClone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.Aborted;
        }

        internal override void RestartCommitIfNeeded(InternalTransaction tx)
        {
        }
    }
}

