namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    [Serializable]
    public sealed class DependentTransaction : Transaction
    {
        private bool blocking;

        internal DependentTransaction(IsolationLevel isoLevel, InternalTransaction internalTransaction, bool blocking) : base(isoLevel, internalTransaction)
        {
            this.blocking = blocking;
            lock (base.internalTransaction)
            {
                if (blocking)
                {
                    base.internalTransaction.State.CreateBlockingClone(base.internalTransaction);
                }
                else
                {
                    base.internalTransaction.State.CreateAbortingClone(base.internalTransaction);
                }
            }
        }

        public void Complete()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "DependentTransaction.Complete");
            }
            lock (base.internalTransaction)
            {
                if (base.Disposed)
                {
                    throw new ObjectDisposedException("Transaction");
                }
                if (base.complete)
                {
                    throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
                }
                base.complete = true;
                if (this.blocking)
                {
                    base.internalTransaction.State.CompleteBlockingClone(base.internalTransaction);
                }
                else
                {
                    base.internalTransaction.State.CompleteAbortingClone(base.internalTransaction);
                }
            }
            if (DiagnosticTrace.Information)
            {
                DependentCloneCompleteTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), base.TransactionTraceId);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "DependentTransaction.Complete");
            }
        }
    }
}

