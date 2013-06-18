namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    internal class TransactionStatePromotedCommitted : TransactionStatePromotedEnded
    {
        internal override void ChangeStatePromotedCommitted(InternalTransaction tx)
        {
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.EnterState(tx);
            if (tx.phase1Volatiles.VolatileDemux != null)
            {
                tx.phase1Volatiles.VolatileDemux.BroadcastCommitted(ref tx.phase1Volatiles);
            }
            if (tx.phase0Volatiles.VolatileDemux != null)
            {
                tx.phase0Volatiles.VolatileDemux.BroadcastCommitted(ref tx.phase0Volatiles);
            }
            tx.FireCompletion();
            if (DiagnosticTrace.Verbose)
            {
                TransactionCommittedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
            }
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.Committed;
        }

        internal override void InDoubtFromDtc(InternalTransaction tx)
        {
        }

        internal override void InDoubtFromEnlistment(InternalTransaction tx)
        {
        }

        protected override void PromotedTransactionOutcome(InternalTransaction tx)
        {
        }
    }
}

