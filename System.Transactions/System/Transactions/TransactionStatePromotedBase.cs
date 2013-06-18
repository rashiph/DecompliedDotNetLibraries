namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Transactions.Diagnostics;
    using System.Transactions.Oletx;

    internal abstract class TransactionStatePromotedBase : TransactionState
    {
        protected TransactionStatePromotedBase()
        {
        }

        internal override void AddOutcomeRegistrant(InternalTransaction tx, TransactionCompletedEventHandler transactionCompletedDelegate)
        {
            tx.transactionCompletedDelegate = (TransactionCompletedEventHandler) Delegate.Combine(tx.transactionCompletedDelegate, transactionCompletedDelegate);
        }

        internal override void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            tx.asyncCommit = asyncCommit;
            tx.asyncCallback = asyncCallback;
            tx.asyncState = asyncState;
            TransactionState._TransactionStatePromotedCommitting.EnterState(tx);
        }

        internal override void ChangeStateAbortedDuringPromotion(InternalTransaction tx)
        {
            TransactionState._TransactionStateAborted.EnterState(tx);
        }

        internal override void ChangeStatePromotedAborted(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedAborted.EnterState(tx);
        }

        internal override void ChangeStatePromotedCommitted(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedCommitted.EnterState(tx);
        }

        internal override void CompleteAbortingClone(InternalTransaction tx)
        {
            if (tx.phase1Volatiles.VolatileDemux != null)
            {
                tx.phase1Volatiles.dependentClones--;
            }
            else
            {
                tx.abortingDependentCloneCount--;
                if (tx.abortingDependentCloneCount == 0)
                {
                    OletxDependentTransaction abortingDependentClone = tx.abortingDependentClone;
                    tx.abortingDependentClone = null;
                    Monitor.Exit(tx);
                    try
                    {
                        try
                        {
                            abortingDependentClone.Complete();
                        }
                        finally
                        {
                            abortingDependentClone.Dispose();
                        }
                    }
                    finally
                    {
                        Monitor.Enter(tx);
                    }
                }
            }
        }

        internal override void CompleteBlockingClone(InternalTransaction tx)
        {
            if (tx.phase0Volatiles.dependentClones > 0)
            {
                tx.phase0Volatiles.dependentClones--;
                if (tx.phase0Volatiles.preparedVolatileEnlistments == (tx.phase0VolatileWaveCount + tx.phase0Volatiles.dependentClones))
                {
                    tx.State.Phase0VolatilePrepareDone(tx);
                }
            }
            else
            {
                tx.phase0WaveDependentCloneCount--;
                if (tx.phase0WaveDependentCloneCount == 0)
                {
                    OletxDependentTransaction transaction = tx.phase0WaveDependentClone;
                    tx.phase0WaveDependentClone = null;
                    Monitor.Exit(tx);
                    try
                    {
                        try
                        {
                            transaction.Complete();
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                    finally
                    {
                        Monitor.Enter(tx);
                    }
                }
            }
        }

        internal override bool ContinuePhase0Prepares()
        {
            return true;
        }

        internal override void CreateAbortingClone(InternalTransaction tx)
        {
            if (tx.phase1Volatiles.VolatileDemux != null)
            {
                tx.phase1Volatiles.dependentClones++;
            }
            else
            {
                if (tx.abortingDependentClone == null)
                {
                    tx.abortingDependentClone = tx.PromotedTransaction.DependentClone(false);
                }
                tx.abortingDependentCloneCount++;
            }
        }

        internal override void CreateBlockingClone(InternalTransaction tx)
        {
            if (tx.phase0WaveDependentClone == null)
            {
                tx.phase0WaveDependentClone = tx.PromotedTransaction.DependentClone(true);
            }
            tx.phase0WaveDependentCloneCount++;
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment2;
            Monitor.Exit(tx);
            try
            {
                Enlistment enlistment = new Enlistment(resourceManagerIdentifier, tx, enlistmentNotification, null, atomicTransaction);
                EnlistmentState._EnlistmentStatePromoted.EnterState(enlistment.InternalEnlistment);
                enlistment.InternalEnlistment.PromotedEnlistment = tx.PromotedTransaction.EnlistDurable(resourceManagerIdentifier, (DurableInternalEnlistment) enlistment.InternalEnlistment, false, enlistmentOptions);
                enlistment2 = enlistment;
            }
            finally
            {
                Monitor.Enter(tx);
            }
            return enlistment2;
        }

        internal override Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment2;
            Monitor.Exit(tx);
            try
            {
                Enlistment enlistment = new Enlistment(resourceManagerIdentifier, tx, enlistmentNotification, enlistmentNotification, atomicTransaction);
                EnlistmentState._EnlistmentStatePromoted.EnterState(enlistment.InternalEnlistment);
                enlistment.InternalEnlistment.PromotedEnlistment = tx.PromotedTransaction.EnlistDurable(resourceManagerIdentifier, (DurableInternalEnlistment) enlistment.InternalEnlistment, true, enlistmentOptions);
                enlistment2 = enlistment;
            }
            finally
            {
                Monitor.Enter(tx);
            }
            return enlistment2;
        }

        internal override bool EnlistPromotableSinglePhase(InternalTransaction tx, IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Transaction atomicTransaction)
        {
            return false;
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment2;
            Monitor.Exit(tx);
            try
            {
                Enlistment enlistment = new Enlistment(enlistmentNotification, tx, atomicTransaction);
                EnlistmentState._EnlistmentStatePromoted.EnterState(enlistment.InternalEnlistment);
                enlistment.InternalEnlistment.PromotedEnlistment = tx.PromotedTransaction.EnlistVolatile((ISinglePhaseNotificationInternal) enlistment.InternalEnlistment, enlistmentOptions);
                enlistment2 = enlistment;
            }
            finally
            {
                Monitor.Enter(tx);
            }
            return enlistment2;
        }

        internal override Enlistment EnlistVolatile(InternalTransaction tx, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            Enlistment enlistment2;
            Monitor.Exit(tx);
            try
            {
                Enlistment enlistment = new Enlistment(enlistmentNotification, tx, atomicTransaction);
                EnlistmentState._EnlistmentStatePromoted.EnterState(enlistment.InternalEnlistment);
                enlistment.InternalEnlistment.PromotedEnlistment = tx.PromotedTransaction.EnlistVolatile((ISinglePhaseNotificationInternal) enlistment.InternalEnlistment, enlistmentOptions);
                enlistment2 = enlistment;
            }
            finally
            {
                Monitor.Enter(tx);
            }
            return enlistment2;
        }

        internal override Guid get_Identifier(InternalTransaction tx)
        {
            return tx.PromotedTransaction.Identifier;
        }

        internal override TransactionStatus get_Status(InternalTransaction tx)
        {
            return TransactionStatus.Active;
        }

        internal override void GetObjectData(InternalTransaction tx, SerializationInfo serializationInfo, StreamingContext context)
        {
            ISerializable promotedTransaction = tx.PromotedTransaction;
            if (promotedTransaction == null)
            {
                throw new NotSupportedException();
            }
            serializationInfo.FullTypeName = tx.PromotedTransaction.GetType().FullName;
            promotedTransaction.GetObjectData(serializationInfo, context);
        }

        internal override void InDoubtFromDtc(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedIndoubt.EnterState(tx);
        }

        internal override void InDoubtFromEnlistment(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedIndoubt.EnterState(tx);
        }

        internal override void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
        }

        internal override void Promote(InternalTransaction tx)
        {
        }

        internal override void RestartCommitIfNeeded(InternalTransaction tx)
        {
            TransactionState._TransactionStatePromotedP0Wave.EnterState(tx);
        }

        internal override void Rollback(InternalTransaction tx, Exception e)
        {
            if (tx.innerException == null)
            {
                tx.innerException = e;
            }
            Monitor.Exit(tx);
            try
            {
                tx.PromotedTransaction.Rollback();
            }
            finally
            {
                Monitor.Enter(tx);
            }
        }

        internal override void Timeout(InternalTransaction tx)
        {
            try
            {
                if (tx.innerException == null)
                {
                    tx.innerException = new TimeoutException(System.Transactions.SR.GetString("TraceTransactionTimeout"));
                }
                tx.PromotedTransaction.Rollback();
                if (DiagnosticTrace.Warning)
                {
                    TransactionTimeoutTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), tx.TransactionTraceId);
                }
            }
            catch (TransactionException exception)
            {
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), exception);
                }
            }
        }
    }
}

