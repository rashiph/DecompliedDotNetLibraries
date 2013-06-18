namespace System.Transactions
{
    using System;

    internal class DurableEnlistmentEnded : DurableEnlistmentState
    {
        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
        }

        internal override void InDoubt(InternalEnlistment enlistment, Exception e)
        {
        }

        internal override void InternalAborted(InternalEnlistment enlistment)
        {
        }
    }
}

