namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class DurableEnlistmentAborting : DurableEnlistmentState
    {
        internal override void Aborted(InternalEnlistment enlistment, Exception e)
        {
            if (enlistment.Transaction.innerException == null)
            {
                enlistment.Transaction.innerException = e;
            }
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
        }

        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
            Monitor.Exit(enlistment.Transaction);
            try
            {
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), enlistment.EnlistmentTraceId, NotificationCall.Rollback);
                }
                if (enlistment.SinglePhaseNotification != null)
                {
                    enlistment.SinglePhaseNotification.Rollback(enlistment.SinglePhaseEnlistment);
                }
                else
                {
                    enlistment.PromotableSinglePhaseNotification.Rollback(enlistment.SinglePhaseEnlistment);
                }
            }
            finally
            {
                Monitor.Enter(enlistment.Transaction);
            }
        }
    }
}

