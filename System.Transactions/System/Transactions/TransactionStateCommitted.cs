namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    internal class TransactionStateCommitted : TransactionStateEnded
    {
        internal override void EndCommit(InternalTransaction tx)
        {
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.EnterState(tx);
            base.CommonEnterState(tx);
            for (int i = 0; i < tx.phase0Volatiles.volatileEnlistmentCount; i++)
            {
                tx.phase0Volatiles.volatileEnlistments[i].twoPhaseState.InternalCommitted(tx.phase0Volatiles.volatileEnlistments[i]);
            }
            for (int j = 0; j < tx.phase1Volatiles.volatileEnlistmentCount; j++)
            {
                tx.phase1Volatiles.volatileEnlistments[j].twoPhaseState.InternalCommitted(tx.phase1Volatiles.volatileEnlistments[j]);
            }
            TransactionManager.TransactionTable.Remove(tx);
            if (DiagnosticTrace.Verbose)
            {
                TransactionCommittedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
            }
            tx.FireCompletion();
            if (tx.asyncCommit)
            {
                tx.SignalAsyncCompletion();
            }
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.Committed;
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }
    }
}

