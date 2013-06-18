namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    internal class Phase1VolatileDemultiplexer : VolatileDemultiplexer
    {
        public Phase1VolatileDemultiplexer(InternalTransaction transaction) : base(transaction)
        {
        }

        public override void Commit(IPromotedEnlistment en)
        {
            base.oletxEnlistment = en;
            VolatileDemultiplexer.PoolableCommit(this);
        }

        public override void InDoubt(IPromotedEnlistment en)
        {
            base.oletxEnlistment = en;
            VolatileDemultiplexer.PoolableInDoubt(this);
        }

        protected override void InternalCommit()
        {
            base.oletxEnlistment.EnlistmentDone();
            base.transaction.State.ChangeStatePromotedCommitted(base.transaction);
        }

        protected override void InternalInDoubt()
        {
            base.transaction.State.InDoubtFromDtc(base.transaction);
        }

        protected override void InternalPrepare()
        {
            try
            {
                base.transaction.State.ChangeStatePromotedPhase1(base.transaction);
            }
            catch (TransactionAbortedException exception)
            {
                base.oletxEnlistment.ForceRollback(exception);
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception);
                }
            }
            catch (TransactionInDoubtException exception2)
            {
                base.oletxEnlistment.EnlistmentDone();
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception2);
                }
            }
        }

        protected override void InternalRollback()
        {
            base.oletxEnlistment.EnlistmentDone();
            base.transaction.State.ChangeStatePromotedAborted(base.transaction);
        }

        public override void Prepare(IPromotedEnlistment en)
        {
            base.preparingEnlistment = en;
            VolatileDemultiplexer.PoolablePrepare(this);
        }

        public override void Rollback(IPromotedEnlistment en)
        {
            base.oletxEnlistment = en;
            VolatileDemultiplexer.PoolableRollback(this);
        }
    }
}

