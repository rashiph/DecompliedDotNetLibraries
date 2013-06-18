namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    internal class TransactionStateInDoubt : TransactionStateEnded
    {
        internal override void CheckForFinishedTransaction(InternalTransaction tx)
        {
            throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceBase"), tx.innerException);
        }

        internal override void EndCommit(InternalTransaction tx)
        {
            throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceBase"), tx.innerException);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.EnterState(tx);
            base.CommonEnterState(tx);
            for (int i = 0; i < tx.phase0Volatiles.volatileEnlistmentCount; i++)
            {
                tx.phase0Volatiles.volatileEnlistments[i].twoPhaseState.InternalIndoubt(tx.phase0Volatiles.volatileEnlistments[i]);
            }
            for (int j = 0; j < tx.phase1Volatiles.volatileEnlistmentCount; j++)
            {
                tx.phase1Volatiles.volatileEnlistments[j].twoPhaseState.InternalIndoubt(tx.phase1Volatiles.volatileEnlistments[j]);
            }
            TransactionManager.TransactionTable.Remove(tx);
            if (DiagnosticTrace.Warning)
            {
                TransactionInDoubtTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
            }
            tx.FireCompletion();
            if (tx.asyncCommit)
            {
                tx.SignalAsyncCompletion();
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

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }
    }
}

