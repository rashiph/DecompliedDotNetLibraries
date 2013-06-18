namespace System.Transactions
{
    using System;
    using System.Threading;

    internal abstract class TransactionStatePromotedEnded : TransactionStateEnded
    {
        private static WaitCallback signalMethod;

        protected TransactionStatePromotedEnded()
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

        internal override void CompleteAbortingClone(InternalTransaction tx)
        {
        }

        internal override void CompleteBlockingClone(InternalTransaction tx)
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

        internal override void EndCommit(InternalTransaction tx)
        {
            this.PromotedTransactionOutcome(tx);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.EnterState(tx);
            base.CommonEnterState(tx);
            if (!ThreadPool.QueueUserWorkItem(SignalMethod, tx))
            {
                throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("UnexpectedFailureOfThreadPool"), null);
            }
        }

        internal override Guid get_Identifier(InternalTransaction tx)
        {
            return tx.PromotedTransaction.Identifier;
        }

        internal override void Promote(InternalTransaction tx)
        {
        }

        protected abstract void PromotedTransactionOutcome(InternalTransaction tx);
        private static void SignalCallback(object state)
        {
            InternalTransaction tx = (InternalTransaction) state;
            lock (tx)
            {
                tx.SignalAsyncCompletion();
                TransactionManager.TransactionTable.Remove(tx);
            }
        }

        private static WaitCallback SignalMethod
        {
            get
            {
                if (signalMethod == null)
                {
                    lock (TransactionState.ClassSyncObject)
                    {
                        if (signalMethod == null)
                        {
                            signalMethod = new WaitCallback(TransactionStatePromotedEnded.SignalCallback);
                        }
                    }
                }
                return signalMethod;
            }
        }
    }
}

