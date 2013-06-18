namespace System.Transactions
{
    using System;
    using System.Globalization;
    using System.Threading;

    internal class InternalEnlistment : ISinglePhaseNotificationInternal, IEnlistmentNotificationInternal
    {
        private System.Transactions.Transaction atomicTransaction;
        private System.Transactions.Enlistment enlistment;
        private int enlistmentId;
        private System.Transactions.PreparingEnlistment preparingEnlistment;
        private IPromotedEnlistment promotedEnlistment;
        private System.Transactions.SinglePhaseEnlistment singlePhaseEnlistment;
        protected ISinglePhaseNotification singlePhaseNotifications;
        private EnlistmentTraceIdentifier traceIdentifier;
        protected InternalTransaction transaction;
        protected IEnlistmentNotification twoPhaseNotifications;
        internal EnlistmentState twoPhaseState;

        protected InternalEnlistment(System.Transactions.Enlistment enlistment, IEnlistmentNotification twoPhaseNotifications)
        {
            this.enlistment = enlistment;
            this.twoPhaseNotifications = twoPhaseNotifications;
            this.enlistmentId = 1;
            this.traceIdentifier = EnlistmentTraceIdentifier.Empty;
        }

        protected InternalEnlistment(System.Transactions.Enlistment enlistment, InternalTransaction transaction, System.Transactions.Transaction atomicTransaction)
        {
            this.enlistment = enlistment;
            this.transaction = transaction;
            this.atomicTransaction = atomicTransaction;
            this.enlistmentId = transaction.enlistmentCount++;
            this.traceIdentifier = EnlistmentTraceIdentifier.Empty;
        }

        internal InternalEnlistment(System.Transactions.Enlistment enlistment, IEnlistmentNotification twoPhaseNotifications, InternalTransaction transaction, System.Transactions.Transaction atomicTransaction)
        {
            this.enlistment = enlistment;
            this.twoPhaseNotifications = twoPhaseNotifications;
            this.transaction = transaction;
            this.atomicTransaction = atomicTransaction;
        }

        internal InternalEnlistment(System.Transactions.Enlistment enlistment, InternalTransaction transaction, IEnlistmentNotification twoPhaseNotifications, ISinglePhaseNotification singlePhaseNotifications, System.Transactions.Transaction atomicTransaction)
        {
            this.enlistment = enlistment;
            this.transaction = transaction;
            this.twoPhaseNotifications = twoPhaseNotifications;
            this.singlePhaseNotifications = singlePhaseNotifications;
            this.atomicTransaction = atomicTransaction;
            this.enlistmentId = transaction.enlistmentCount++;
            this.traceIdentifier = EnlistmentTraceIdentifier.Empty;
        }

        internal virtual void CheckComplete()
        {
            if (this.Transaction.phase0Volatiles.preparedVolatileEnlistments == (this.Transaction.phase0VolatileWaveCount + this.Transaction.phase0Volatiles.dependentClones))
            {
                this.Transaction.State.Phase0VolatilePrepareDone(this.Transaction);
            }
        }

        internal virtual void FinishEnlistment()
        {
            this.Transaction.phase0Volatiles.preparedVolatileEnlistments++;
            this.CheckComplete();
        }

        void IEnlistmentNotificationInternal.Commit(IPromotedEnlistment enlistment)
        {
            this.promotedEnlistment = enlistment;
            this.twoPhaseNotifications.Commit(this.Enlistment);
        }

        void IEnlistmentNotificationInternal.InDoubt(IPromotedEnlistment enlistment)
        {
            this.promotedEnlistment = enlistment;
            this.twoPhaseNotifications.InDoubt(this.Enlistment);
        }

        void IEnlistmentNotificationInternal.Prepare(IPromotedEnlistment preparingEnlistment)
        {
            this.promotedEnlistment = preparingEnlistment;
            this.twoPhaseNotifications.Prepare(this.PreparingEnlistment);
        }

        void IEnlistmentNotificationInternal.Rollback(IPromotedEnlistment enlistment)
        {
            this.promotedEnlistment = enlistment;
            this.twoPhaseNotifications.Rollback(this.Enlistment);
        }

        void ISinglePhaseNotificationInternal.SinglePhaseCommit(IPromotedEnlistment singlePhaseEnlistment)
        {
            bool flag = false;
            this.promotedEnlistment = singlePhaseEnlistment;
            try
            {
                this.singlePhaseNotifications.SinglePhaseCommit(this.SinglePhaseEnlistment);
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    this.SinglePhaseEnlistment.InDoubt();
                }
            }
        }

        internal System.Transactions.Enlistment Enlistment
        {
            get
            {
                return this.enlistment;
            }
        }

        internal IEnlistmentNotification EnlistmentNotification
        {
            get
            {
                return this.twoPhaseNotifications;
            }
        }

        internal EnlistmentTraceIdentifier EnlistmentTraceId
        {
            get
            {
                if (this.traceIdentifier == EnlistmentTraceIdentifier.Empty)
                {
                    lock (this.SyncRoot)
                    {
                        if (this.traceIdentifier == EnlistmentTraceIdentifier.Empty)
                        {
                            EnlistmentTraceIdentifier identifier;
                            if (null != this.atomicTransaction)
                            {
                                identifier = new EnlistmentTraceIdentifier(Guid.Empty, this.atomicTransaction.TransactionTraceId, this.enlistmentId);
                            }
                            else
                            {
                                identifier = new EnlistmentTraceIdentifier(Guid.Empty, new TransactionTraceIdentifier(InternalTransaction.InstanceIdentifier + Convert.ToString(Interlocked.Increment(ref InternalTransaction.nextHash), CultureInfo.InvariantCulture), 0), this.enlistmentId);
                            }
                            Thread.MemoryBarrier();
                            this.traceIdentifier = identifier;
                        }
                    }
                }
                return this.traceIdentifier;
            }
        }

        internal System.Transactions.PreparingEnlistment PreparingEnlistment
        {
            get
            {
                if (this.preparingEnlistment == null)
                {
                    this.preparingEnlistment = new System.Transactions.PreparingEnlistment(this);
                }
                return this.preparingEnlistment;
            }
        }

        internal virtual IPromotableSinglePhaseNotification PromotableSinglePhaseNotification
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal IPromotedEnlistment PromotedEnlistment
        {
            get
            {
                return this.promotedEnlistment;
            }
            set
            {
                this.promotedEnlistment = value;
            }
        }

        internal virtual Guid ResourceManagerIdentifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal System.Transactions.SinglePhaseEnlistment SinglePhaseEnlistment
        {
            get
            {
                if (this.singlePhaseEnlistment == null)
                {
                    this.singlePhaseEnlistment = new System.Transactions.SinglePhaseEnlistment(this);
                }
                return this.singlePhaseEnlistment;
            }
        }

        internal ISinglePhaseNotification SinglePhaseNotification
        {
            get
            {
                return this.singlePhaseNotifications;
            }
        }

        internal EnlistmentState State
        {
            get
            {
                return this.twoPhaseState;
            }
            set
            {
                this.twoPhaseState = value;
            }
        }

        internal virtual object SyncRoot
        {
            get
            {
                return this.transaction;
            }
        }

        internal InternalTransaction Transaction
        {
            get
            {
                return this.transaction;
            }
        }
    }
}

