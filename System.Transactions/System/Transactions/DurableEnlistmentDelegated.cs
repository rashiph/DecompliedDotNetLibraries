namespace System.Transactions
{
    using System;

    internal class DurableEnlistmentDelegated : DurableEnlistmentState
    {
        internal override void Aborted(InternalEnlistment enlistment, Exception e)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
            if (enlistment.Transaction.innerException == null)
            {
                enlistment.Transaction.innerException = e;
            }
            enlistment.Transaction.State.ChangeStatePromotedAborted(enlistment.Transaction);
        }

        internal override void Committed(InternalEnlistment enlistment)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
            enlistment.Transaction.State.ChangeStatePromotedCommitted(enlistment.Transaction);
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
        }

        internal override void InDoubt(InternalEnlistment enlistment, Exception e)
        {
            DurableEnlistmentState._DurableEnlistmentEnded.EnterState(enlistment);
            if (enlistment.Transaction.innerException == null)
            {
                enlistment.Transaction.innerException = e;
            }
            enlistment.Transaction.State.InDoubtFromEnlistment(enlistment.Transaction);
        }
    }
}

