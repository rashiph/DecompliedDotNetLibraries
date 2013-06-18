namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class DurableEnlistmentCommitting : DurableEnlistmentState
    {
        internal override void Aborted(InternalEnlistment enlistment, Exception e)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStateTransactionAborted(enlistment.Transaction, e);
        }

        internal override void Committed(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStateTransactionCommitted(enlistment.Transaction);
        }

        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStateTransactionCommitted(enlistment.Transaction);
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            bool flag = false;
            enlistment.State = this;
            Monitor.Exit(enlistment.Transaction);
            try
            {
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), enlistment.EnlistmentTraceId, NotificationCall.SinglePhaseCommit);
                }
                if (enlistment.SinglePhaseNotification != null)
                {
                    enlistment.SinglePhaseNotification.SinglePhaseCommit(enlistment.SinglePhaseEnlistment);
                }
                else
                {
                    enlistment.PromotableSinglePhaseNotification.SinglePhaseCommit(enlistment.SinglePhaseEnlistment);
                }
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    enlistment.SinglePhaseEnlistment.InDoubt();
                }
                Monitor.Enter(enlistment.Transaction);
            }
        }

        internal override void InDoubt(InternalEnlistment enlistment, Exception e)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
            if (enlistment.Transaction.innerException == null)
            {
                enlistment.Transaction.innerException = e;
            }
            enlistment.Transaction.State.InDoubtFromEnlistment(enlistment.Transaction);
        }
    }
}

