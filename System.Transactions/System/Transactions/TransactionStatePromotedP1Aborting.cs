namespace System.Transactions
{
    using System;
    using System.Threading;

    internal class TransactionStatePromotedP1Aborting : TransactionStatePromotedAborting
    {
        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            this.ChangeStatePromotedAborted(tx);
            Monitor.Exit(tx);
            try
            {
                tx.phase1Volatiles.VolatileDemux.oletxEnlistment.ForceRollback();
            }
            finally
            {
                Monitor.Enter(tx);
            }
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
        }
    }
}

