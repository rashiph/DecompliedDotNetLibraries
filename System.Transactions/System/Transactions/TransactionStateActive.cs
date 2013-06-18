namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    internal class TransactionStateActive : EnlistableStates
    {
        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            tx.asyncCommit = asyncCommit;
            tx.asyncCallback = asyncCallback;
            tx.asyncState = asyncState;
            TransactionState._TransactionStatePhase0.EnterState(tx);
        }

        internal override void DisposeRoot(InternalTransaction tx)
        {
            tx.State.Rollback(tx, null);
        }

        internal override bool EnlistPromotableSinglePhase(InternalTransaction tx, IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Transaction atomicTransaction)
        {
            if (tx.durableEnlistment != null)
            {
                return false;
            }
            TransactionState._TransactionStatePSPEOperation.PSPEInitialize(tx, promotableSinglePhaseNotification);
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
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            TransactionState._TransactionStateAborted.EnterState(tx);
        }
    }
}

