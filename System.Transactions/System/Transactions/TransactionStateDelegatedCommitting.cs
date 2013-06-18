namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class TransactionStateDelegatedCommitting : TransactionStatePromotedCommitting
    {
        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            Monitor.Exit(tx);
            if (DiagnosticTrace.Verbose)
            {
                EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.durableEnlistment.EnlistmentTraceId, NotificationCall.SinglePhaseCommit);
            }
            try
            {
                tx.durableEnlistment.PromotableSinglePhaseNotification.SinglePhaseCommit(tx.durableEnlistment.SinglePhaseEnlistment);
            }
            finally
            {
                Monitor.Enter(tx);
            }
        }
    }
}

