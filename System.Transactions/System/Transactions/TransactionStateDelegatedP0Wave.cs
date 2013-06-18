namespace System.Transactions
{
    using System;

    internal class TransactionStateDelegatedP0Wave : TransactionStatePromotedP0Wave
    {
        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
            TransactionState._TransactionStateDelegatedCommitting.EnterState(tx);
        }
    }
}

