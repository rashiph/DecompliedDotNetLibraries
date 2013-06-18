namespace System.Transactions
{
    using System;

    internal class DurableInternalEnlistment : InternalEnlistment
    {
        internal Guid resourceManagerIdentifier;

        protected DurableInternalEnlistment(Enlistment enlistment, IEnlistmentNotification twoPhaseNotifications) : base(enlistment, twoPhaseNotifications)
        {
        }

        internal DurableInternalEnlistment(Enlistment enlistment, Guid resourceManagerIdentifier, InternalTransaction transaction, IEnlistmentNotification twoPhaseNotifications, ISinglePhaseNotification singlePhaseNotifications, Transaction atomicTransaction) : base(enlistment, transaction, twoPhaseNotifications, singlePhaseNotifications, atomicTransaction)
        {
            this.resourceManagerIdentifier = resourceManagerIdentifier;
        }

        internal override Guid ResourceManagerIdentifier
        {
            get
            {
                return this.resourceManagerIdentifier;
            }
        }
    }
}

