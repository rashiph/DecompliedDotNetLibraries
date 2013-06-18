namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    internal class TransactionStateVolatilePhase1 : ActiveStates
    {
        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStateAborted.EnterState(tx);
        }

        internal override bool ContinuePhase1Prepares()
        {
            return true;
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            tx.committableTransaction.complete = true;
            if (tx.phase1Volatiles.dependentClones != 0)
            {
                TransactionState._TransactionStateAborted.EnterState(tx);
            }
            else if (((tx.phase1Volatiles.volatileEnlistmentCount == 1) && (tx.durableEnlistment == null)) && (tx.phase1Volatiles.volatileEnlistments[0].SinglePhaseNotification != null))
            {
                TransactionState._TransactionStateVolatileSPC.EnterState(tx);
            }
            else if (tx.phase1Volatiles.volatileEnlistmentCount > 0)
            {
                for (int i = 0; i < tx.phase1Volatiles.volatileEnlistmentCount; i++)
                {
                    tx.phase1Volatiles.volatileEnlistments[i].twoPhaseState.ChangeStatePreparing(tx.phase1Volatiles.volatileEnlistments[i]);
                    if (!tx.State.ContinuePhase1Prepares())
                    {
                        return;
                    }
                }
            }
            else
            {
                TransactionState._TransactionStateSPC.EnterState(tx);
            }
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
            TransactionState._TransactionStateSPC.EnterState(tx);
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            this.ChangeStateTransactionAborted(tx, e);
        }

        internal override void Timeout(InternalTransaction tx)
        {
            if (DiagnosticTrace.Warning)
            {
                TransactionTimeoutTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
            }
            TimeoutException e = new TimeoutException(System.Transactions.SR.GetString("TraceTransactionTimeout"));
            this.Rollback(tx, e);
        }
    }
}

