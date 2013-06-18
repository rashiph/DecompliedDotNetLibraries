namespace System.Transactions
{
    using System;
    using System.EnterpriseServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions.Diagnostics;
    using System.Transactions.Oletx;

    [Serializable]
    public class Transaction : IDisposable, ISerializable
    {
        private static EnterpriseServicesState _enterpriseServicesOk = EnterpriseServicesState.Unknown;
        internal int cloneId;
        internal bool complete;
        internal int disposed;
        internal const int disposedTrueValue = 1;
        private static Guid IID_IObjContext = new Guid("000001c6-0000-0000-C000-000000000046");
        internal InternalTransaction internalTransaction;
        internal System.Transactions.IsolationLevel isoLevel;
        internal TransactionTraceIdentifier traceIdentifier;

        public event TransactionCompletedEventHandler TransactionCompleted
        {
            add
            {
                if (this.Disposed)
                {
                    throw new ObjectDisposedException("Transaction");
                }
                lock (this.internalTransaction)
                {
                    this.internalTransaction.State.AddOutcomeRegistrant(this.internalTransaction, value);
                }
            }
            remove
            {
                lock (this.internalTransaction)
                {
                    this.internalTransaction.transactionCompletedDelegate = (TransactionCompletedEventHandler) Delegate.Remove(this.internalTransaction.transactionCompletedDelegate, value);
                }
            }
        }

        private Transaction()
        {
        }

        internal Transaction(OletxTransaction oleTransaction)
        {
            this.isoLevel = oleTransaction.IsolationLevel;
            this.internalTransaction = new InternalTransaction(this, oleTransaction);
            this.cloneId = Interlocked.Increment(ref this.internalTransaction.cloneCount);
        }

        internal Transaction(System.Transactions.IsolationLevel isoLevel, InternalTransaction internalTransaction)
        {
            TransactionManager.ValidateIsolationLevel(isoLevel);
            this.isoLevel = isoLevel;
            if (System.Transactions.IsolationLevel.Unspecified == this.isoLevel)
            {
                this.isoLevel = TransactionManager.DefaultIsolationLevel;
            }
            if (internalTransaction != null)
            {
                this.internalTransaction = internalTransaction;
                this.cloneId = Interlocked.Increment(ref this.internalTransaction.cloneCount);
            }
        }

        internal Transaction(System.Transactions.IsolationLevel isoLevel, ISimpleTransactionSuperior superior)
        {
            TransactionManager.ValidateIsolationLevel(isoLevel);
            if (superior == null)
            {
                throw new ArgumentNullException("superior");
            }
            this.isoLevel = isoLevel;
            if (System.Transactions.IsolationLevel.Unspecified == this.isoLevel)
            {
                this.isoLevel = TransactionManager.DefaultIsolationLevel;
            }
            this.internalTransaction = new InternalTransaction(this, superior);
            this.cloneId = 1;
        }

        public Transaction Clone()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.Clone");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            Transaction transaction = this.InternalClone();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.Clone");
            }
            return transaction;
        }

        public DependentTransaction DependentClone(DependentCloneOption cloneOption)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.DependentClone");
            }
            if ((cloneOption != DependentCloneOption.BlockCommitUntilComplete) && (cloneOption != DependentCloneOption.RollbackIfNotComplete))
            {
                throw new ArgumentOutOfRangeException("cloneOption");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            DependentTransaction transaction = new DependentTransaction(this.isoLevel, this.internalTransaction, cloneOption == DependentCloneOption.BlockCommitUntilComplete);
            if (DiagnosticTrace.Information)
            {
                DependentCloneCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), transaction.TransactionTraceId, cloneOption);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.DependentClone");
            }
            return transaction;
        }

        public void Dispose()
        {
            this.InternalDispose();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public Enlistment EnlistDurable(Guid resourceManagerIdentifier, IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistDurable( IEnlistmentNotification )");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (resourceManagerIdentifier == Guid.Empty)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("BadResourceManagerId"), "resourceManagerIdentifier");
            }
            if (enlistmentNotification == null)
            {
                throw new ArgumentNullException("enlistmentNotification");
            }
            if ((enlistmentOptions != EnlistmentOptions.None) && (enlistmentOptions != EnlistmentOptions.EnlistDuringPrepareRequired))
            {
                throw new ArgumentOutOfRangeException("enlistmentOptions");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            lock (this.internalTransaction)
            {
                Enlistment enlistment2 = this.internalTransaction.State.EnlistDurable(this.internalTransaction, resourceManagerIdentifier, enlistmentNotification, enlistmentOptions, this);
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistDurable( IEnlistmentNotification )");
                }
                return enlistment2;
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public Enlistment EnlistDurable(Guid resourceManagerIdentifier, ISinglePhaseNotification singlePhaseNotification, EnlistmentOptions enlistmentOptions)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistDurable( ISinglePhaseNotification )");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (resourceManagerIdentifier == Guid.Empty)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("BadResourceManagerId"), "resourceManagerIdentifier");
            }
            if (singlePhaseNotification == null)
            {
                throw new ArgumentNullException("singlePhaseNotification");
            }
            if ((enlistmentOptions != EnlistmentOptions.None) && (enlistmentOptions != EnlistmentOptions.EnlistDuringPrepareRequired))
            {
                throw new ArgumentOutOfRangeException("enlistmentOptions");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            lock (this.internalTransaction)
            {
                Enlistment enlistment2 = this.internalTransaction.State.EnlistDurable(this.internalTransaction, resourceManagerIdentifier, singlePhaseNotification, enlistmentOptions, this);
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistDurable( ISinglePhaseNotification )");
                }
                return enlistment2;
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public bool EnlistPromotableSinglePhase(IPromotableSinglePhaseNotification promotableSinglePhaseNotification)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistPromotableSinglePhase");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (promotableSinglePhaseNotification == null)
            {
                throw new ArgumentNullException("promotableSinglePhaseNotification");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            bool flag = false;
            lock (this.internalTransaction)
            {
                flag = this.internalTransaction.State.EnlistPromotableSinglePhase(this.internalTransaction, promotableSinglePhaseNotification, this);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistPromotableSinglePhase");
            }
            return flag;
        }

        public Enlistment EnlistVolatile(IEnlistmentNotification enlistmentNotification, EnlistmentOptions enlistmentOptions)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistVolatile( IEnlistmentNotification )");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (enlistmentNotification == null)
            {
                throw new ArgumentNullException("enlistmentNotification");
            }
            if ((enlistmentOptions != EnlistmentOptions.None) && (enlistmentOptions != EnlistmentOptions.EnlistDuringPrepareRequired))
            {
                throw new ArgumentOutOfRangeException("enlistmentOptions");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            lock (this.internalTransaction)
            {
                Enlistment enlistment2 = this.internalTransaction.State.EnlistVolatile(this.internalTransaction, enlistmentNotification, enlistmentOptions, this);
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistVolatile( IEnlistmentNotification )");
                }
                return enlistment2;
            }
        }

        public Enlistment EnlistVolatile(ISinglePhaseNotification singlePhaseNotification, EnlistmentOptions enlistmentOptions)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistVolatile( ISinglePhaseNotification )");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (singlePhaseNotification == null)
            {
                throw new ArgumentNullException("singlePhaseNotification");
            }
            if ((enlistmentOptions != EnlistmentOptions.None) && (enlistmentOptions != EnlistmentOptions.EnlistDuringPrepareRequired))
            {
                throw new ArgumentOutOfRangeException("enlistmentOptions");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            lock (this.internalTransaction)
            {
                Enlistment enlistment2 = this.internalTransaction.State.EnlistVolatile(this.internalTransaction, singlePhaseNotification, enlistmentOptions, this);
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.EnlistVolatile( ISinglePhaseNotification )");
                }
                return enlistment2;
            }
        }

        public override bool Equals(object obj)
        {
            Transaction transaction = obj as Transaction;
            if (null == transaction)
            {
                return false;
            }
            return (this.internalTransaction.TransactionHash == transaction.internalTransaction.TransactionHash);
        }

        internal static Transaction FastGetTransaction(TransactionScope currentScope, ContextData contextData, out Transaction contextTransaction)
        {
            Transaction transaction = null;
            contextTransaction = null;
            contextTransaction = contextData.CurrentTransaction;
            switch (InteropMode(currentScope))
            {
                case EnterpriseServicesInteropOption.None:
                    transaction = contextTransaction;
                    if ((transaction == null) && (currentScope == null))
                    {
                        if (!TransactionManager.currentDelegateSet)
                        {
                            return GetContextTransaction(contextData);
                        }
                        transaction = TransactionManager.currentDelegate();
                    }
                    return transaction;

                case EnterpriseServicesInteropOption.Automatic:
                    if (!UseServiceDomainForCurrent())
                    {
                        return contextData.CurrentTransaction;
                    }
                    return GetContextTransaction(contextData);

                case EnterpriseServicesInteropOption.Full:
                    return GetContextTransaction(contextData);
            }
            return transaction;
        }

        internal static Transaction GetContextTransaction(ContextData contextData)
        {
            if (EnterpriseServicesOk)
            {
                return JitSafeGetContextTransaction(contextData);
            }
            return null;
        }

        internal static void GetCurrentTransactionAndScope(out Transaction current, out TransactionScope currentScope, out ContextData contextData, out Transaction contextTransaction)
        {
            contextData = ContextData.CurrentData;
            currentScope = contextData.CurrentScope;
            current = FastGetTransaction(currentScope, contextData, out contextTransaction);
        }

        public override int GetHashCode()
        {
            return this.internalTransaction.TransactionHash;
        }

        internal Transaction InternalClone()
        {
            Transaction transaction = new Transaction(this.isoLevel, this.internalTransaction);
            if (DiagnosticTrace.Verbose)
            {
                CloneCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), transaction.TransactionTraceId);
            }
            return transaction;
        }

        internal virtual void InternalDispose()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "IDisposable.Dispose");
            }
            if (Interlocked.Exchange(ref this.disposed, 1) != 1)
            {
                long num = Interlocked.Decrement(ref this.internalTransaction.cloneCount);
                if (num == 0L)
                {
                    this.internalTransaction.Dispose();
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "IDisposable.Dispose");
                }
            }
        }

        internal static EnterpriseServicesInteropOption InteropMode(TransactionScope currentScope)
        {
            if (currentScope != null)
            {
                return currentScope.InteropMode;
            }
            return EnterpriseServicesInteropOption.None;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Transaction JitSafeGetContextTransaction(ContextData contextData)
        {
            SafeIUnknown safeUnknown = null;
            if (contextData.WeakDefaultComContext != null)
            {
                safeUnknown = (SafeIUnknown) contextData.WeakDefaultComContext.Target;
            }
            if ((contextData.DefaultComContextState == DefaultComContextState.Unknown) || ((contextData.DefaultComContextState == DefaultComContextState.Available) && (safeUnknown == null)))
            {
                try
                {
                    System.Transactions.NativeMethods.CoGetDefaultContext(-1, ref IID_IObjContext, out safeUnknown);
                    contextData.WeakDefaultComContext = new WeakReference(safeUnknown);
                    contextData.DefaultComContextState = DefaultComContextState.Available;
                }
                catch (EntryPointNotFoundException exception)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), exception);
                    }
                    contextData.DefaultComContextState = DefaultComContextState.Unavailable;
                }
            }
            if (contextData.DefaultComContextState == DefaultComContextState.Available)
            {
                IntPtr zero = IntPtr.Zero;
                System.Transactions.NativeMethods.CoGetContextToken(out zero);
                if (safeUnknown.DangerousGetHandle() == zero)
                {
                    return null;
                }
            }
            if (!ContextUtil.IsInTransaction)
            {
                return null;
            }
            return ContextUtil.SystemTransaction;
        }

        public static bool operator ==(Transaction x, Transaction y)
        {
            if (x != null)
            {
                return x.Equals(y);
            }
            return (y == null);
        }

        public static bool operator !=(Transaction x, Transaction y)
        {
            if (x != null)
            {
                return !x.Equals(y);
            }
            return (y != null);
        }

        internal OletxTransaction Promote()
        {
            lock (this.internalTransaction)
            {
                this.internalTransaction.State.Promote(this.internalTransaction);
                return this.internalTransaction.PromotedTransaction;
            }
        }

        public void Rollback()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.Rollback");
            }
            if (DiagnosticTrace.Warning)
            {
                TransactionRollbackCalledTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), this.TransactionTraceId);
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            lock (this.internalTransaction)
            {
                this.internalTransaction.State.Rollback(this.internalTransaction, null);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.Rollback");
            }
        }

        public void Rollback(Exception e)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.Rollback");
            }
            if (DiagnosticTrace.Warning)
            {
                TransactionRollbackCalledTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), this.TransactionTraceId);
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            lock (this.internalTransaction)
            {
                this.internalTransaction.State.Rollback(this.internalTransaction, e);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.Rollback");
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "ISerializable.GetObjectData");
            }
            if (this.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (serializationInfo == null)
            {
                throw new ArgumentNullException("serializationInfo");
            }
            if (this.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            lock (this.internalTransaction)
            {
                this.internalTransaction.State.GetObjectData(this.internalTransaction, serializationInfo, context);
            }
            if (DiagnosticTrace.Information)
            {
                TransactionSerializedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), this.TransactionTraceId);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "ISerializable.GetObjectData");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool UseServiceDomainForCurrent()
        {
            return !ContextUtil.IsDefaultContext();
        }

        internal static void VerifyEnterpriseServicesOk()
        {
            if (!EnterpriseServicesOk)
            {
                throw new NotSupportedException(System.Transactions.SR.GetString("EsNotSupported"));
            }
        }

        public static Transaction Current
        {
            get
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "Transaction.get_Current");
                }
                Transaction current = null;
                TransactionScope currentScope = null;
                ContextData contextData = null;
                Transaction contextTransaction = null;
                GetCurrentTransactionAndScope(out current, out currentScope, out contextData, out contextTransaction);
                if ((currentScope != null) && currentScope.ScopeComplete)
                {
                    throw new InvalidOperationException(System.Transactions.SR.GetString("TransactionScopeComplete"));
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "Transaction.get_Current");
                }
                return current;
            }
            set
            {
                if (!TransactionManager._platformValidated)
                {
                    TransactionManager.ValidatePlatform();
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "Transaction.set_Current");
                }
                if (InteropMode(ContextData.CurrentData.CurrentScope) != EnterpriseServicesInteropOption.None)
                {
                    if (DiagnosticTrace.Error)
                    {
                        InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("CannotSetCurrent"));
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("CannotSetCurrent"));
                }
                ContextData.CurrentData.CurrentTransaction = value;
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "Transaction.set_Current");
                }
            }
        }

        internal bool Disposed
        {
            get
            {
                return (this.disposed == 1);
            }
        }

        internal static bool EnterpriseServicesOk
        {
            get
            {
                if (_enterpriseServicesOk == EnterpriseServicesState.Unknown)
                {
                    if (null != Type.GetType("System.EnterpriseServices.ContextUtil, System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false))
                    {
                        _enterpriseServicesOk = EnterpriseServicesState.Available;
                    }
                    else
                    {
                        _enterpriseServicesOk = EnterpriseServicesState.Unavailable;
                    }
                }
                return (_enterpriseServicesOk == EnterpriseServicesState.Available);
            }
        }

        public System.Transactions.IsolationLevel IsolationLevel
        {
            get
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.get_IsolationLevel");
                }
                if (this.Disposed)
                {
                    throw new ObjectDisposedException("Transaction");
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.get_IsolationLevel");
                }
                return this.isoLevel;
            }
        }

        public System.Transactions.TransactionInformation TransactionInformation
        {
            get
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.get_TransactionInformation");
                }
                if (this.Disposed)
                {
                    throw new ObjectDisposedException("Transaction");
                }
                System.Transactions.TransactionInformation transactionInformation = this.internalTransaction.transactionInformation;
                if (transactionInformation == null)
                {
                    transactionInformation = new System.Transactions.TransactionInformation(this.internalTransaction);
                    this.internalTransaction.transactionInformation = transactionInformation;
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "Transaction.get_TransactionInformation");
                }
                return transactionInformation;
            }
        }

        internal TransactionTraceIdentifier TransactionTraceId
        {
            get
            {
                if (this.traceIdentifier == TransactionTraceIdentifier.Empty)
                {
                    lock (this.internalTransaction)
                    {
                        if (this.traceIdentifier == TransactionTraceIdentifier.Empty)
                        {
                            TransactionTraceIdentifier identifier = new TransactionTraceIdentifier(this.internalTransaction.TransactionTraceId.TransactionIdentifier, this.cloneId);
                            Thread.MemoryBarrier();
                            this.traceIdentifier = identifier;
                        }
                    }
                }
                return this.traceIdentifier;
            }
        }
    }
}

