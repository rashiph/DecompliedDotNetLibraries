namespace System.Transactions
{
    using System;

    internal class Phase1VolatileEnlistment : InternalEnlistment
    {
        public Phase1VolatileEnlistment(Enlistment enlistment, InternalTransaction transaction, IEnlistmentNotification twoPhaseNotifications, ISinglePhaseNotification singlePhaseNotifications, Transaction atomicTransaction) : base(enlistment, transaction, twoPhaseNotifications, singlePhaseNotifications, atomicTransaction)
        {
        }

        internal override void CheckComplete()
        {
            if (base.transaction.phase1Volatiles.preparedVolatileEnlistments == (base.transaction.phase1Volatiles.volatileEnlistmentCount + base.transaction.phase1Volatiles.dependentClones))
            {
                base.transaction.State.Phase1VolatilePrepareDone(base.transaction);
            }
        }

        internal override void FinishEnlistment()
        {
            base.transaction.phase1Volatiles.preparedVolatileEnlistments++;
            this.CheckComplete();
        }
    }
}

