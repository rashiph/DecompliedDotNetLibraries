namespace System.Transactions
{
    using System;
    using System.Threading;

    internal class TransactionStatePromotedP0Aborting : TransactionStatePromotedAborting
    {
        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            this.ChangeStatePromotedAborted(tx);
            if (tx.phase0Volatiles.VolatileDemux.preparingEnlistment != null)
            {
                Monitor.Exit(tx);
                try
                {
                    tx.phase0Volatiles.VolatileDemux.oletxEnlistment.ForceRollback();
                }
                finally
                {
                    Monitor.Enter(tx);
                }
            }
            else
            {
                tx.PromotedTransaction.Rollback();
            }
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
        }
    }
}

