namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    internal class TransactionStatePhase0 : EnlistableStates
    {
        internal override void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStateAborted.EnterState(tx);
        }

        internal override bool ContinuePhase0Prepares()
        {
            return true;
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment = base.EnlistDurable(tx, resourceManagerIdentifier, enlistmentNotification, enlistmentOptions, atomicTransaction);
            tx.State.RestartCommitIfNeeded(tx);
            return enlistment;
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment = base.EnlistDurable(tx, resourceManagerIdentifier, enlistmentNotification, enlistmentOptions, atomicTransaction);
            tx.State.RestartCommitIfNeeded(tx);
            return enlistment;
        }

        internal override bool EnlistPromotableSinglePhase(InternalTransaction tx, IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Transaction atomicTransaction)
        {
            if (tx.durableEnlistment != null)
            {
                return false;
            }
            TransactionState._TransactionStatePSPEOperation.Phase0PSPEInitialize(tx, promotableSinglePhaseNotification);
            Enlistment enlistment = new Enlistment(tx, promotableSinglePhaseNotification, atomicTransaction);
            tx.durableEnlistment = enlistment.InternalEnlistment;
            if (DiagnosticTrace.Information)
            {
                EnlistmentTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.durableEnlistment.EnlistmentTraceId, EnlistmentType.PromotableSinglePhase, EnlistmentOptions.None);
            }
            tx.promoter = promotableSinglePhaseNotification;
            tx.promoteState = TransactionState._TransactionStateDelegated;
            DurableEnlistmentState._DurableEnlistmentActive.EnterState(tx.durableEnlistment);
            return true;
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment = new Enlistment(tx, enlistmentNotification, null, atomicTransaction, enlistmentOptions);
            if ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)
            {
                base.AddVolatileEnlistment(ref tx.phase0Volatiles, enlistment);
            }
            else
            {
                base.AddVolatileEnlistment(ref tx.phase1Volatiles, enlistment);
            }
            if (DiagnosticTrace.Information)
            {
                EnlistmentTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), enlistment.InternalEnlistment.EnlistmentTraceId, EnlistmentType.Volatile, enlistmentOptions);
            }
            return enlistment;
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment = new Enlistment(tx, enlistmentNotification, enlistmentNotification, atomicTransaction, enlistmentOptions);
            if ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)
            {
                base.AddVolatileEnlistment(ref tx.phase0Volatiles, enlistment);
            }
            else
            {
                base.AddVolatileEnlistment(ref tx.phase1Volatiles, enlistment);
            }
            if (DiagnosticTrace.Information)
            {
                EnlistmentTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), enlistment.InternalEnlistment.EnlistmentTraceId, EnlistmentType.Volatile, enlistmentOptions);
            }
            return enlistment;
        }

        internal override void EnterState(InternalTransaction tx)
        {
            base.CommonEnterState(tx);
            int volatileEnlistmentCount = tx.phase0Volatiles.volatileEnlistmentCount;
            int dependentClones = tx.phase0Volatiles.dependentClones;
            tx.phase0VolatileWaveCount = volatileEnlistmentCount;
            if (tx.phase0Volatiles.preparedVolatileEnlistments < (volatileEnlistmentCount + dependentClones))
            {
                for (int i = 0; i < volatileEnlistmentCount; i++)
                {
                    tx.phase0Volatiles.volatileEnlistments[i].twoPhaseState.ChangeStatePreparing(tx.phase0Volatiles.volatileEnlistments[i]);
                    if (!tx.State.ContinuePhase0Prepares())
                    {
                        return;
                    }
                }
            }
            else
            {
                TransactionState._TransactionStateVolatilePhase1.EnterState(tx);
            }
        }

        internal override void GetObjectData(InternalTransaction tx, SerializationInfo serializationInfo, StreamingContext context)
        {
            tx.promoteState.EnterState(tx);
            tx.State.GetObjectData(tx, serializationInfo, context);
            tx.State.RestartCommitIfNeeded(tx);
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
            int volatileEnlistmentCount = tx.phase0Volatiles.volatileEnlistmentCount;
            int dependentClones = tx.phase0Volatiles.dependentClones;
            tx.phase0VolatileWaveCount = volatileEnlistmentCount;
            if (tx.phase0Volatiles.preparedVolatileEnlistments < (volatileEnlistmentCount + dependentClones))
            {
                for (int i = 0; i < volatileEnlistmentCount; i++)
                {
                    tx.phase0Volatiles.volatileEnlistments[i].twoPhaseState.ChangeStatePreparing(tx.phase0Volatiles.volatileEnlistments[i]);
                    if (!tx.State.ContinuePhase0Prepares())
                    {
                        return;
                    }
                }
            }
            else
            {
                TransactionState._TransactionStateVolatilePhase1.EnterState(tx);
            }
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void Promote(InternalTransaction tx)
        {
            tx.promoteState.EnterState(tx);
            tx.State.CheckForFinishedTransaction(tx);
            tx.State.RestartCommitIfNeeded(tx);
        }

        internal override void RestartCommitIfNeeded(InternalTransaction tx)
        {
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            this.ChangeStateTransactionAborted(tx, e);
        }
    }
}

