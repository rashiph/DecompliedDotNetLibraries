namespace System.Transactions.Oletx
{
    using System;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    [Serializable]
    internal class OletxCommittableTransaction : OletxTransaction
    {
        private bool commitCalled;

        internal OletxCommittableTransaction(RealOletxTransaction realOletxTransaction) : base(realOletxTransaction)
        {
            realOletxTransaction.committableTransaction = this;
        }

        internal void BeginCommit(InternalTransaction internalTransaction)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "CommittableTransaction.BeginCommit");
                TransactionCommitCalledTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.TransactionTraceId);
            }
            base.realOletxTransaction.InternalTransaction = internalTransaction;
            this.commitCalled = true;
            base.realOletxTransaction.Commit();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "CommittableTransaction.BeginCommit");
            }
        }

        internal bool CommitCalled
        {
            get
            {
                return this.commitCalled;
            }
        }
    }
}

