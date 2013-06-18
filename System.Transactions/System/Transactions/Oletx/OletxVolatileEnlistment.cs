namespace System.Transactions.Oletx
{
    using System;
    using System.Threading;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class OletxVolatileEnlistment : OletxBaseEnlistment, IPromotedEnlistment
    {
        private OletxVolatileEnlistmentContainer container;
        internal bool enlistDuringPrepareRequired;
        private IEnlistmentNotificationInternal iEnlistmentNotification;
        private TransactionStatus pendingOutcome;
        private OletxVolatileEnlistmentState state;

        internal OletxVolatileEnlistment(IEnlistmentNotificationInternal enlistmentNotification, EnlistmentOptions enlistmentOptions, OletxTransaction oletxTransaction) : base(null, oletxTransaction)
        {
            this.iEnlistmentNotification = enlistmentNotification;
            this.enlistDuringPrepareRequired = (enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None;
            this.container = null;
            this.pendingOutcome = TransactionStatus.Active;
            if (DiagnosticTrace.Information)
            {
                EnlistmentTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentType.Volatile, enlistmentOptions);
            }
        }

        internal void Commit()
        {
            OletxVolatileEnlistmentState active = OletxVolatileEnlistmentState.Active;
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            lock (this)
            {
                if (OletxVolatileEnlistmentState.Prepared == this.state)
                {
                    active = this.state = OletxVolatileEnlistmentState.Committing;
                    iEnlistmentNotification = this.iEnlistmentNotification;
                }
                else
                {
                    active = this.state;
                }
            }
            if (OletxVolatileEnlistmentState.Committing == active)
            {
                if (iEnlistmentNotification != null)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Commit);
                    }
                    iEnlistmentNotification.Commit(this);
                    return;
                }
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                }
                throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
            if (OletxVolatileEnlistmentState.Done != active)
            {
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                }
                throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
        }

        internal void InDoubt()
        {
            OletxVolatileEnlistmentState active = OletxVolatileEnlistmentState.Active;
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            lock (this)
            {
                if (OletxVolatileEnlistmentState.Prepared == this.state)
                {
                    active = this.state = OletxVolatileEnlistmentState.InDoubt;
                    iEnlistmentNotification = this.iEnlistmentNotification;
                }
                else
                {
                    if (OletxVolatileEnlistmentState.Preparing == this.state)
                    {
                        this.pendingOutcome = TransactionStatus.InDoubt;
                    }
                    active = this.state;
                }
            }
            if (OletxVolatileEnlistmentState.InDoubt == active)
            {
                if (iEnlistmentNotification != null)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.InDoubt);
                    }
                    iEnlistmentNotification.InDoubt(this);
                    return;
                }
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                }
                throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
            if ((OletxVolatileEnlistmentState.Preparing != active) && (OletxVolatileEnlistmentState.Done != active))
            {
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                }
                throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
        }

        internal void Prepare(OletxVolatileEnlistmentContainer container)
        {
            OletxVolatileEnlistmentState active = OletxVolatileEnlistmentState.Active;
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            lock (this)
            {
                iEnlistmentNotification = this.iEnlistmentNotification;
                if (this.state == OletxVolatileEnlistmentState.Active)
                {
                    active = this.state = OletxVolatileEnlistmentState.Preparing;
                }
                else
                {
                    active = this.state;
                }
                this.container = container;
            }
            if (OletxVolatileEnlistmentState.Preparing == active)
            {
                if (iEnlistmentNotification != null)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Prepare);
                    }
                    iEnlistmentNotification.Prepare(this);
                    return;
                }
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                }
                throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
            if (OletxVolatileEnlistmentState.Done == active)
            {
                container.DecrementOutstandingNotifications(true);
            }
            else if ((OletxVolatileEnlistmentState.Prepared == active) && this.enlistDuringPrepareRequired)
            {
                container.DecrementOutstandingNotifications(true);
            }
            else if ((OletxVolatileEnlistmentState.Aborting == active) || (OletxVolatileEnlistmentState.Aborted == active))
            {
                container.DecrementOutstandingNotifications(false);
            }
            else
            {
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                }
                throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
        }

        internal void Rollback()
        {
            OletxVolatileEnlistmentState active = OletxVolatileEnlistmentState.Active;
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            lock (this)
            {
                if ((OletxVolatileEnlistmentState.Prepared == this.state) || (this.state == OletxVolatileEnlistmentState.Active))
                {
                    active = this.state = OletxVolatileEnlistmentState.Aborting;
                    iEnlistmentNotification = this.iEnlistmentNotification;
                }
                else
                {
                    if (OletxVolatileEnlistmentState.Preparing == this.state)
                    {
                        this.pendingOutcome = TransactionStatus.Aborted;
                    }
                    active = this.state;
                }
            }
            if (OletxVolatileEnlistmentState.Aborting == active)
            {
                if (iEnlistmentNotification != null)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Rollback);
                    }
                    iEnlistmentNotification.Rollback(this);
                }
            }
            else if ((OletxVolatileEnlistmentState.Preparing != active) && (OletxVolatileEnlistmentState.Done != active))
            {
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                }
                throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
        }

        void IPromotedEnlistment.Aborted()
        {
            throw new InvalidOperationException();
        }

        void IPromotedEnlistment.Aborted(Exception e)
        {
            throw new InvalidOperationException();
        }

        void IPromotedEnlistment.Committed()
        {
            throw new InvalidOperationException();
        }

        void IPromotedEnlistment.EnlistmentDone()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.EnlistmentDone");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.Done);
            }
            OletxVolatileEnlistmentState active = OletxVolatileEnlistmentState.Active;
            OletxVolatileEnlistmentContainer container = null;
            lock (this)
            {
                active = this.state;
                container = this.container;
                if ((((this.state != OletxVolatileEnlistmentState.Active) && (OletxVolatileEnlistmentState.Preparing != this.state)) && ((OletxVolatileEnlistmentState.Aborting != this.state) && (OletxVolatileEnlistmentState.Committing != this.state))) && (OletxVolatileEnlistmentState.InDoubt != this.state))
                {
                    throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                this.state = OletxVolatileEnlistmentState.Done;
            }
            if ((OletxVolatileEnlistmentState.Preparing == active) && (container != null))
            {
                container.DecrementOutstandingNotifications(true);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.EnlistmentDone");
            }
        }

        void IPromotedEnlistment.ForceRollback()
        {
            ((IPromotedEnlistment) this).ForceRollback(null);
        }

        void IPromotedEnlistment.ForceRollback(Exception e)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.ForceRollback");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.ForceRollback);
            }
            OletxVolatileEnlistmentContainer container = null;
            lock (this)
            {
                if (OletxVolatileEnlistmentState.Preparing != this.state)
                {
                    throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                this.state = OletxVolatileEnlistmentState.Done;
                if (this.container == null)
                {
                    if (DiagnosticTrace.Critical)
                    {
                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
                }
                container = this.container;
            }
            Interlocked.CompareExchange<Exception>(ref base.oletxTransaction.realOletxTransaction.innerException, e, null);
            container.DecrementOutstandingNotifications(false);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.ForceRollback");
            }
        }

        byte[] IPromotedEnlistment.GetRecoveryInformation()
        {
            throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("VolEnlistNoRecoveryInfo"), null);
        }

        void IPromotedEnlistment.InDoubt()
        {
            throw new InvalidOperationException();
        }

        void IPromotedEnlistment.InDoubt(Exception e)
        {
            throw new InvalidOperationException();
        }

        void IPromotedEnlistment.Prepared()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.Prepared");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.Prepared);
            }
            OletxVolatileEnlistmentContainer container = null;
            TransactionStatus active = TransactionStatus.Active;
            lock (this)
            {
                if (OletxVolatileEnlistmentState.Preparing != this.state)
                {
                    throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                this.state = OletxVolatileEnlistmentState.Prepared;
                active = this.pendingOutcome;
                if (this.container == null)
                {
                    if (DiagnosticTrace.Critical)
                    {
                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
                }
                container = this.container;
            }
            container.DecrementOutstandingNotifications(true);
            switch (active)
            {
                case TransactionStatus.Active:
                    break;

                case TransactionStatus.Aborted:
                    this.Rollback();
                    break;

                case TransactionStatus.InDoubt:
                    this.InDoubt();
                    break;

                default:
                    if (DiagnosticTrace.Critical)
                    {
                        InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "");
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InternalError"));
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.Prepared");
            }
        }

        InternalEnlistment IPromotedEnlistment.InternalEnlistment
        {
            get
            {
                return base.internalEnlistment;
            }
            set
            {
                base.internalEnlistment = value;
            }
        }

        private enum OletxVolatileEnlistmentState
        {
            Active,
            Preparing,
            Committing,
            Aborting,
            Prepared,
            Aborted,
            InDoubt,
            Done
        }
    }
}

