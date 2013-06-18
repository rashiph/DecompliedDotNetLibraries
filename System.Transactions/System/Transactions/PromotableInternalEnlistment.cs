namespace System.Transactions
{
    using System;

    internal class PromotableInternalEnlistment : InternalEnlistment
    {
        private IPromotableSinglePhaseNotification promotableNotificationInterface;

        internal PromotableInternalEnlistment(Enlistment enlistment, InternalTransaction transaction, IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Transaction atomicTransaction) : base(enlistment, transaction, atomicTransaction)
        {
            this.promotableNotificationInterface = promotableSinglePhaseNotification;
        }

        internal override IPromotableSinglePhaseNotification PromotableSinglePhaseNotification
        {
            get
            {
                return this.promotableNotificationInterface;
            }
        }
    }
}

