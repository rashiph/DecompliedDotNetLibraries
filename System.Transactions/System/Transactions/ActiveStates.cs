namespace System.Transactions
{
    using System;

    internal abstract class ActiveStates : TransactionState
    {
        protected ActiveStates()
        {
        }

        internal override void AddOutcomeRegistrant(InternalTransaction tx, TransactionCompletedEventHandler transactionCompletedDelegate)
        {
            tx.transactionCompletedDelegate = (TransactionCompletedEventHandler) Delegate.Combine(tx.transactionCompletedDelegate, transactionCompletedDelegate);
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.Active;
        }
    }
}

