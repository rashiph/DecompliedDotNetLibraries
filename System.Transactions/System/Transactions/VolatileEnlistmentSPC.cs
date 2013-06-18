namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class VolatileEnlistmentSPC : VolatileEnlistmentState
    {
        internal override void Aborted(InternalEnlistment enlistment, Exception e)
        {
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStateTransactionAborted(enlistment.Transaction, e);
        }

        internal override void Committed(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStateTransactionCommitted(enlistment.Transaction);
        }

        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStateTransactionCommitted(enlistment.Transaction);
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            bool flag = false;
            enlistment.State = this;
            if (DiagnosticTrace.Verbose)
            {
                EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), enlistment.EnlistmentTraceId, NotificationCall.SinglePhaseCommit);
            }
            Monitor.Exit(enlistment.Transaction);
            try
            {
                enlistment.SinglePhaseNotification.SinglePhaseCommit(enlistment.SinglePhaseEnlistment);
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
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
            if (enlistment.Transaction.innerException == null)
            {
                enlistment.Transaction.innerException = e;
            }
            enlistment.Transaction.State.InDoubtFromEnlistment(enlistment.Transaction);
        }
    }
}

