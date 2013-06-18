namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class VolatileEnlistmentCommitting : VolatileEnlistmentState
    {
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
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), enlistment.EnlistmentTraceId, NotificationCall.Commit);
                }
                enlistment.EnlistmentNotification.Commit(enlistment.Enlistment);
            }
            finally
            {
                Monitor.Enter(enlistment.Transaction);
            }
        }
    }
}

