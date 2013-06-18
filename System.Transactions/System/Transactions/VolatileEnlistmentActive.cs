namespace System.Transactions
{
    using System;

    internal class VolatileEnlistmentActive : VolatileEnlistmentState
    {
        internal override void ChangeStatePreparing(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentPreparing.EnterState(enlistment);
        }

        internal override void ChangeStateSinglePhaseCommit(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentSPC.EnterState(enlistment);
        }

        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentDone.EnterState(enlistment);
            enlistment.FinishEnlistment();
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
        }

        internal override void InternalAborted(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentAborting.EnterState(enlistment);
        }
    }
}

