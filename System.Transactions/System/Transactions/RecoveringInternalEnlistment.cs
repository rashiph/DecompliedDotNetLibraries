namespace System.Transactions
{
    using System;

    internal class RecoveringInternalEnlistment : DurableInternalEnlistment
    {
        private object syncRoot;

        internal RecoveringInternalEnlistment(Enlistment enlistment, IEnlistmentNotification twoPhaseNotifications, object syncRoot) : base(enlistment, twoPhaseNotifications)
        {
            this.syncRoot = syncRoot;
        }

        internal override object SyncRoot
        {
            get
            {
                return this.syncRoot;
            }
        }
    }
}

