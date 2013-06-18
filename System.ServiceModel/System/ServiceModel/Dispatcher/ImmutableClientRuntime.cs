namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics.Application;
    using System.Transactions;

    internal class ImmutableClientRuntime
    {
        private bool addTransactionFlowProperties;
        private IChannelInitializer[] channelInitializers;
        private int correlationCount;
        private IInteractiveChannelInitializer[] interactiveChannelInitializers;
        private IClientMessageInspector[] messageInspectors;
        private Dictionary<string, ProxyOperationRuntime> operations;
        private IClientOperationSelector operationSelector;
        private ProxyOperationRuntime unhandled;
        private bool useSynchronizationContext;
        private bool validateMustUnderstand;

        internal ImmutableClientRuntime(ClientRuntime behavior)
        {
            this.channelInitializers = EmptyArray<IChannelInitializer>.ToArray(behavior.ChannelInitializers);
            this.interactiveChannelInitializers = EmptyArray<IInteractiveChannelInitializer>.ToArray(behavior.InteractiveChannelInitializers);
            this.messageInspectors = EmptyArray<IClientMessageInspector>.ToArray(behavior.MessageInspectors);
            this.operationSelector = behavior.OperationSelector;
            this.useSynchronizationContext = behavior.UseSynchronizationContext;
            this.validateMustUnderstand = behavior.ValidateMustUnderstand;
            this.unhandled = new ProxyOperationRuntime(behavior.UnhandledClientOperation, this);
            this.addTransactionFlowProperties = behavior.AddTransactionFlowProperties;
            this.operations = new Dictionary<string, ProxyOperationRuntime>();
            for (int i = 0; i < behavior.Operations.Count; i++)
            {
                ClientOperation operation = behavior.Operations[i];
                ProxyOperationRuntime runtime = new ProxyOperationRuntime(operation, this);
                this.operations.Add(operation.Name, runtime);
            }
            this.correlationCount = this.messageInspectors.Length + behavior.MaxParameterInspectors;
        }

        internal void AfterReceiveReply(ref ProxyRpc rpc)
        {
            int messageInspectorCorrelationOffset = this.MessageInspectorCorrelationOffset;
            try
            {
                for (int i = 0; i < this.messageInspectors.Length; i++)
                {
                    this.messageInspectors[i].AfterReceiveReply(ref rpc.Reply, rpc.Correlation[messageInspectorCorrelationOffset + i]);
                    if (TD.ClientMessageInspectorAfterReceiveInvokedIsEnabled())
                    {
                        TD.ClientMessageInspectorAfterReceiveInvoked(this.messageInspectors[i].GetType().FullName);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
        }

        internal void BeforeSendRequest(ref ProxyRpc rpc)
        {
            int messageInspectorCorrelationOffset = this.MessageInspectorCorrelationOffset;
            try
            {
                for (int i = 0; i < this.messageInspectors.Length; i++)
                {
                    rpc.Correlation[messageInspectorCorrelationOffset + i] = this.messageInspectors[i].BeforeSendRequest(ref rpc.Request, (IClientChannel) rpc.Channel.Proxy);
                    if (TD.ClientMessageInspectorBeforeSendInvokedIsEnabled())
                    {
                        TD.ClientMessageInspectorBeforeSendInvoked(this.messageInspectors[i].GetType().FullName);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
            if (this.addTransactionFlowProperties)
            {
                SendTransaction(ref rpc);
            }
        }

        internal IAsyncResult BeginDisplayInitializationUI(ServiceChannel channel, AsyncCallback callback, object state)
        {
            return new DisplayInitializationUIAsyncResult(channel, this.interactiveChannelInitializers, callback, state);
        }

        internal void DisplayInitializationUI(ServiceChannel channel)
        {
            this.EndDisplayInitializationUI(this.BeginDisplayInitializationUI(channel, null, null));
        }

        internal void EndDisplayInitializationUI(IAsyncResult result)
        {
            DisplayInitializationUIAsyncResult.End(result);
        }

        internal ProxyOperationRuntime GetOperation(MethodBase methodBase, object[] args, out bool canCacheResult)
        {
            ProxyOperationRuntime runtime2;
            if (this.operationSelector == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SFxNeedProxyBehaviorOperationSelector2", new object[] { methodBase.Name, methodBase.DeclaringType.Name })));
            }
            try
            {
                ProxyOperationRuntime runtime;
                if (this.operationSelector.AreParametersRequiredForSelection)
                {
                    canCacheResult = false;
                }
                else
                {
                    args = null;
                    canCacheResult = true;
                }
                string key = this.operationSelector.SelectOperation(methodBase, args);
                if ((key != null) && this.operations.TryGetValue(key, out runtime))
                {
                    return runtime;
                }
                runtime2 = null;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
            return runtime2;
        }

        internal ProxyOperationRuntime GetOperationByName(string operationName)
        {
            ProxyOperationRuntime runtime = null;
            if (this.operations.TryGetValue(operationName, out runtime))
            {
                return runtime;
            }
            return null;
        }

        internal void InitializeChannel(IClientChannel channel)
        {
            try
            {
                for (int i = 0; i < this.channelInitializers.Length; i++)
                {
                    this.channelInitializers[i].Initialize(channel);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SendTransaction(ref ProxyRpc rpc)
        {
            TransactionFlowProperty.Set(Transaction.Current, rpc.Request);
        }

        internal int CorrelationCount
        {
            get
            {
                return this.correlationCount;
            }
        }

        internal int MessageInspectorCorrelationOffset
        {
            get
            {
                return 0;
            }
        }

        internal IClientOperationSelector OperationSelector
        {
            get
            {
                return this.operationSelector;
            }
        }

        internal int ParameterInspectorCorrelationOffset
        {
            get
            {
                return this.messageInspectors.Length;
            }
        }

        internal ProxyOperationRuntime UnhandledProxyOperation
        {
            get
            {
                return this.unhandled;
            }
        }

        internal bool UseSynchronizationContext
        {
            get
            {
                return this.useSynchronizationContext;
            }
        }

        internal bool ValidateMustUnderstand
        {
            get
            {
                return this.validateMustUnderstand;
            }
            set
            {
                this.validateMustUnderstand = value;
            }
        }

        private class DisplayInitializationUIAsyncResult : AsyncResult
        {
            private static AsyncCallback callback = Fx.ThunkCallback(new AsyncCallback(ImmutableClientRuntime.DisplayInitializationUIAsyncResult.Callback));
            private ServiceChannel channel;
            private int index;
            private IInteractiveChannelInitializer[] initializers;
            private IClientChannel proxy;

            internal DisplayInitializationUIAsyncResult(ServiceChannel channel, IInteractiveChannelInitializer[] initializers, AsyncCallback callback, object state) : base(callback, state)
            {
                this.index = -1;
                this.channel = channel;
                this.initializers = initializers;
                this.proxy = channel.Proxy as IClientChannel;
                this.CallBegin(true);
            }

            private static void Callback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ImmutableClientRuntime.DisplayInitializationUIAsyncResult asyncState = (ImmutableClientRuntime.DisplayInitializationUIAsyncResult) result.AsyncState;
                    Exception exception = null;
                    asyncState.CallEnd(result, out exception);
                    if (exception != null)
                    {
                        asyncState.CallComplete(false, exception);
                    }
                    else
                    {
                        asyncState.CallBegin(false);
                    }
                }
            }

            private void CallBegin(bool completedSynchronously)
            {
                while (++this.index < this.initializers.Length)
                {
                    IAsyncResult result = null;
                    Exception exception = null;
                    try
                    {
                        result = this.initializers[this.index].BeginDisplayInitializationUI(this.proxy, callback, this);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (exception == null)
                    {
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        this.CallEnd(result, out exception);
                    }
                    if (exception != null)
                    {
                        this.CallComplete(completedSynchronously, exception);
                        return;
                    }
                }
                this.CallComplete(completedSynchronously, null);
            }

            private void CallComplete(bool completedSynchronously, Exception exception)
            {
                base.Complete(completedSynchronously, exception);
            }

            private void CallEnd(IAsyncResult result, out Exception exception)
            {
                try
                {
                    this.initializers[this.index].EndDisplayInitializationUI(result);
                    exception = null;
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<ImmutableClientRuntime.DisplayInitializationUIAsyncResult>(result);
            }
        }
    }
}

