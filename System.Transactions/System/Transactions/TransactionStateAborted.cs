namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    internal class TransactionStateAborted : TransactionStateEnded
    {
        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            throw this.CreateTransactionAbortedException(tx);
        }

        internal override void ChangeStateAbortedDuringPromotion(InternalTransaction tx)
        {
        }

        internal override void ChangeStatePromotedAborted(InternalTransaction tx)
        {
        }

        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
        }

        internal override void CheckForFinishedTransaction(InternalTransaction tx)
        {
            throw this.CreateTransactionAbortedException(tx);
        }

        internal override void CreateAbortingClone(InternalTransaction tx)
        {
            throw this.CreateTransactionAbortedException(tx);
        }

        internal override void CreateBlockingClone(InternalTransaction tx)
        {
            throw this.CreateTransactionAbortedException(tx);
        }

        private TransactionException CreateTransactionAbortedException(InternalTransaction tx)
        {
            return TransactionAbortedException.Create(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("TransactionAborted"), tx.innerException);
        }

        internal override void EndCommit(InternalTransaction tx)
        {
            throw this.CreateTransactionAbortedException(tx);
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.EnterState(tx);
            base.CommonEnterState(tx);
            for (int i = 0; i < tx.phase0Volatiles.volatileEnlistmentCount; i++)
            {
                tx.phase0Volatiles.volatileEnlistments[i].twoPhaseState.InternalAborted(tx.phase0Volatiles.volatileEnlistments[i]);
            }
            for (int j = 0; j < tx.phase1Volatiles.volatileEnlistmentCount; j++)
            {
                tx.phase1Volatiles.volatileEnlistments[j].twoPhaseState.InternalAborted(tx.phase1Volatiles.volatileEnlistments[j]);
            }
            if (tx.durableEnlistment != null)
            {
                tx.durableEnlistment.State.InternalAborted(tx.durableEnlistment);
            }
            TransactionManager.TransactionTable.Remove(tx);
            if (DiagnosticTrace.Warning)
            {
                TransactionAbortedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
            }
            tx.FireCompletion();
            if (tx.asyncCommit)
            {
                tx.SignalAsyncCompletion();
            }
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.Aborted;
        }

        internal override void GetObjectData(InternalTransaction tx, SerializationInfo serializationInfo, StreamingContext context)
        {
            throw this.CreateTransactionAbortedException(tx);
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void RestartCommitIfNeeded(InternalTransaction tx)
        {
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
        }

        internal override void Timeout(InternalTransaction tx)
        {
        }
    }
}

