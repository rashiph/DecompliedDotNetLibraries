namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    internal class TransactionStatePromotedIndoubt : TransactionStatePromotedEnded
    {
        internal override void ChangeStatePromotedAborted(InternalTransaction tx)
        {
        }

        internal override void ChangeStatePromotedCommitted(InternalTransaction tx)
        {
        }

        internal override void ChangeStatePromotedPhase0(InternalTransaction tx)
        {
            throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceBase"), tx.innerException);
        }

        internal override void ChangeStatePromotedPhase1(InternalTransaction tx)
        {
            throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceBase"), tx.innerException);
        }

        internal override void CheckForFinishedTransaction(InternalTransaction tx)
        {
            throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceBase"), tx.innerException);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.EnterState(tx);
            if (tx.phase1Volatiles.VolatileDemux != null)
            {
                tx.phase1Volatiles.VolatileDemux.BroadcastInDoubt(ref tx.phase1Volatiles);
            }
            if (tx.phase0Volatiles.VolatileDemux != null)
            {
                tx.phase0Volatiles.VolatileDemux.BroadcastInDoubt(ref tx.phase0Volatiles);
            }
            tx.FireCompletion();
            if (DiagnosticTrace.Warning)
            {
                TransactionInDoubtTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
            }
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.InDoubt;
        }

        internal override void GetObjectData(InternalTransaction tx, SerializationInfo serializationInfo, StreamingContext context)
        {
            throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceBase"), tx.innerException);
        }

        internal override void InDoubtFromDtc(InternalTransaction tx)
        {
        }

        internal override void InDoubtFromEnlistment(InternalTransaction tx)
        {
        }

        protected override void PromotedTransactionOutcome(InternalTransaction tx)
        {
            if ((tx.innerException == null) && (tx.PromotedTransaction != null))
            {
                tx.innerException = tx.PromotedTransaction.InnerException;
            }
            throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceBase"), tx.innerException);
        }

        internal override void RestartCommitIfNeeded(InternalTransaction tx)
        {
        }
    }
}

