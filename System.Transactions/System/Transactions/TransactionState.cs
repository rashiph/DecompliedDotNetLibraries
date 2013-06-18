namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Transactions.Diagnostics;

    internal abstract class TransactionState
    {
        private static TransactionStateAborted _transactionStateAborted;
        private static TransactionStateActive _transactionStateActive;
        private static TransactionStateCommitted _transactionStateCommitted;
        private static TransactionStateDelegated _transactionStateDelegated;
        private static TransactionStateDelegatedAborting _transactionStateDelegatedAborting;
        private static TransactionStateDelegatedCommitting _transactionStateDelegatedCommitting;
        private static TransactionStateDelegatedP0Wave _transactionStateDelegatedP0Wave;
        private static TransactionStateDelegatedSubordinate _transactionStateDelegatedSubordinate;
        private static TransactionStateInDoubt _transactionStateInDoubt;
        private static TransactionStateNonCommittablePromoted _transactionStateNonCommittablePromoted;
        private static TransactionStatePhase0 _transactionStatePhase0;
        private static TransactionStatePromoted _transactionStatePromoted;
        private static TransactionStatePromotedAborted _transactionStatePromotedAborted;
        private static TransactionStatePromotedCommitted _transactionStatePromotedCommitted;
        private static TransactionStatePromotedCommitting _transactionStatePromotedCommitting;
        private static TransactionStatePromotedIndoubt _transactionStatePromotedIndoubt;
        private static TransactionStatePromotedP0Aborting _transactionStatePromotedP0Aborting;
        private static TransactionStatePromotedP0Wave _transactionStatePromotedP0Wave;
        private static TransactionStatePromotedP1Aborting _transactionStatePromotedP1Aborting;
        private static TransactionStatePromotedPhase0 _transactionStatePromotedPhase0;
        private static TransactionStatePromotedPhase1 _transactionStatePromotedPhase1;
        private static TransactionStatePSPEOperation _transactionStatePSPEOperation;
        private static TransactionStateSPC _transactionStateSPC;
        private static TransactionStateSubordinateActive _transactionStateSubordinateActive;
        private static TransactionStateVolatilePhase1 _transactionStateVolatilePhase1;
        private static TransactionStateVolatileSPC _transactionStateVolatileSPC;
        private static object classSyncObject;

        protected TransactionState()
        {
        }

        internal virtual void AddOutcomeRegistrant(InternalTransaction tx, TransactionCompletedEventHandler transactionCompletedDelegate)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        protected void AddVolatileEnlistment(ref VolatileEnlistmentSet enlistments, Enlistment enlistment)
        {
            if (enlistments.volatileEnlistmentCount == enlistments.volatileEnlistmentSize)
            {
                InternalEnlistment[] destinationArray = new InternalEnlistment[enlistments.volatileEnlistmentSize + 8];
                if (enlistments.volatileEnlistmentSize > 0)
                {
                    Array.Copy(enlistments.volatileEnlistments, destinationArray, enlistments.volatileEnlistmentSize);
                }
                enlistments.volatileEnlistmentSize += 8;
                enlistments.volatileEnlistments = destinationArray;
            }
            enlistments.volatileEnlistments[enlistments.volatileEnlistmentCount] = enlistment.InternalEnlistment;
            enlistments.volatileEnlistmentCount++;
            VolatileEnlistmentState._VolatileEnlistmentActive.EnterState(enlistments.volatileEnlistments[enlistments.volatileEnlistmentCount - 1]);
        }

        internal virtual void BeginCommit(InternalTransaction tx, bool asyncCommit, AsyncCallback asyncCallback, object asyncState)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void ChangeStateAbortedDuringPromotion(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void ChangeStatePromotedAborted(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void ChangeStatePromotedCommitted(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void ChangeStatePromotedPhase0(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void ChangeStatePromotedPhase1(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void ChangeStateTransactionAborted(InternalTransaction tx, Exception e)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void ChangeStateTransactionCommitted(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void CheckForFinishedTransaction(InternalTransaction tx)
        {
        }

        internal void CommonEnterState(InternalTransaction tx)
        {
            tx.State = this;
        }

        internal virtual void CompleteAbortingClone(InternalTransaction tx)
        {
        }

        internal virtual void CompleteBlockingClone(InternalTransaction tx)
        {
        }

        internal virtual bool ContinuePhase0Prepares()
        {
            return false;
        }

        internal virtual bool ContinuePhase1Prepares()
        {
            return false;
        }

        internal virtual void CreateAbortingClone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void CreateBlockingClone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void DisposeRoot(InternalTransaction tx)
        {
        }

        internal virtual void EndCommit(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual Enlistment EnlistDurable(InternalTransaction tx, Guid resourceManagerIdentifier, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual bool EnlistPromotableSinglePhase(InternalTransaction tx, IPromotableSinglePhaseNotification promotableSinglePhaseNotification, Transaction atomicTransaction)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual Enlistment EnlistVolatile(InternalTransaction tx, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual Enlistment EnlistVolatile(InternalTransaction tx, ISinglePhaseNotification enlistmentNotification, EnlistmentOptions enlistmentOptions, Transaction atomicTransaction)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal abstract void EnterState(InternalTransaction tx);
        internal virtual Guid get_Identifier(InternalTransaction tx)
        {
            return Guid.Empty;
        }

        internal abstract TransactionStatus get_Status(InternalTransaction tx);
        internal virtual void GetObjectData(InternalTransaction tx, SerializationInfo serializationInfo, StreamingContext context)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void InDoubtFromDtc(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void InDoubtFromEnlistment(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual bool IsCompleted(InternalTransaction tx)
        {
            tx.needPulse = true;
            return false;
        }

        internal virtual void Phase0VolatilePrepareDone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void Phase1VolatilePrepareDone(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void Promote(InternalTransaction tx)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void RestartCommitIfNeeded(InternalTransaction tx)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "");
            }
            throw new InvalidOperationException();
        }

        internal virtual void Rollback(InternalTransaction tx, Exception e)
        {
            throw TransactionException.CreateTransactionStateException(System.Transactions.SR.GetString("TraceSourceLtm"), tx.innerException);
        }

        internal virtual void Timeout(InternalTransaction tx)
        {
        }

        protected static TransactionStateAborted _TransactionStateAborted
        {
            get
            {
                if (_transactionStateAborted == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateAborted == null)
                        {
                            TransactionStateAborted aborted = new TransactionStateAborted();
                            Thread.MemoryBarrier();
                            _transactionStateAborted = aborted;
                        }
                    }
                }
                return _transactionStateAborted;
            }
        }

        internal static TransactionStateActive _TransactionStateActive
        {
            get
            {
                if (_transactionStateActive == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateActive == null)
                        {
                            TransactionStateActive active = new TransactionStateActive();
                            Thread.MemoryBarrier();
                            _transactionStateActive = active;
                        }
                    }
                }
                return _transactionStateActive;
            }
        }

        protected static TransactionStateCommitted _TransactionStateCommitted
        {
            get
            {
                if (_transactionStateCommitted == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateCommitted == null)
                        {
                            TransactionStateCommitted committed = new TransactionStateCommitted();
                            Thread.MemoryBarrier();
                            _transactionStateCommitted = committed;
                        }
                    }
                }
                return _transactionStateCommitted;
            }
        }

        protected static TransactionStateDelegated _TransactionStateDelegated
        {
            get
            {
                if (_transactionStateDelegated == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateDelegated == null)
                        {
                            TransactionStateDelegated delegated = new TransactionStateDelegated();
                            Thread.MemoryBarrier();
                            _transactionStateDelegated = delegated;
                        }
                    }
                }
                return _transactionStateDelegated;
            }
        }

        protected static TransactionStateDelegatedAborting _TransactionStateDelegatedAborting
        {
            get
            {
                if (_transactionStateDelegatedAborting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateDelegatedAborting == null)
                        {
                            TransactionStateDelegatedAborting aborting = new TransactionStateDelegatedAborting();
                            Thread.MemoryBarrier();
                            _transactionStateDelegatedAborting = aborting;
                        }
                    }
                }
                return _transactionStateDelegatedAborting;
            }
        }

        protected static TransactionStateDelegatedCommitting _TransactionStateDelegatedCommitting
        {
            get
            {
                if (_transactionStateDelegatedCommitting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateDelegatedCommitting == null)
                        {
                            TransactionStateDelegatedCommitting committing = new TransactionStateDelegatedCommitting();
                            Thread.MemoryBarrier();
                            _transactionStateDelegatedCommitting = committing;
                        }
                    }
                }
                return _transactionStateDelegatedCommitting;
            }
        }

        protected static TransactionStateDelegatedP0Wave _TransactionStateDelegatedP0Wave
        {
            get
            {
                if (_transactionStateDelegatedP0Wave == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateDelegatedP0Wave == null)
                        {
                            TransactionStateDelegatedP0Wave wave = new TransactionStateDelegatedP0Wave();
                            Thread.MemoryBarrier();
                            _transactionStateDelegatedP0Wave = wave;
                        }
                    }
                }
                return _transactionStateDelegatedP0Wave;
            }
        }

        internal static TransactionStateDelegatedSubordinate _TransactionStateDelegatedSubordinate
        {
            get
            {
                if (_transactionStateDelegatedSubordinate == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateDelegatedSubordinate == null)
                        {
                            TransactionStateDelegatedSubordinate subordinate = new TransactionStateDelegatedSubordinate();
                            Thread.MemoryBarrier();
                            _transactionStateDelegatedSubordinate = subordinate;
                        }
                    }
                }
                return _transactionStateDelegatedSubordinate;
            }
        }

        protected static TransactionStateInDoubt _TransactionStateInDoubt
        {
            get
            {
                if (_transactionStateInDoubt == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateInDoubt == null)
                        {
                            TransactionStateInDoubt doubt = new TransactionStateInDoubt();
                            Thread.MemoryBarrier();
                            _transactionStateInDoubt = doubt;
                        }
                    }
                }
                return _transactionStateInDoubt;
            }
        }

        internal static TransactionStateNonCommittablePromoted _TransactionStateNonCommittablePromoted
        {
            get
            {
                if (_transactionStateNonCommittablePromoted == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateNonCommittablePromoted == null)
                        {
                            TransactionStateNonCommittablePromoted promoted = new TransactionStateNonCommittablePromoted();
                            Thread.MemoryBarrier();
                            _transactionStateNonCommittablePromoted = promoted;
                        }
                    }
                }
                return _transactionStateNonCommittablePromoted;
            }
        }

        protected static TransactionStatePhase0 _TransactionStatePhase0
        {
            get
            {
                if (_transactionStatePhase0 == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePhase0 == null)
                        {
                            TransactionStatePhase0 phase = new TransactionStatePhase0();
                            Thread.MemoryBarrier();
                            _transactionStatePhase0 = phase;
                        }
                    }
                }
                return _transactionStatePhase0;
            }
        }

        internal static TransactionStatePromoted _TransactionStatePromoted
        {
            get
            {
                if (_transactionStatePromoted == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromoted == null)
                        {
                            TransactionStatePromoted promoted = new TransactionStatePromoted();
                            Thread.MemoryBarrier();
                            _transactionStatePromoted = promoted;
                        }
                    }
                }
                return _transactionStatePromoted;
            }
        }

        protected static TransactionStatePromotedAborted _TransactionStatePromotedAborted
        {
            get
            {
                if (_transactionStatePromotedAborted == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedAborted == null)
                        {
                            TransactionStatePromotedAborted aborted = new TransactionStatePromotedAborted();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedAborted = aborted;
                        }
                    }
                }
                return _transactionStatePromotedAborted;
            }
        }

        protected static TransactionStatePromotedCommitted _TransactionStatePromotedCommitted
        {
            get
            {
                if (_transactionStatePromotedCommitted == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedCommitted == null)
                        {
                            TransactionStatePromotedCommitted committed = new TransactionStatePromotedCommitted();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedCommitted = committed;
                        }
                    }
                }
                return _transactionStatePromotedCommitted;
            }
        }

        protected static TransactionStatePromotedCommitting _TransactionStatePromotedCommitting
        {
            get
            {
                if (_transactionStatePromotedCommitting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedCommitting == null)
                        {
                            TransactionStatePromotedCommitting committing = new TransactionStatePromotedCommitting();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedCommitting = committing;
                        }
                    }
                }
                return _transactionStatePromotedCommitting;
            }
        }

        protected static TransactionStatePromotedIndoubt _TransactionStatePromotedIndoubt
        {
            get
            {
                if (_transactionStatePromotedIndoubt == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedIndoubt == null)
                        {
                            TransactionStatePromotedIndoubt indoubt = new TransactionStatePromotedIndoubt();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedIndoubt = indoubt;
                        }
                    }
                }
                return _transactionStatePromotedIndoubt;
            }
        }

        protected static TransactionStatePromotedP0Aborting _TransactionStatePromotedP0Aborting
        {
            get
            {
                if (_transactionStatePromotedP0Aborting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedP0Aborting == null)
                        {
                            TransactionStatePromotedP0Aborting aborting = new TransactionStatePromotedP0Aborting();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedP0Aborting = aborting;
                        }
                    }
                }
                return _transactionStatePromotedP0Aborting;
            }
        }

        protected static TransactionStatePromotedP0Wave _TransactionStatePromotedP0Wave
        {
            get
            {
                if (_transactionStatePromotedP0Wave == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedP0Wave == null)
                        {
                            TransactionStatePromotedP0Wave wave = new TransactionStatePromotedP0Wave();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedP0Wave = wave;
                        }
                    }
                }
                return _transactionStatePromotedP0Wave;
            }
        }

        protected static TransactionStatePromotedP1Aborting _TransactionStatePromotedP1Aborting
        {
            get
            {
                if (_transactionStatePromotedP1Aborting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedP1Aborting == null)
                        {
                            TransactionStatePromotedP1Aborting aborting = new TransactionStatePromotedP1Aborting();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedP1Aborting = aborting;
                        }
                    }
                }
                return _transactionStatePromotedP1Aborting;
            }
        }

        protected static TransactionStatePromotedPhase0 _TransactionStatePromotedPhase0
        {
            get
            {
                if (_transactionStatePromotedPhase0 == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedPhase0 == null)
                        {
                            TransactionStatePromotedPhase0 phase = new TransactionStatePromotedPhase0();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedPhase0 = phase;
                        }
                    }
                }
                return _transactionStatePromotedPhase0;
            }
        }

        protected static TransactionStatePromotedPhase1 _TransactionStatePromotedPhase1
        {
            get
            {
                if (_transactionStatePromotedPhase1 == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePromotedPhase1 == null)
                        {
                            TransactionStatePromotedPhase1 phase = new TransactionStatePromotedPhase1();
                            Thread.MemoryBarrier();
                            _transactionStatePromotedPhase1 = phase;
                        }
                    }
                }
                return _transactionStatePromotedPhase1;
            }
        }

        internal static TransactionStatePSPEOperation _TransactionStatePSPEOperation
        {
            get
            {
                if (_transactionStatePSPEOperation == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStatePSPEOperation == null)
                        {
                            TransactionStatePSPEOperation operation = new TransactionStatePSPEOperation();
                            Thread.MemoryBarrier();
                            _transactionStatePSPEOperation = operation;
                        }
                    }
                }
                return _transactionStatePSPEOperation;
            }
        }

        protected static TransactionStateSPC _TransactionStateSPC
        {
            get
            {
                if (_transactionStateSPC == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateSPC == null)
                        {
                            TransactionStateSPC espc = new TransactionStateSPC();
                            Thread.MemoryBarrier();
                            _transactionStateSPC = espc;
                        }
                    }
                }
                return _transactionStateSPC;
            }
        }

        internal static TransactionStateSubordinateActive _TransactionStateSubordinateActive
        {
            get
            {
                if (_transactionStateSubordinateActive == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateSubordinateActive == null)
                        {
                            TransactionStateSubordinateActive active = new TransactionStateSubordinateActive();
                            Thread.MemoryBarrier();
                            _transactionStateSubordinateActive = active;
                        }
                    }
                }
                return _transactionStateSubordinateActive;
            }
        }

        protected static TransactionStateVolatilePhase1 _TransactionStateVolatilePhase1
        {
            get
            {
                if (_transactionStateVolatilePhase1 == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateVolatilePhase1 == null)
                        {
                            TransactionStateVolatilePhase1 phase = new TransactionStateVolatilePhase1();
                            Thread.MemoryBarrier();
                            _transactionStateVolatilePhase1 = phase;
                        }
                    }
                }
                return _transactionStateVolatilePhase1;
            }
        }

        protected static TransactionStateVolatileSPC _TransactionStateVolatileSPC
        {
            get
            {
                if (_transactionStateVolatileSPC == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_transactionStateVolatileSPC == null)
                        {
                            TransactionStateVolatileSPC espc = new TransactionStateVolatileSPC();
                            Thread.MemoryBarrier();
                            _transactionStateVolatileSPC = espc;
                        }
                    }
                }
                return _transactionStateVolatileSPC;
            }
        }

        internal static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }
    }
}

