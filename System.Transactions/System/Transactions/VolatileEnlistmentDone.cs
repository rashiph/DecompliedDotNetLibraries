namespace System.Transactions
{
    using System;

    internal class VolatileEnlistmentDone : VolatileEnlistmentEnded
    {
        internal override void ChangeStatePreparing(InternalEnlistment enlistment)
        {
            enlistment.CheckComplete();
        }

        internal override void EnterState(InternalEnlistment enlistment)
        {
            enlistment.State = this;
        }
    }
}

