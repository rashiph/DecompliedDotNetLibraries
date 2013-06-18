namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;

    internal class TransactionBehavior
    {
        private DispatchRuntime dispatch;
        private bool isConcurrent;
        private IsolationLevel isolation;
        private bool isTransactedReceiveChannelDispatcher;
        private TimeSpan timeout;

        internal TransactionBehavior()
        {
            this.isolation = ServiceBehaviorAttribute.DefaultIsolationLevel;
            this.timeout = TimeSpan.Zero;
        }

        internal TransactionBehavior(DispatchRuntime dispatch)
        {
            this.isolation = ServiceBehaviorAttribute.DefaultIsolationLevel;
            this.timeout = TimeSpan.Zero;
            this.isConcurrent = (dispatch.ConcurrencyMode == ConcurrencyMode.Multiple) || (dispatch.ConcurrencyMode == ConcurrencyMode.Reentrant);
            this.dispatch = dispatch;
            this.isTransactedReceiveChannelDispatcher = dispatch.ChannelDispatcher.IsTransactedReceive;
            if (dispatch.ChannelDispatcher.TransactionIsolationLevelSet)
            {
                this.InitializeIsolationLevel(dispatch);
            }
            this.timeout = NormalizeTimeout(dispatch.ChannelDispatcher.TransactionTimeout);
        }

        internal void ClearCallContext(ref MessageRpc rpc)
        {
            if (rpc.Operation.TransactionRequired)
            {
                rpc.Transaction.ThreadLeave();
            }
        }

        internal static Exception CreateFault(string reasonText, string codeString, bool isNetDispatcherFault)
        {
            string str;
            string str2;
            if (isNetDispatcherFault)
            {
                str = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher";
                str2 = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault";
            }
            else
            {
                str = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions";
                str2 = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault";
            }
            FaultReason reason = new FaultReason(reasonText, CultureInfo.CurrentCulture);
            return new FaultException(reason, FaultCode.CreateSenderFaultCode(codeString, str), str2);
        }

        internal static TransactionBehavior CreateIfNeeded(DispatchRuntime dispatch)
        {
            if (NeedsTransactionBehavior(dispatch))
            {
                return new TransactionBehavior(dispatch);
            }
            return null;
        }

        internal static CommittableTransaction CreateTransaction(IsolationLevel isolation, TimeSpan timeout)
        {
            return new CommittableTransaction(new TransactionOptions { IsolationLevel = isolation, Timeout = timeout });
        }

        private Transaction GetInstanceContextTransaction(ref MessageRpc rpc)
        {
            return rpc.InstanceContext.Transaction.Attached;
        }

        internal void InitializeCallContext(ref MessageRpc rpc)
        {
            if (rpc.Operation.TransactionRequired)
            {
                rpc.Transaction.ThreadEnter(ref rpc.Error);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeIsolationLevel(DispatchRuntime dispatch)
        {
            this.isolation = dispatch.ChannelDispatcher.TransactionIsolationLevel;
        }

        private static bool NeedsTransactionBehavior(DispatchRuntime dispatch)
        {
            DispatchOperation unhandledDispatchOperation = dispatch.UnhandledDispatchOperation;
            if ((unhandledDispatchOperation != null) && unhandledDispatchOperation.TransactionRequired)
            {
                return true;
            }
            if (dispatch.ChannelDispatcher.IsTransactedReceive)
            {
                return true;
            }
            for (int i = 0; i < dispatch.Operations.Count; i++)
            {
                DispatchOperation operation2 = dispatch.Operations[i];
                if (operation2.TransactionRequired)
                {
                    return true;
                }
            }
            return false;
        }

        internal static TimeSpan NormalizeTimeout(TimeSpan timeout)
        {
            if (TimeSpan.Zero == timeout)
            {
                timeout = TransactionManager.DefaultTimeout;
                return timeout;
            }
            if ((TimeSpan.Zero != TransactionManager.MaximumTimeout) && (timeout > TransactionManager.MaximumTimeout))
            {
                timeout = TransactionManager.MaximumTimeout;
            }
            return timeout;
        }

        internal void ResolveOutcome(ref MessageRpc rpc)
        {
            if ((rpc.InstanceContext != null) && (rpc.transaction != null))
            {
                TransactionInstanceContextFacet transaction = rpc.InstanceContext.Transaction;
                if (transaction != null)
                {
                    transaction.CheckIfTxCompletedAndUpdateAttached(ref rpc, this.isConcurrent);
                }
                rpc.Transaction.Complete(rpc.Error);
            }
        }

        internal void ResolveTransaction(ref MessageRpc rpc)
        {
            InstanceContext context;
            if (rpc.Operation.HasDefaultUnhandledActionInvoker)
            {
                return;
            }
            Transaction transactionForInstance = null;
            if (rpc.Operation.IsInsideTransactedReceiveScope)
            {
                IInstanceTransaction invoker = rpc.Operation.Invoker as IInstanceTransaction;
                if (invoker != null)
                {
                    transactionForInstance = invoker.GetTransactionForInstance(rpc.OperationContext);
                }
                if ((transactionForInstance != null) && DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0xe000f, System.ServiceModel.SR.GetString("TraceCodeTxSourceTxScopeRequiredUsingExistingTransaction", new object[] { transactionForInstance.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                }
            }
            else
            {
                transactionForInstance = this.GetInstanceContextTransaction(ref rpc);
            }
            Transaction transaction3 = null;
            try
            {
                transaction3 = TransactionMessageProperty.TryGetTransaction(rpc.Request);
            }
            catch (TransactionException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateFault(System.ServiceModel.SR.GetString("SFxTransactionUnmarshalFailed", new object[] { exception.Message }), "TransactionUnmarshalingFailed", false));
            }
            if (rpc.Operation.TransactionRequired)
            {
                if (transaction3 == null)
                {
                    goto Label_0256;
                }
                if (this.isTransactedReceiveChannelDispatcher)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0xe0001, System.ServiceModel.SR.GetString("TraceCodeTxSourceTxScopeRequiredIsTransactedTransport", new object[] { transaction3.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                    }
                    goto Label_0256;
                }
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0xe0002, System.ServiceModel.SR.GetString("TraceCodeTxSourceTxScopeRequiredIsTransactionFlow", new object[] { transaction3.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                }
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxFlowed(PerformanceCounters.GetEndpointDispatcher(), rpc.Operation.Name);
                }
                bool flag = false;
                if (rpc.Operation.IsInsideTransactedReceiveScope)
                {
                    flag = transaction3.Equals(transactionForInstance);
                }
                else
                {
                    flag = transaction3 == transactionForInstance;
                }
                if (flag)
                {
                    goto Label_0256;
                }
                try
                {
                    transaction3 = transaction3.DependentClone(DependentCloneOption.RollbackIfNotComplete);
                    goto Label_0256;
                }
                catch (TransactionException exception2)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true));
                }
            }
            if ((transaction3 != null) && this.isTransactedReceiveChannelDispatcher)
            {
                try
                {
                    if (rpc.TransactedBatchContext != null)
                    {
                        rpc.TransactedBatchContext.ForceCommit();
                        rpc.TransactedBatchContext = null;
                    }
                    else
                    {
                        TransactionInstanceContextFacet.Complete(transaction3, null);
                    }
                }
                finally
                {
                    transaction3.Dispose();
                    transaction3 = null;
                }
            }
        Label_0256:
            context = rpc.InstanceContext;
            if (context.Transaction.ShouldReleaseInstance && !this.isConcurrent)
            {
                if (context.Behavior.ReleaseServiceInstanceOnTransactionComplete)
                {
                    context.ReleaseServiceInstance();
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0xe000c, System.ServiceModel.SR.GetString("TraceCodeTxReleaseServiceInstanceOnCompletion", new object[] { transactionForInstance.TransactionInformation.LocalIdentifier }));
                    }
                }
                context.Transaction.ShouldReleaseInstance = false;
                if ((transaction3 == null) || (transaction3 == transactionForInstance))
                {
                    rpc.Transaction.Current = transactionForInstance;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true));
                }
                transactionForInstance = null;
            }
            if (rpc.Operation.TransactionRequired)
            {
                if (transaction3 == null)
                {
                    if (transactionForInstance != null)
                    {
                        transaction3 = transactionForInstance;
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information, 0xe0003, System.ServiceModel.SR.GetString("TraceCodeTxSourceTxScopeRequiredIsAttachedTransaction", new object[] { transaction3.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                        }
                    }
                    else
                    {
                        transaction3 = CreateTransaction(this.isolation, this.timeout);
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information, 0xe0004, System.ServiceModel.SR.GetString("TraceCodeTxSourceTxScopeRequiredIsCreateNewTransaction", new object[] { transaction3.TransactionInformation.LocalIdentifier, rpc.Operation.Name }));
                        }
                    }
                }
                if ((this.isolation != IsolationLevel.Unspecified) && (transaction3.IsolationLevel != this.isolation))
                {
                    throw TraceUtility.ThrowHelperError(CreateFault(System.ServiceModel.SR.GetString("IsolationLevelMismatch2", new object[] { transaction3.IsolationLevel, this.isolation }), "TransactionIsolationLevelMismatch", false), rpc.Request);
                }
                rpc.Transaction.Current = transaction3;
                rpc.InstanceContext.Transaction.AddReference(ref rpc, rpc.Transaction.Current, true);
                try
                {
                    rpc.Transaction.Clone = transaction3.Clone();
                    if (rpc.Operation.IsInsideTransactedReceiveScope)
                    {
                        rpc.Transaction.CreateDependentClone();
                    }
                }
                catch (ObjectDisposedException exception3)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateFault(System.ServiceModel.SR.GetString("SFxTransactionAsyncAborted"), "TransactionAborted", true));
                }
                rpc.InstanceContext.Transaction.AddReference(ref rpc, rpc.Transaction.Clone, false);
                rpc.OperationContext.TransactionFacet = rpc.Transaction;
                if (!rpc.Operation.TransactionAutoComplete)
                {
                    rpc.Transaction.SetIncomplete();
                }
            }
        }

        internal void SetCurrent(ref MessageRpc rpc)
        {
            if (!this.isConcurrent)
            {
                rpc.InstanceContext.Transaction.SetCurrent(ref rpc);
            }
        }
    }
}

