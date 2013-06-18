namespace System.Transactions
{
    using System;
    using System.EnterpriseServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions.Diagnostics;
    using System.Transactions.Oletx;

    public sealed class TransactionScope : IDisposable
    {
        private CommittableTransaction committableTransaction;
        private bool complete;
        private Transaction contextTransaction;
        private bool createdDoubleServiceDomain;
        private bool createdServiceDomain;
        private DependentTransaction dependentTransaction;
        private bool disposed;
        private Transaction expectedCurrent;
        private bool interopModeSpecified;
        private EnterpriseServicesInteropOption interopOption;
        private Transaction savedCurrent;
        private TransactionScope savedCurrentScope;
        private Thread scopeThread;
        private Timer scopeTimer;
        private ContextData threadContextData;

        public TransactionScope() : this(TransactionScopeOption.Required)
        {
        }

        public TransactionScope(Transaction transactionToUse)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( Transaction )");
            }
            this.Initialize(transactionToUse, TimeSpan.Zero, false);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( Transaction )");
            }
        }

        public TransactionScope(TransactionScopeOption scopeOption)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption )");
            }
            if (this.NeedToCreateTransaction(scopeOption))
            {
                this.committableTransaction = new CommittableTransaction();
                this.expectedCurrent = this.committableTransaction.Clone();
            }
            if (DiagnosticTrace.Information)
            {
                if (null == this.expectedCurrent)
                {
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), TransactionTraceIdentifier.Empty, TransactionScopeResult.NoTransaction);
                }
                else
                {
                    TransactionScopeResult usingExistingCurrent;
                    if (null == this.committableTransaction)
                    {
                        usingExistingCurrent = TransactionScopeResult.UsingExistingCurrent;
                    }
                    else
                    {
                        usingExistingCurrent = TransactionScopeResult.CreatedTransaction;
                    }
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId, usingExistingCurrent);
                }
            }
            this.PushScope();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption )");
            }
        }

        public TransactionScope(Transaction transactionToUse, TimeSpan scopeTimeout)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( Transaction, TimeSpan )");
            }
            this.Initialize(transactionToUse, scopeTimeout, false);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( Transaction, TimeSpan )");
            }
        }

        public TransactionScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption, TimeSpan )");
            }
            this.ValidateScopeTimeout("scopeTimeout", scopeTimeout);
            TimeSpan timeout = TransactionManager.ValidateTimeout(scopeTimeout);
            if (this.NeedToCreateTransaction(scopeOption))
            {
                this.committableTransaction = new CommittableTransaction(timeout);
                this.expectedCurrent = this.committableTransaction.Clone();
            }
            if (((null != this.expectedCurrent) && (null == this.committableTransaction)) && (TimeSpan.Zero != scopeTimeout))
            {
                this.scopeTimer = new Timer(new System.Threading.TimerCallback(TransactionScope.TimerCallback), this, scopeTimeout, TimeSpan.Zero);
            }
            if (DiagnosticTrace.Information)
            {
                if (null == this.expectedCurrent)
                {
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), TransactionTraceIdentifier.Empty, TransactionScopeResult.NoTransaction);
                }
                else
                {
                    TransactionScopeResult usingExistingCurrent;
                    if (null == this.committableTransaction)
                    {
                        usingExistingCurrent = TransactionScopeResult.UsingExistingCurrent;
                    }
                    else
                    {
                        usingExistingCurrent = TransactionScopeResult.CreatedTransaction;
                    }
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId, usingExistingCurrent);
                }
            }
            this.PushScope();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption, TimeSpan )");
            }
        }

        public TransactionScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption, TransactionOptions )");
            }
            this.ValidateScopeTimeout("transactionOptions.Timeout", transactionOptions.Timeout);
            TimeSpan timeout = transactionOptions.Timeout;
            transactionOptions.Timeout = TransactionManager.ValidateTimeout(transactionOptions.Timeout);
            TransactionManager.ValidateIsolationLevel(transactionOptions.IsolationLevel);
            if (this.NeedToCreateTransaction(scopeOption))
            {
                this.committableTransaction = new CommittableTransaction(transactionOptions);
                this.expectedCurrent = this.committableTransaction.Clone();
            }
            else if (((null != this.expectedCurrent) && (IsolationLevel.Unspecified != transactionOptions.IsolationLevel)) && (this.expectedCurrent.IsolationLevel != transactionOptions.IsolationLevel))
            {
                throw new ArgumentException(System.Transactions.SR.GetString("TransactionScopeIsolationLevelDifferentFromTransaction"), "transactionOptions.IsolationLevel");
            }
            if (((null != this.expectedCurrent) && (null == this.committableTransaction)) && (TimeSpan.Zero != timeout))
            {
                this.scopeTimer = new Timer(new System.Threading.TimerCallback(TransactionScope.TimerCallback), this, timeout, TimeSpan.Zero);
            }
            if (DiagnosticTrace.Information)
            {
                if (null == this.expectedCurrent)
                {
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), TransactionTraceIdentifier.Empty, TransactionScopeResult.NoTransaction);
                }
                else
                {
                    TransactionScopeResult usingExistingCurrent;
                    if (null == this.committableTransaction)
                    {
                        usingExistingCurrent = TransactionScopeResult.UsingExistingCurrent;
                    }
                    else
                    {
                        usingExistingCurrent = TransactionScopeResult.CreatedTransaction;
                    }
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId, usingExistingCurrent);
                }
            }
            this.PushScope();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption, TransactionOptions )");
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public TransactionScope(Transaction transactionToUse, TimeSpan scopeTimeout, EnterpriseServicesInteropOption interopOption)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( Transaction, TimeSpan, EnterpriseServicesInteropOption )");
            }
            this.ValidateInteropOption(interopOption);
            this.interopOption = interopOption;
            this.Initialize(transactionToUse, scopeTimeout, true);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( Transaction, TimeSpan, EnterpriseServicesInteropOption )");
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public TransactionScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, EnterpriseServicesInteropOption interopOption)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption, TransactionOptions, EnterpriseServicesInteropOption )");
            }
            this.ValidateScopeTimeout("transactionOptions.Timeout", transactionOptions.Timeout);
            TimeSpan timeout = transactionOptions.Timeout;
            transactionOptions.Timeout = TransactionManager.ValidateTimeout(transactionOptions.Timeout);
            TransactionManager.ValidateIsolationLevel(transactionOptions.IsolationLevel);
            this.ValidateInteropOption(interopOption);
            this.interopModeSpecified = true;
            this.interopOption = interopOption;
            if (this.NeedToCreateTransaction(scopeOption))
            {
                this.committableTransaction = new CommittableTransaction(transactionOptions);
                this.expectedCurrent = this.committableTransaction.Clone();
            }
            else if (((null != this.expectedCurrent) && (IsolationLevel.Unspecified != transactionOptions.IsolationLevel)) && (this.expectedCurrent.IsolationLevel != transactionOptions.IsolationLevel))
            {
                throw new ArgumentException(System.Transactions.SR.GetString("TransactionScopeIsolationLevelDifferentFromTransaction"), "transactionOptions.IsolationLevel");
            }
            if (((null != this.expectedCurrent) && (null == this.committableTransaction)) && (TimeSpan.Zero != timeout))
            {
                this.scopeTimer = new Timer(new System.Threading.TimerCallback(TransactionScope.TimerCallback), this, timeout, TimeSpan.Zero);
            }
            if (DiagnosticTrace.Information)
            {
                if (null == this.expectedCurrent)
                {
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), TransactionTraceIdentifier.Empty, TransactionScopeResult.NoTransaction);
                }
                else
                {
                    TransactionScopeResult usingExistingCurrent;
                    if (null == this.committableTransaction)
                    {
                        usingExistingCurrent = TransactionScopeResult.UsingExistingCurrent;
                    }
                    else
                    {
                        usingExistingCurrent = TransactionScopeResult.CreatedTransaction;
                    }
                    TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId, usingExistingCurrent);
                }
            }
            this.PushScope();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.ctor( TransactionScopeOption, TransactionOptions, EnterpriseServicesInteropOption )");
            }
        }

        private void CommonInitialize()
        {
            this.complete = false;
            this.dependentTransaction = null;
            this.disposed = false;
            this.committableTransaction = null;
            this.expectedCurrent = null;
            this.scopeTimer = null;
            this.scopeThread = Thread.CurrentThread;
            Transaction.GetCurrentTransactionAndScope(out this.savedCurrent, out this.savedCurrentScope, out this.threadContextData, out this.contextTransaction);
        }

        public void Complete()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.Complete");
            }
            if (this.disposed)
            {
                throw new ObjectDisposedException("TransactionScope");
            }
            if (this.complete)
            {
                throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("DisposeScope"), null);
            }
            this.complete = true;
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.Complete");
            }
        }

        public void Dispose()
        {
            bool flag = false;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.Dispose");
            }
            if (this.disposed)
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.Dispose");
                }
            }
            else
            {
                if (this.scopeThread != Thread.CurrentThread)
                {
                    if (DiagnosticTrace.Error)
                    {
                        InvalidOperationExceptionTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("InvalidScopeThread"));
                    }
                    throw new InvalidOperationException(System.Transactions.SR.GetString("InvalidScopeThread"));
                }
                Exception exception = null;
                try
                {
                    this.disposed = true;
                    TransactionScope currentScope = this.threadContextData.CurrentScope;
                    Transaction contextTransaction = null;
                    Transaction transaction = Transaction.FastGetTransaction(currentScope, this.threadContextData, out contextTransaction);
                    if (!this.Equals(currentScope))
                    {
                        if (currentScope == null)
                        {
                            Transaction committableTransaction = this.committableTransaction;
                            if (committableTransaction == null)
                            {
                                committableTransaction = this.dependentTransaction;
                            }
                            committableTransaction.Rollback();
                            flag = true;
                            throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("TransactionScopeInvalidNesting"), null);
                        }
                        if ((currentScope.interopOption == EnterpriseServicesInteropOption.None) && (((null != currentScope.expectedCurrent) && !currentScope.expectedCurrent.Equals(transaction)) || ((null != transaction) && (null == currentScope.expectedCurrent))))
                        {
                            if (DiagnosticTrace.Warning)
                            {
                                TransactionTraceIdentifier transactionTraceId;
                                TransactionTraceIdentifier empty;
                                if (null == transaction)
                                {
                                    empty = TransactionTraceIdentifier.Empty;
                                }
                                else
                                {
                                    empty = transaction.TransactionTraceId;
                                }
                                if (null == this.expectedCurrent)
                                {
                                    transactionTraceId = TransactionTraceIdentifier.Empty;
                                }
                                else
                                {
                                    transactionTraceId = this.expectedCurrent.TransactionTraceId;
                                }
                                TransactionScopeCurrentChangedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), transactionTraceId, empty);
                            }
                            exception = TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("TransactionScopeIncorrectCurrent"), null);
                            if (null != transaction)
                            {
                                try
                                {
                                    transaction.Rollback();
                                }
                                catch (TransactionException)
                                {
                                }
                                catch (ObjectDisposedException)
                                {
                                }
                            }
                        }
                        while (!this.Equals(currentScope))
                        {
                            if (exception == null)
                            {
                                exception = TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("TransactionScopeInvalidNesting"), null);
                            }
                            if (DiagnosticTrace.Warning)
                            {
                                if (null == currentScope.expectedCurrent)
                                {
                                    TransactionScopeNestedIncorrectlyTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), TransactionTraceIdentifier.Empty);
                                }
                                else
                                {
                                    TransactionScopeNestedIncorrectlyTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), currentScope.expectedCurrent.TransactionTraceId);
                                }
                            }
                            currentScope.complete = false;
                            try
                            {
                                currentScope.InternalDispose();
                            }
                            catch (TransactionException)
                            {
                            }
                            currentScope = this.threadContextData.CurrentScope;
                            this.complete = false;
                        }
                    }
                    else if ((this.interopOption == EnterpriseServicesInteropOption.None) && (((null != this.expectedCurrent) && !this.expectedCurrent.Equals(transaction)) || ((null != transaction) && (null == this.expectedCurrent))))
                    {
                        if (DiagnosticTrace.Warning)
                        {
                            TransactionTraceIdentifier identifier;
                            TransactionTraceIdentifier identifier2;
                            if (null == transaction)
                            {
                                identifier2 = TransactionTraceIdentifier.Empty;
                            }
                            else
                            {
                                identifier2 = transaction.TransactionTraceId;
                            }
                            if (null == this.expectedCurrent)
                            {
                                identifier = TransactionTraceIdentifier.Empty;
                            }
                            else
                            {
                                identifier = this.expectedCurrent.TransactionTraceId;
                            }
                            TransactionScopeCurrentChangedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), identifier, identifier2);
                        }
                        if (exception == null)
                        {
                            exception = TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("TransactionScopeIncorrectCurrent"), null);
                        }
                        if (null != transaction)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (TransactionException)
                            {
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                        }
                        this.complete = false;
                    }
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.PopScope();
                    }
                }
                this.InternalDispose();
                if (exception != null)
                {
                    throw exception;
                }
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), "TransactionScope.Dispose");
                }
            }
        }

        private void Initialize(Transaction transactionToUse, TimeSpan scopeTimeout, bool interopModeSpecified)
        {
            if (null == transactionToUse)
            {
                throw new ArgumentNullException("transactionToUse");
            }
            this.ValidateScopeTimeout("scopeTimeout", scopeTimeout);
            this.CommonInitialize();
            if (TimeSpan.Zero != scopeTimeout)
            {
                this.scopeTimer = new Timer(new System.Threading.TimerCallback(TransactionScope.TimerCallback), this, scopeTimeout, TimeSpan.Zero);
            }
            this.expectedCurrent = transactionToUse;
            this.interopModeSpecified = interopModeSpecified;
            if (DiagnosticTrace.Information)
            {
                TransactionScopeCreatedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId, TransactionScopeResult.TransactionPassed);
            }
            this.PushScope();
        }

        private void InternalDispose()
        {
            this.disposed = true;
            try
            {
                this.PopScope();
                if (DiagnosticTrace.Information)
                {
                    if (null == this.expectedCurrent)
                    {
                        TransactionScopeDisposedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), TransactionTraceIdentifier.Empty);
                    }
                    else
                    {
                        TransactionScopeDisposedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId);
                    }
                }
                if (null != this.expectedCurrent)
                {
                    if (!this.complete)
                    {
                        if (DiagnosticTrace.Warning)
                        {
                            TransactionScopeIncompleteTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId);
                        }
                        Transaction committableTransaction = this.committableTransaction;
                        if (committableTransaction == null)
                        {
                            committableTransaction = this.dependentTransaction;
                        }
                        committableTransaction.Rollback();
                    }
                    else if (null != this.committableTransaction)
                    {
                        this.committableTransaction.Commit();
                    }
                    else
                    {
                        this.dependentTransaction.Complete();
                    }
                }
            }
            finally
            {
                if (this.scopeTimer != null)
                {
                    this.scopeTimer.Dispose();
                }
                if (null != this.committableTransaction)
                {
                    this.committableTransaction.Dispose();
                    this.expectedCurrent.Dispose();
                }
                if (null != this.dependentTransaction)
                {
                    this.dependentTransaction.Dispose();
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void JitSafeLeaveServiceDomain()
        {
            if (this.createdDoubleServiceDomain)
            {
                ServiceDomain.Leave();
            }
            ServiceDomain.Leave();
        }

        private bool NeedToCreateTransaction(TransactionScopeOption scopeOption)
        {
            bool flag = false;
            this.CommonInitialize();
            switch (scopeOption)
            {
                case TransactionScopeOption.Required:
                    this.expectedCurrent = this.savedCurrent;
                    if (null == this.expectedCurrent)
                    {
                        flag = true;
                    }
                    return flag;

                case TransactionScopeOption.RequiresNew:
                    return true;

                case TransactionScopeOption.Suppress:
                    this.expectedCurrent = null;
                    return false;
            }
            throw new ArgumentOutOfRangeException("scopeOption");
        }

        private void PopScope()
        {
            this.threadContextData.CurrentScope = this.savedCurrentScope;
            this.RestoreCurrent();
        }

        private void PushScope()
        {
            if (!this.interopModeSpecified)
            {
                this.interopOption = Transaction.InteropMode(this.savedCurrentScope);
            }
            this.SetCurrent(this.expectedCurrent);
            this.threadContextData.CurrentScope = this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void PushServiceDomain(Transaction newCurrent)
        {
            if (((newCurrent == null) || !newCurrent.Equals(ContextUtil.SystemTransaction)) && ((newCurrent != null) || (ContextUtil.SystemTransaction != null)))
            {
                ServiceConfig cfg = new ServiceConfig();
                try
                {
                    if (newCurrent != null)
                    {
                        cfg.Synchronization = SynchronizationOption.RequiresNew;
                        ServiceDomain.Enter(cfg);
                        this.createdDoubleServiceDomain = true;
                        cfg.Synchronization = SynchronizationOption.Required;
                        cfg.BringYourOwnSystemTransaction = newCurrent;
                    }
                    ServiceDomain.Enter(cfg);
                    this.createdServiceDomain = true;
                }
                catch (COMException exception)
                {
                    if (System.Transactions.Oletx.NativeMethods.XACT_E_NOTRANSACTION == exception.ErrorCode)
                    {
                        throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("TransactionAlreadyOver"), exception);
                    }
                    throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceBase"), exception.Message, exception);
                }
                finally
                {
                    if (!this.createdServiceDomain && this.createdDoubleServiceDomain)
                    {
                        ServiceDomain.Leave();
                    }
                }
            }
        }

        private void RestoreCurrent()
        {
            if (this.createdServiceDomain)
            {
                this.JitSafeLeaveServiceDomain();
            }
            this.threadContextData.CurrentTransaction = this.contextTransaction;
        }

        private void SetCurrent(Transaction newCurrent)
        {
            if (((this.dependentTransaction == null) && (this.committableTransaction == null)) && (newCurrent != null))
            {
                this.dependentTransaction = newCurrent.DependentClone(DependentCloneOption.RollbackIfNotComplete);
            }
            switch (this.interopOption)
            {
                case EnterpriseServicesInteropOption.None:
                    this.threadContextData.CurrentTransaction = newCurrent;
                    return;

                case EnterpriseServicesInteropOption.Automatic:
                    Transaction.VerifyEnterpriseServicesOk();
                    if (!Transaction.UseServiceDomainForCurrent())
                    {
                        this.threadContextData.CurrentTransaction = newCurrent;
                        return;
                    }
                    this.PushServiceDomain(newCurrent);
                    return;

                case EnterpriseServicesInteropOption.Full:
                    Transaction.VerifyEnterpriseServicesOk();
                    this.PushServiceDomain(newCurrent);
                    return;
            }
        }

        private void Timeout()
        {
            if (!this.complete && (null != this.expectedCurrent))
            {
                if (DiagnosticTrace.Warning)
                {
                    TransactionScopeTimeoutTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), this.expectedCurrent.TransactionTraceId);
                }
                try
                {
                    this.expectedCurrent.Rollback();
                }
                catch (ObjectDisposedException exception2)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), exception2);
                    }
                }
                catch (TransactionException exception)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), exception);
                    }
                }
            }
        }

        private static void TimerCallback(object state)
        {
            TransactionScope scope = state as TransactionScope;
            if (scope == null)
            {
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("TransactionScopeTimerObjectInvalid"));
                }
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceBase"), System.Transactions.SR.GetString("InternalError") + System.Transactions.SR.GetString("TransactionScopeTimerObjectInvalid"), null);
            }
            scope.Timeout();
        }

        private void ValidateInteropOption(EnterpriseServicesInteropOption interopOption)
        {
            if ((interopOption < EnterpriseServicesInteropOption.None) || (interopOption > EnterpriseServicesInteropOption.Full))
            {
                throw new ArgumentOutOfRangeException("interopOption");
            }
        }

        private void ValidateScopeTimeout(string paramName, TimeSpan scopeTimeout)
        {
            if (scopeTimeout < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }

        internal EnterpriseServicesInteropOption InteropMode
        {
            get
            {
                return this.interopOption;
            }
        }

        internal bool ScopeComplete
        {
            get
            {
                return this.complete;
            }
        }
    }
}

