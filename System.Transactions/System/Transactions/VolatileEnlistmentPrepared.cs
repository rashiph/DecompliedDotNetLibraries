namespace System.Transactions
{
    using System;

    internal class VolatileEnlistmentPrepared : VolatileEnlistmentState
    {
        internal override void ChangeStatePreparing(InternalEnlistment enlistment)
        {
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
        }

        internal override void InternalAborted(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentAborting.EnterState(enlistment);
        }

        internal override void InternalCommitted(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentCommitting.EnterState(enlistment);
        }

        internal override void InternalIndoubt(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentInDoubt.EnterState(enlistment);
        }
    }
}

