namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    public class Enlistment
    {
        internal System.Transactions.InternalEnlistment internalEnlistment;

        internal Enlistment(System.Transactions.InternalEnlistment internalEnlistment)
        {
            this.internalEnlistment = internalEnlistment;
        }

        internal Enlistment(IEnlistmentNotification twoPhaseNotifications, object syncRoot)
        {
            this.internalEnlistment = new RecoveringInternalEnlistment(this, twoPhaseNotifications, syncRoot);
        }

        internal Enlistment(IEnlistmentNotification twoPhaseNotifications, InternalTransaction transaction, Transaction atomicTransaction)
        {
            this.internalEnlistment = new System.Transactions.InternalEnlistment(this, twoPhaseNotifications, transaction, atomicTransaction);
        }

        internal Enlistment(InternalTransaction transaction, IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Transaction atomicTransaction)
        {
            this.internalEnlistment = new PromotableInternalEnlistment(this, transaction, promotableSinglePhaseNotification, atomicTransaction);
        }

        internal Enlistment(Guid resourceManagerIdentifier, InternalTransaction transaction, IEnlistmentNotification twoPhaseNotifications, ISinglePhaseNotification singlePhaseNotifications, Transaction atomicTransaction)
        {
            this.internalEnlistment = new DurableInternalEnlistment(this, resourceManagerIdentifier, transaction, twoPhaseNotifications, singlePhaseNotifications, atomicTransaction);
        }

        internal Enlistment(InternalTransaction transaction, IEnlistmentNotification twoPhaseNotifications, ISinglePhaseNotification singlePhaseNotifications, Transaction atomicTransaction, EnlistmentOptions enlistmentOptions)
        {
            if ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)
            {
                this.internalEnlistment = new System.Transactions.InternalEnlistment(this, transaction, twoPhaseNotifications, singlePhaseNotifications, atomicTransaction);
            }
            else
            {
                this.internalEnlistment = new Phase1VolatileEnlistment(this, transaction, twoPhaseNotifications, singlePhaseNotifications, atomicTransaction);
            }
        }

        public void Done()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Enlistment.Done");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), this.internalEnlistment.EnlistmentTraceId, EnlistmentCallback.Done);
            }
            lock (this.internalEnlistment.SyncRoot)
            {
                this.internalEnlistment.State.EnlistmentDone(this.internalEnlistment);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Enlistment.Done");
            }
        }

        internal System.Transactions.InternalEnlistment InternalEnlistment
        {
            get
            {
                return this.internalEnlistment;
            }
        }
    }
}

