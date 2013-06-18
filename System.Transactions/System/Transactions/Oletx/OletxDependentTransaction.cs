namespace System.Transactions.Oletx
{
    using System;
    using System.Threading;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    [Serializable]
    internal class OletxDependentTransaction : OletxTransaction
    {
        private int completed;
        private OletxVolatileEnlistmentContainer volatileEnlistmentContainer;

        internal OletxDependentTransaction(RealOletxTransaction realTransaction, bool delayCommit) : base(realTransaction)
        {
            if (realTransaction == null)
            {
                throw new ArgumentNullException("realTransaction");
            }
            this.volatileEnlistmentContainer = base.realOletxTransaction.AddDependentClone(delayCommit);
            if (DiagnosticTrace.Information)
            {
                DependentCloneCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.TransactionTraceId, delayCommit ? DependentCloneOption.BlockCommitUntilComplete : DependentCloneOption.RollbackIfNotComplete);
            }
        }

        public void Complete()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "DependentTransaction.Complete");
            }
            int num = Interlocked.CompareExchange(ref this.completed, 1, 0);
            if (1 == num)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceOletx"));
            }
            if (DiagnosticTrace.Information)
            {
                DependentCloneCompleteTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.TransactionTraceId);
            }
            this.volatileEnlistmentContainer.DependentCloneCompleted();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "DependentTransaction.Complete");
            }
        }
    }
}

