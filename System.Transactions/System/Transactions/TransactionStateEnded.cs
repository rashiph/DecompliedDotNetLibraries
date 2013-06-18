namespace System.Transactions
{
    using System;
    using System.Threading;

    internal abstract class TransactionStateEnded : TransactionState
    {
        protected TransactionStateEnded()
        {
        }

        internal override void AddOutcomeRegistrant(InternalTransaction tx, TransactionCompletedEventHandler transactionCompletedDelegate)
        {
            if (transactionCompletedDelegate != null)
            {
                TransactionEventArgs e = new TransactionEventArgs {
                    transaction = tx.outcomeSource.InternalClone()
                };
                transactionCompletedDelegate(e.transaction, e);
            }
        }

        internal override void EnterState(InternalTransaction tx)
        {
            if (tx.needPulse)
            {
                Monitor.Pulse(tx);
            }
        }

        internal override bool IsCompleted(InternalTransaction tx)
        {
            return true;
        }
    }
}

