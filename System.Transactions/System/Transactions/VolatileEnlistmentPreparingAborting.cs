namespace System.Transactions
{
    using System;

    internal class VolatileEnlistmentPreparingAborting : VolatileEnlistmentState
    {
        internal override void EnlistmentDone(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
        }

        internal override void ForceRollback(InternalEnlistment enlistment, Exception e)
        {
            VolatileEnlistmentState._VolatileEnlistmentEnded.EnterState(enlistment);
            if (enlistment.Transaction.innerException == null)
            {
                enlistment.Transaction.innerException = e;
            }
            enlistment.FinishEnlistment();
        }

        internal override void InternalAborted(InternalEnlistment enlistment)
        {
        }

        internal override void Prepared(InternalEnlistment enlistment)
        {
            VolatileEnlistmentState._VolatileEnlistmentAborting.EnterState(enlistment);
            enlistment.FinishEnlistment();
        }
    }
}

