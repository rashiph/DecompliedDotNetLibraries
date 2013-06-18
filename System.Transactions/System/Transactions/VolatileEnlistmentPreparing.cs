namespace System.Transactions
{
    using System;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal class VolatileEnlistmentPreparing : VolatileEnlistmentState
    {
        internal override void ChangeStatePreparing(InternalEnlistment enlistment)
        {
        }

        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentDone.EnterState(enlistment);
            enlistment.FinishEnlistment();
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
            Monitor.Exit(enlistment.Transaction);
            try
            {
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), enlistment.EnlistmentTraceId, NotificationCall.Prepare);
                }
                enlistment.EnlistmentNotification.Prepare(enlistment.PreparingEnlistment);
            }
            finally
            {
                Monitor.Enter(enlistment.Transaction);
            }
        }

        internal override void ForceRollback(InternalEnlistment enlistment, Exception e)
        {
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStateTransactionAborted(enlistment.Transaction, e);
            enlistment.FinishEnlistment();
        }

        internal override void InternalAborted(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentPreparingAborting.EnterState(enlistment);
        }

        internal override void Prepared(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentPrepared.EnterState(enlistment);
            enlistment.FinishEnlistment();
        }
    }
}

