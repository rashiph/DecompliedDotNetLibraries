namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    internal class TransactionStatePromotedAborted : TransactionStatePromotedEnded
    {
        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            throw TransactionAbortedException.Create(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void ChangeStatePromotedAborted(InternalTransaction tx)
        {
        }

        internal override void ChangeStatePromotedPhase0(InternalTransaction tx)
        {
            throw new TransactionAbortedException(tx.innerException);
        }

        internal override void ChangeStatePromotedPhase1(InternalTransaction tx)
        {
            throw new TransactionAbortedException(tx.innerException);
        }

        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
        }

        internal override void CheckForFinishedTransaction(InternalTransaction tx)
        {
            throw new TransactionAbortedException(tx.innerException);
        }

        internal override void CreateAbortingClone(InternalTransaction tx)
        {
            throw TransactionAbortedException.Create(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void CreateBlockingClone(InternalTransaction tx)
        {
            throw TransactionAbortedException.Create(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.EnterState(tx);
            if (tx.phase1Volatiles.VolatileDemux != null)
            {
                tx.phase1Volatiles.VolatileDemux.BroadcastRollback(ref tx.phase1Volatiles);
            }
            if (tx.phase0Volatiles.VolatileDemux != null)
            {
                tx.phase0Volatiles.VolatileDemux.BroadcastRollback(ref tx.phase0Volatiles);
            }
            tx.FireCompletion();
            if (DiagnosticTrace.Warning)
            {
                TransactionAbortedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
            }
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.Aborted;
        }

        internal override void GetObjectData(InternalTransaction tx, SerializationInfo serializationInfo, StreamingContext context)
        {
            throw TransactionAbortedException.Create(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void InDoubtFromDtc(InternalTransaction tx)
        {
        }

        internal override void InDoubtFromEnlistment(InternalTransaction tx)
        {
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
        }

        protected override void PromotedTransactionOutcome(InternalTransaction tx)
        {
            if ((tx.innerException == null) && (tx.PromotedTransaction != null))
            {
                tx.innerException = tx.PromotedTransaction.InnerException;
            }
            throw TransactionAbortedException.Create(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal override void RestartCommitIfNeeded(InternalTransaction tx)
        {
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
        }
    }
}

