namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class VolatileEnlistmentAborting : VolatileEnlistmentState
    {
        internal override void ChangeStatePreparing(InternalEnlistment enlistment)
        {
        }

        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
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
                enlistment.EnlistmentNotification.Rollback(enlistment.SinglePhaseEnlistment);
            }
            finally
            {
                Monitor.Enter(enlistment.Transaction);
            }
        }

        internal override void InternalAborted(InternalEnlistment enlistment)
        {
        }
    }
}

