namespace System.Transactions.Oletx
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class RealOletxTransaction
    {
        internal OletxCommittableTransaction committableTransaction;
        private DateTime creationTime;
        private bool doomed;
        internal int enlistmentCount;
        internal Exception innerException;
        internal OletxTransaction internalClone;
        private System.Transactions.InternalTransaction internalTransaction;
        private IsolationLevel isolationLevel;
        private DateTime lastStateChangeTime;
        private OletxTransactionManager oletxTransactionManager;
        private OutcomeEnlistment outcomeEnlistment;
        internal ArrayList phase0EnlistVolatilementContainerList;
        internal OletxPhase1VolatileEnlistmentContainer phase1EnlistVolatilementContainer;
        private TransactionStatus status;
        private bool tooLateForEnlistments;
        private TransactionTraceIdentifier traceIdentifier = TransactionTraceIdentifier.Empty;
        private ITransactionShim transactionShim;
        private Guid txGuid;
        private int undecidedEnlistmentCount;
        private int undisposedOletxTransactionCount;

        internal RealOletxTransaction(OletxTransactionManager transactionManager, ITransactionShim transactionShim, OutcomeEnlistment outcomeEnlistment, Guid identifier, OletxTransactionIsolationLevel oletxIsoLevel, bool isRoot)
        {
            bool flag = false;
            try
            {
                this.oletxTransactionManager = transactionManager;
                this.transactionShim = transactionShim;
                this.outcomeEnlistment = outcomeEnlistment;
                this.txGuid = identifier;
                this.isolationLevel = OletxTransactionManager.ConvertIsolationLevelFromProxyValue(oletxIsoLevel);
                this.status = TransactionStatus.Active;
                this.undisposedOletxTransactionCount = 0;
                this.phase0EnlistVolatilementContainerList = null;
                this.phase1EnlistVolatilementContainer = null;
                this.tooLateForEnlistments = false;
                this.internalTransaction = null;
                this.creationTime = DateTime.UtcNow;
                this.lastStateChangeTime = this.creationTime;
                this.internalClone = new OletxTransaction(this);
                if (this.outcomeEnlistment != null)
                {
                    this.outcomeEnlistment.SetRealTransaction(this);
                }
                else
                {
                    this.status = TransactionStatus.InDoubt;
                }
                if (DiagnosticTrace.HaveListeners)
                {
                    DiagnosticTrace.TraceTransfer(this.txGuid);
                }
                flag = true;
            }
            finally
            {
                if (!flag && (this.outcomeEnlistment != null))
                {
                    this.outcomeEnlistment.UnregisterOutcomeCallback();
                    this.outcomeEnlistment = null;
                }
            }
        }

        internal OletxVolatileEnlistmentContainer AddDependentClone(bool delayCommit)
        {
            IVoterBallotShim voterBallotShim = null;
            IPhase0EnlistmentShim shim2 = null;
            bool flag2 = false;
            bool flag = false;
            OletxVolatileEnlistmentContainer container3 = null;
            OletxPhase0VolatileEnlistmentContainer container = null;
            OletxPhase1VolatileEnlistmentContainer target = null;
            bool flag3 = false;
            bool flag5 = false;
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                lock (this)
                {
                    if (delayCommit)
                    {
                        if (this.phase0EnlistVolatilementContainerList == null)
                        {
                            this.phase0EnlistVolatilementContainerList = new ArrayList(1);
                        }
                        if (this.phase0EnlistVolatilementContainerList.Count == 0)
                        {
                            container = new OletxPhase0VolatileEnlistmentContainer(this);
                            flag = true;
                        }
                        else
                        {
                            container = this.phase0EnlistVolatilementContainerList[this.phase0EnlistVolatilementContainerList.Count - 1] as OletxPhase0VolatileEnlistmentContainer;
                            if (container != null)
                            {
                                this.TakeContainerLock(container, ref flag5);
                            }
                            if (!container.NewEnlistmentsAllowed)
                            {
                                this.ReleaseContainerLock(container, ref flag5);
                                container = new OletxPhase0VolatileEnlistmentContainer(this);
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            zero = HandleTable.AllocHandle(container);
                        }
                    }
                    else if (this.phase1EnlistVolatilementContainer == null)
                    {
                        target = new OletxPhase1VolatileEnlistmentContainer(this);
                        flag2 = true;
                        target.voterHandle = HandleTable.AllocHandle(target);
                    }
                    else
                    {
                        flag2 = false;
                        target = this.phase1EnlistVolatilementContainer;
                    }
                    try
                    {
                        if (container != null)
                        {
                            this.TakeContainerLock(container, ref flag5);
                        }
                        if (flag)
                        {
                            this.transactionShim.Phase0Enlist(zero, out shim2);
                            container.Phase0EnlistmentShim = shim2;
                        }
                        if (flag2)
                        {
                            this.OletxTransactionManagerInstance.dtcTransactionManagerLock.AcquireReaderLock(-1);
                            try
                            {
                                this.transactionShim.CreateVoter(target.voterHandle, out voterBallotShim);
                                flag3 = true;
                            }
                            finally
                            {
                                this.OletxTransactionManagerInstance.dtcTransactionManagerLock.ReleaseReaderLock();
                            }
                            target.VoterBallotShim = voterBallotShim;
                        }
                        if (delayCommit)
                        {
                            if (flag)
                            {
                                this.phase0EnlistVolatilementContainerList.Add(container);
                            }
                            container.AddDependentClone();
                            return container;
                        }
                        if (flag2)
                        {
                            this.phase1EnlistVolatilementContainer = target;
                        }
                        target.AddDependentClone();
                        return target;
                    }
                    catch (COMException exception)
                    {
                        OletxTransactionManager.ProxyException(exception);
                        throw;
                    }
                    return container3;
                }
            }
            finally
            {
                if (container != null)
                {
                    this.ReleaseContainerLock(container, ref flag5);
                }
                if ((zero != IntPtr.Zero) && (container.Phase0EnlistmentShim == null))
                {
                    HandleTable.FreeHandle(zero);
                }
                if ((!flag3 && (target != null)) && ((target.voterHandle != IntPtr.Zero) && flag2))
                {
                    HandleTable.FreeHandle(target.voterHandle);
                }
            }
            return container3;
        }

        internal void Commit()
        {
            try
            {
                this.transactionShim.Commit();
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_ABORTED == exception.ErrorCode) || (System.Transactions.Oletx.NativeMethods.XACT_E_INDOUBT == exception.ErrorCode))
                {
                    Interlocked.CompareExchange<Exception>(ref this.innerException, exception, null);
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
                else
                {
                    if (System.Transactions.Oletx.NativeMethods.XACT_E_ALREADYINPROGRESS == exception.ErrorCode)
                    {
                        throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionAlreadyOver"), exception);
                    }
                    OletxTransactionManager.ProxyException(exception);
                    throw;
                }
            }
        }

        internal IPromotedEnlistment CommonEnlistVolatile(IEnlistmentNotificationInternal enlistmentNotification, EnlistmentOptions enlistmentOptions, OletxTransaction oletxTransaction)
        {
            OletxVolatileEnlistment enlistment = null;
            bool flag2 = false;
            bool flag = false;
            OletxPhase0VolatileEnlistmentContainer target = null;
            OletxPhase1VolatileEnlistmentContainer container = null;
            IntPtr zero = IntPtr.Zero;
            IVoterBallotShim voterBallotShim = null;
            IPhase0EnlistmentShim shim = null;
            bool flag3 = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                lock (this)
                {
                    enlistment = new OletxVolatileEnlistment(enlistmentNotification, enlistmentOptions, oletxTransaction);
                    if ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)
                    {
                        if (this.phase0EnlistVolatilementContainerList == null)
                        {
                            this.phase0EnlistVolatilementContainerList = new ArrayList(1);
                        }
                        if (this.phase0EnlistVolatilementContainerList.Count == 0)
                        {
                            target = new OletxPhase0VolatileEnlistmentContainer(this);
                            flag = true;
                        }
                        else
                        {
                            target = this.phase0EnlistVolatilementContainerList[this.phase0EnlistVolatilementContainerList.Count - 1] as OletxPhase0VolatileEnlistmentContainer;
                            if (!target.NewEnlistmentsAllowed)
                            {
                                target = new OletxPhase0VolatileEnlistmentContainer(this);
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            zero = HandleTable.AllocHandle(target);
                        }
                    }
                    else if (this.phase1EnlistVolatilementContainer == null)
                    {
                        flag2 = true;
                        container = new OletxPhase1VolatileEnlistmentContainer(this) {
                            voterHandle = HandleTable.AllocHandle(container)
                        };
                    }
                    else
                    {
                        flag2 = false;
                        container = this.phase1EnlistVolatilementContainer;
                    }
                    try
                    {
                        if (flag)
                        {
                            lock (target)
                            {
                                this.transactionShim.Phase0Enlist(zero, out shim);
                                target.Phase0EnlistmentShim = shim;
                            }
                        }
                        if (flag2)
                        {
                            this.transactionShim.CreateVoter(container.voterHandle, out voterBallotShim);
                            flag3 = true;
                            container.VoterBallotShim = voterBallotShim;
                        }
                        if ((enlistmentOptions & EnlistmentOptions.EnlistDuringPrepareRequired) != EnlistmentOptions.None)
                        {
                            target.AddEnlistment(enlistment);
                            if (flag)
                            {
                                this.phase0EnlistVolatilementContainerList.Add(target);
                            }
                            return enlistment;
                        }
                        container.AddEnlistment(enlistment);
                        if (flag2)
                        {
                            this.phase1EnlistVolatilementContainer = container;
                        }
                        return enlistment;
                    }
                    catch (COMException exception)
                    {
                        OletxTransactionManager.ProxyException(exception);
                        throw;
                    }
                    return enlistment;
                }
            }
            finally
            {
                if ((zero != IntPtr.Zero) && (target.Phase0EnlistmentShim == null))
                {
                    HandleTable.FreeHandle(zero);
                }
                if ((!flag3 && (container != null)) && ((container.voterHandle != IntPtr.Zero) && flag2))
                {
                    HandleTable.FreeHandle(container.voterHandle);
                }
            }
            return enlistment;
        }

        internal void DecrementUndecidedEnlistments()
        {
            Interlocked.Decrement(ref this.undecidedEnlistmentCount);
        }

        internal IPromotedEnlistment EnlistVolatile(IEnlistmentNotificationInternal enlistmentNotification, EnlistmentOptions enlistmentOptions, OletxTransaction oletxTransaction)
        {
            return this.CommonEnlistVolatile(enlistmentNotification, enlistmentOptions, oletxTransaction);
        }

        internal IPromotedEnlistment EnlistVolatile(ISinglePhaseNotificationInternal enlistmentNotification, EnlistmentOptions enlistmentOptions, OletxTransaction oletxTransaction)
        {
            return this.CommonEnlistVolatile(enlistmentNotification, enlistmentOptions, oletxTransaction);
        }

        internal void FireOutcome(TransactionStatus statusArg)
        {
            lock (this)
            {
                if (statusArg == TransactionStatus.Committed)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        TransactionCommittedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), this.TransactionTraceId);
                    }
                    this.status = TransactionStatus.Committed;
                }
                else if (statusArg == TransactionStatus.Aborted)
                {
                    if (DiagnosticTrace.Warning)
                    {
                        TransactionAbortedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), this.TransactionTraceId);
                    }
                    this.status = TransactionStatus.Aborted;
                }
                else
                {
                    if (DiagnosticTrace.Warning)
                    {
                        TransactionInDoubtTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), this.TransactionTraceId);
                    }
                    this.status = TransactionStatus.InDoubt;
                }
            }
            if (this.InternalTransaction != null)
            {
                System.Transactions.InternalTransaction.DistributedTransactionOutcome(this.InternalTransaction, this.status);
            }
        }

        internal void IncrementUndecidedEnlistments()
        {
            Interlocked.Increment(ref this.undecidedEnlistmentCount);
        }

        internal void OletxTransactionCreated()
        {
            Interlocked.Increment(ref this.undisposedOletxTransactionCount);
        }

        internal void OletxTransactionDisposed()
        {
            Interlocked.Decrement(ref this.undisposedOletxTransactionCount);
        }

        private void ReleaseContainerLock(OletxPhase0VolatileEnlistmentContainer localPhase0VolatileContainer, ref bool phase0ContainerLockAcquired)
        {
            if (phase0ContainerLockAcquired)
            {
                Monitor.Exit(localPhase0VolatileContainer);
                phase0ContainerLockAcquired = false;
            }
        }

        internal void Rollback()
        {
            lock (this)
            {
                if ((TransactionStatus.Aborted != this.status) && (this.status != TransactionStatus.Active))
                {
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionAlreadyOver"), null);
                }
                if (TransactionStatus.Aborted == this.status)
                {
                    return;
                }
                if (0 < this.undecidedEnlistmentCount)
                {
                    this.doomed = true;
                }
                else if (this.tooLateForEnlistments)
                {
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionAlreadyOver"), null);
                }
                if (this.phase0EnlistVolatilementContainerList != null)
                {
                    foreach (OletxPhase0VolatileEnlistmentContainer container in this.phase0EnlistVolatilementContainerList)
                    {
                        container.RollbackFromTransaction();
                    }
                }
                if (this.phase1EnlistVolatilementContainer != null)
                {
                    this.phase1EnlistVolatilementContainer.RollbackFromTransaction();
                }
            }
            try
            {
                this.transactionShim.Abort();
            }
            catch (COMException exception)
            {
                if (System.Transactions.Oletx.NativeMethods.XACT_E_ALREADYINPROGRESS == exception.ErrorCode)
                {
                    if (!this.doomed)
                    {
                        throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionAlreadyOver"), exception);
                    }
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
                else
                {
                    OletxTransactionManager.ProxyException(exception);
                    throw;
                }
            }
        }

        private void TakeContainerLock(OletxPhase0VolatileEnlistmentContainer localPhase0VolatileContainer, ref bool phase0ContainerLockAcquired)
        {
            if (!phase0ContainerLockAcquired)
            {
                Monitor.Enter(localPhase0VolatileContainer);
                phase0ContainerLockAcquired = true;
            }
        }

        internal void TMDown()
        {
            lock (this)
            {
                if (this.phase0EnlistVolatilementContainerList != null)
                {
                    foreach (OletxPhase0VolatileEnlistmentContainer container in this.phase0EnlistVolatilementContainerList)
                    {
                        container.TMDown();
                    }
                }
            }
            this.outcomeEnlistment.TMDown();
        }

        internal bool Doomed
        {
            get
            {
                return this.doomed;
            }
        }

        internal Guid Identifier
        {
            get
            {
                if (this.txGuid.Equals(Guid.Empty))
                {
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("CannotGetTransactionIdentifier"), null);
                }
                return this.txGuid;
            }
        }

        internal System.Transactions.InternalTransaction InternalTransaction
        {
            get
            {
                return this.internalTransaction;
            }
            set
            {
                this.internalTransaction = value;
            }
        }

        internal OletxTransactionManager OletxTransactionManagerInstance
        {
            get
            {
                return this.oletxTransactionManager;
            }
        }

        internal TransactionStatus Status
        {
            get
            {
                return this.status;
            }
        }

        internal bool TooLateForEnlistments
        {
            get
            {
                return this.tooLateForEnlistments;
            }
            set
            {
                this.tooLateForEnlistments = value;
            }
        }

        internal IsolationLevel TransactionIsolationLevel
        {
            get
            {
                return this.isolationLevel;
            }
        }

        internal ITransactionShim TransactionShim
        {
            get
            {
                ITransactionShim transactionShim = this.transactionShim;
                if (transactionShim == null)
                {
                    throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                return transactionShim;
            }
        }

        internal TransactionTraceIdentifier TransactionTraceId
        {
            get
            {
                if (TransactionTraceIdentifier.Empty == this.traceIdentifier)
                {
                    lock (this)
                    {
                        if ((TransactionTraceIdentifier.Empty == this.traceIdentifier) && (Guid.Empty != this.txGuid))
                        {
                            TransactionTraceIdentifier identifier = new TransactionTraceIdentifier(this.txGuid.ToString(), 0);
                            Thread.MemoryBarrier();
                            this.traceIdentifier = identifier;
                        }
                    }
                }
                return this.traceIdentifier;
            }
        }

        internal Guid TxGuid
        {
            get
            {
                return this.txGuid;
            }
        }

        internal int UndecidedEnlistments
        {
            get
            {
                return this.undecidedEnlistmentCount;
            }
        }
    }
}

