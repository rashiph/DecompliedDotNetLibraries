namespace System.Transactions
{
    using System;

    internal class DurableEnlistmentActive : DurableEnlistmentState
    {
        internal override void ChangeStateCommitting(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentCommitting.EnterState(enlistment);
        }

        internal override void ChangeStateDelegated(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentDelegated.EnterState(enlistment);
        }

        internal override void ChangeStatePromoted(InternalEnlistment enlistment, IPromotedEnlistment promotedEnlistment)
        {
            enlistment.PromotedEnlistment = promotedEnlistment;
            EnlistmentState._EnlistmentStatePromoted.EnterState(enlistment);
        }

        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
        }

        internal override void InternalAborted(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentAborting.EnterState(enlistment);
        }
    }
}

