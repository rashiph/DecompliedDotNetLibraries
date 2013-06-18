namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    internal abstract class EnlistableStates : ActiveStates
    {
        protected EnlistableStates()
        {
        }

        internal override void CompleteAbortingClone(InternalTransaction tx)
        {
            tx.phase1Volatiles.dependentClones--;
        }

        internal override void CompleteBlockingClone(InternalTransaction tx)
        {
            tx.phase0Volatiles.dependentClones--;
            if (tx.phase0Volatiles.preparedVolatileEnlistments == (tx.phase0VolatileWaveCount + tx.phase0Volatiles.dependentClones))
            {
                tx.State.Phase0VolatilePrepareDone(tx);
            }
        }

        internal override void CreateAbortingClone(InternalTransaction tx)
        {
            tx.phase1Volatiles.dependentClones++;
        }

        internal override void CreateBlockingClone(InternalTransaction tx)
        {
            tx.phase0Volatiles.dependentClones++;
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            tx.promoteState.EnterState(tx);
            return tx.State.EnlistDurable(tx, resourceManagerIdentifier, enlistmentNotification, enlistmentOptions, atomicTransaction);
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            if ((tx.durableEnlistment != null) || ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None))
            {
                tx.promoteState.EnterState(tx);
                return tx.State.EnlistDurable(tx, resourceManagerIdentifier, enlistmentNotification, enlistmentOptions, atomicTransaction);
            }
            Enlistment enlistment = new Enlistment(resourceManagerIdentifier, tx, enlistmentNotification, enlistmentNotification, atomicTransaction);
            tx.durableEnlistment = enlistment.InternalEnlistment;
            DurableEnlistmentState._DurableEnlistmentActive.EnterState(tx.durableEnlistment);
            if (DiagnosticTrace.Information)
            {
                EnlistmentTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.durableEnlistment.EnlistmentTraceId, EnlistmentType.Durable, EnlistmentOptions.None);
            }
            return enlistment;
        }

        internal override void GetObjectData(InternalTransaction tx, SerializationInfo serializationInfo, StreamingContext context)
        {
            tx.promoteState.EnterState(tx);
            tx.State.GetObjectData(tx, serializationInfo, context);
        }

        internal override void Promote(InternalTransaction tx)
        {
            tx.promoteState.EnterState(tx);
            tx.State.CheckForFinishedTransaction(tx);
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

