namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MessageRpc
    {
        internal readonly ServiceChannel Channel;
        internal readonly ChannelHandler channelHandler;
        internal readonly object[] Correlation;
        internal readonly ServiceHostBase Host;
        internal readonly System.ServiceModel.OperationContext OperationContext;
        internal ServiceModelActivity Activity;
        internal Guid ResponseActivityId;
        internal IAsyncResult AsyncResult;
        internal bool CanSendReply;
        internal bool SuccessfullySendReply;
        internal CorrelationCallbackMessageProperty CorrelationCallback;
        internal object[] InputParameters;
        internal object[] OutputParameters;
        internal object ReturnParameter;
        internal bool ParametersDisposed;
        internal bool DidDeserializeRequestBody;
        internal System.ServiceModel.Channels.TransactionMessageProperty TransactionMessageProperty;
        internal System.ServiceModel.Dispatcher.TransactedBatchContext TransactedBatchContext;
        internal Exception Error;
        internal MessageRpcProcessor ErrorProcessor;
        internal ErrorHandlerFaultInfo FaultInfo;
        internal bool HasSecurityContext;
        internal object Instance;
        internal bool MessageRpcOwnsInstanceContextThrottle;
        internal MessageRpcProcessor NextProcessor;
        internal Collection<MessageHeaderInfo> NotUnderstoodHeaders;
        internal DispatchOperationRuntime Operation;
        internal Message Request;
        internal System.ServiceModel.Channels.RequestContext RequestContext;
        internal bool RequestContextThrewOnReply;
        internal UniqueId RequestID;
        internal Message Reply;
        internal TimeoutHelper ReplyTimeoutHelper;
        internal System.ServiceModel.Channels.RequestReplyCorrelator.ReplyToInfo ReplyToInfo;
        internal MessageVersion RequestVersion;
        internal ServiceSecurityContext SecurityContext;
        internal System.ServiceModel.InstanceContext InstanceContext;
        internal bool SuccessfullyBoundInstance;
        internal bool SuccessfullyIncrementedActivity;
        internal bool SuccessfullyLockedInstance;
        internal ReceiveContextRPCFacet ReceiveContext;
        internal TransactionRpcFacet transaction;
        internal IAspNetMessageProperty HostingProperty;
        internal MessageRpcInvokeNotification InvokeNotification;
        private static AsyncCallback handleEndComplete;
        private static AsyncCallback handleEndAbandon;
        private bool paused;
        private bool switchedThreads;
        private bool isInstanceContextSingleton;
        private SignalGate<IAsyncResult> invokeContinueGate;
        internal MessageRpc(System.ServiceModel.Channels.RequestContext requestContext, Message request, DispatchOperationRuntime operation, ServiceChannel channel, ServiceHostBase host, ChannelHandler channelHandler, bool cleanThread, System.ServiceModel.OperationContext operationContext, System.ServiceModel.InstanceContext instanceContext)
        {
            this.Activity = null;
            this.AsyncResult = null;
            this.CanSendReply = true;
            this.Channel = channel;
            this.channelHandler = channelHandler;
            this.Correlation = EmptyArray.Allocate(operation.Parent.CorrelationCount);
            this.CorrelationCallback = null;
            this.DidDeserializeRequestBody = false;
            this.TransactionMessageProperty = null;
            this.TransactedBatchContext = null;
            this.Error = null;
            this.ErrorProcessor = null;
            this.FaultInfo = new ErrorHandlerFaultInfo(request.Version.Addressing.DefaultFaultAction);
            this.HasSecurityContext = false;
            this.Host = host;
            this.Instance = null;
            this.MessageRpcOwnsInstanceContextThrottle = false;
            this.NextProcessor = null;
            this.NotUnderstoodHeaders = null;
            this.Operation = operation;
            this.OperationContext = operationContext;
            this.paused = false;
            this.ParametersDisposed = false;
            this.ReceiveContext = null;
            this.Request = request;
            this.RequestContext = requestContext;
            this.RequestContextThrewOnReply = false;
            this.SuccessfullySendReply = false;
            this.RequestVersion = request.Version;
            this.Reply = null;
            this.ReplyTimeoutHelper = new TimeoutHelper();
            this.SecurityContext = null;
            this.InstanceContext = instanceContext;
            this.SuccessfullyBoundInstance = false;
            this.SuccessfullyIncrementedActivity = false;
            this.SuccessfullyLockedInstance = false;
            this.switchedThreads = !cleanThread;
            this.transaction = null;
            this.InputParameters = null;
            this.OutputParameters = null;
            this.ReturnParameter = null;
            this.isInstanceContextSingleton = InstanceContextProviderBase.IsProviderSingleton(this.Channel.DispatchRuntime.InstanceContextProvider);
            this.invokeContinueGate = null;
            if (!operation.IsOneWay && !operation.Parent.ManualAddressing)
            {
                this.RequestID = request.Headers.MessageId;
                this.ReplyToInfo = new System.ServiceModel.Channels.RequestReplyCorrelator.ReplyToInfo(request);
            }
            else
            {
                this.RequestID = null;
                this.ReplyToInfo = new System.ServiceModel.Channels.RequestReplyCorrelator.ReplyToInfo();
            }
            this.HostingProperty = AspNetEnvironment.Current.PrepareMessageForDispatch(request);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                this.Activity = TraceUtility.ExtractActivity(this.Request);
            }
            if (DiagnosticUtility.ShouldUseActivity || TraceUtility.ShouldPropagateActivity)
            {
                this.ResponseActivityId = ActivityIdHeader.ExtractActivityId(this.Request);
            }
            else
            {
                this.ResponseActivityId = Guid.Empty;
            }
            this.InvokeNotification = new MessageRpcInvokeNotification(this.Activity, this.channelHandler);
        }

        internal bool FinalizeCorrelationImplicitly
        {
            get
            {
                return ((this.CorrelationCallback != null) && this.CorrelationCallback.IsFullyDefined);
            }
        }
        internal bool IsPaused
        {
            get
            {
                return this.paused;
            }
        }
        internal bool SwitchedThreads
        {
            get
            {
                return this.switchedThreads;
            }
        }
        internal bool IsInstanceContextSingleton
        {
            set
            {
                this.isInstanceContextSingleton = value;
            }
        }
        internal TransactionRpcFacet Transaction
        {
            get
            {
                if (this.transaction == null)
                {
                    this.transaction = new TransactionRpcFacet(ref this);
                }
                return this.transaction;
            }
        }
        internal void Abort()
        {
            this.AbortRequestContext();
            this.AbortChannel();
            this.AbortInstanceContext();
        }

        private void AbortRequestContext(System.ServiceModel.Channels.RequestContext requestContext)
        {
            try
            {
                requestContext.Abort();
                ReceiveContextRPCFacet receiveContext = this.ReceiveContext;
                if (receiveContext != null)
                {
                    this.ReceiveContext = null;
                    CallbackState state = new CallbackState {
                        ReceiveContext = receiveContext,
                        ChannelHandler = this.channelHandler
                    };
                    IAsyncResult result = receiveContext.BeginAbandon(TimeSpan.MaxValue, handleEndAbandon, state);
                    if (result.CompletedSynchronously)
                    {
                        receiveContext.EndAbandon(result);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.channelHandler.HandleError(exception);
            }
        }

        internal void AbortRequestContext()
        {
            if (this.OperationContext.RequestContext != null)
            {
                this.AbortRequestContext(this.OperationContext.RequestContext);
            }
            if ((this.RequestContext != null) && (this.RequestContext != this.OperationContext.RequestContext))
            {
                this.AbortRequestContext(this.RequestContext);
            }
        }

        internal void CloseRequestContext()
        {
            if (this.OperationContext.RequestContext != null)
            {
                this.DisposeRequestContext(this.OperationContext.RequestContext);
            }
            if ((this.RequestContext != null) && (this.RequestContext != this.OperationContext.RequestContext))
            {
                this.DisposeRequestContext(this.RequestContext);
            }
        }

        private void DisposeRequestContext(System.ServiceModel.Channels.RequestContext context)
        {
            try
            {
                context.Close();
                ReceiveContextRPCFacet receiveContext = this.ReceiveContext;
                if (receiveContext != null)
                {
                    this.ReceiveContext = null;
                    CallbackState state = new CallbackState {
                        ChannelHandler = this.channelHandler,
                        ReceiveContext = receiveContext
                    };
                    IAsyncResult result = receiveContext.BeginComplete(TimeSpan.MaxValue, null, this.channelHandler, handleEndComplete, state);
                    if (result.CompletedSynchronously)
                    {
                        receiveContext.EndComplete(result);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.AbortRequestContext(context);
                this.channelHandler.HandleError(exception);
            }
        }

        private static void HandleEndAbandon(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CallbackState asyncState = (CallbackState) result.AsyncState;
                try
                {
                    asyncState.ReceiveContext.EndAbandon(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.ChannelHandler.HandleError(exception);
                }
            }
        }

        private static void HandleEndComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CallbackState asyncState = (CallbackState) result.AsyncState;
                try
                {
                    asyncState.ReceiveContext.EndComplete(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.ChannelHandler.HandleError(exception);
                }
            }
        }

        internal void AbortChannel()
        {
            if ((this.Channel != null) && this.Channel.HasSession)
            {
                try
                {
                    this.Channel.Abort();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.channelHandler.HandleError(exception);
                }
            }
        }

        internal void CloseChannel()
        {
            if ((this.Channel != null) && this.Channel.HasSession)
            {
                try
                {
                    this.Channel.Close(ChannelHandler.CloseAfterFaultTimeout);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.channelHandler.HandleError(exception);
                }
            }
        }

        internal void AbortInstanceContext()
        {
            if ((this.InstanceContext != null) && !this.isInstanceContextSingleton)
            {
                try
                {
                    this.InstanceContext.Abort();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.channelHandler.HandleError(exception);
                }
            }
        }

        internal void EnsureReceive()
        {
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                ChannelHandler.Register(this.channelHandler);
            }
        }

        private bool ProcessError(Exception e)
        {
            MessageRpcProcessor errorProcessor = this.ErrorProcessor;
            try
            {
                if (e.GetType().IsAssignableFrom(typeof(FaultException)))
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                }
                else if (DiagnosticUtility.ShouldTraceError)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(e, TraceEventType.Error);
                }
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    TraceUtility.SetActivityId(this.Request.Properties);
                    if (Guid.Empty == DiagnosticTrace.ActivityId)
                    {
                        Guid guid = TraceUtility.ExtractActivityId(this.Request);
                        if (Guid.Empty != guid)
                        {
                            DiagnosticTrace.ActivityId = guid;
                        }
                    }
                }
                this.Error = e;
                if (this.ErrorProcessor != null)
                {
                    this.ErrorProcessor(ref this);
                }
                return (this.Error == null);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                return ((errorProcessor != this.ErrorProcessor) && this.ProcessError(exception));
            }
        }

        internal void DisposeParameters(bool excludeInput)
        {
            if (this.Operation.DisposeParameters)
            {
                this.DisposeParametersCore(excludeInput);
            }
        }

        internal void DisposeParametersCore(bool excludeInput)
        {
            if (!this.ParametersDisposed)
            {
                if (!excludeInput)
                {
                    this.DisposeParameterList(this.InputParameters);
                }
                this.DisposeParameterList(this.OutputParameters);
                IDisposable returnParameter = this.ReturnParameter as IDisposable;
                if (returnParameter != null)
                {
                    try
                    {
                        returnParameter.Dispose();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.channelHandler.HandleError(exception);
                    }
                }
                this.ParametersDisposed = true;
            }
        }

        private void DisposeParameterList(object[] parameters)
        {
            IDisposable disposable = null;
            if (parameters != null)
            {
                foreach (object obj2 in parameters)
                {
                    disposable = obj2 as IDisposable;
                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            this.channelHandler.HandleError(exception);
                        }
                    }
                }
            }
        }

        internal IResumeMessageRpc Pause()
        {
            Wrapper wrapper = new Wrapper(ref this);
            this.paused = true;
            return wrapper;
        }

        [SecurityCritical]
        private IDisposable ApplyHostingIntegrationContext()
        {
            if (this.HostingProperty != null)
            {
                return this.ApplyHostingIntegrationContextNoInline();
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private IDisposable ApplyHostingIntegrationContextNoInline()
        {
            return this.HostingProperty.ApplyIntegrationContext();
        }

        [SecuritySafeCritical]
        internal bool Process(bool isOperationContextSet)
        {
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                bool flag = true;
                if (this.NextProcessor != null)
                {
                    System.ServiceModel.OperationContext context;
                    System.ServiceModel.OperationContext.Holder currentHolder;
                    MessageRpcProcessor nextProcessor = this.NextProcessor;
                    this.NextProcessor = null;
                    if (!isOperationContextSet)
                    {
                        currentHolder = System.ServiceModel.OperationContext.CurrentHolder;
                        context = currentHolder.Context;
                    }
                    else
                    {
                        currentHolder = null;
                        context = null;
                    }
                    this.IncrementBusyCount();
                    IDisposable disposable = this.ApplyHostingIntegrationContext();
                    try
                    {
                        if (!isOperationContextSet)
                        {
                            currentHolder.Context = this.OperationContext;
                        }
                        nextProcessor(ref this);
                        if (!this.paused)
                        {
                            this.OperationContext.SetClientReply(null, false);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!this.ProcessError(exception) && (this.FaultInfo.Fault == null))
                        {
                            this.Abort();
                        }
                    }
                    finally
                    {
                        try
                        {
                            this.DecrementBusyCount();
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                            if (!isOperationContextSet)
                            {
                                currentHolder.Context = context;
                            }
                            flag = !this.paused;
                            if (flag)
                            {
                                this.channelHandler.DispatchDone();
                                this.OperationContext.ClearClientReplyNoThrow();
                            }
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception2.Message, exception2);
                        }
                    }
                }
                return flag;
            }
        }

        internal void UnPause()
        {
            this.paused = false;
            this.DecrementBusyCount();
        }

        internal bool UnlockInvokeContinueGate(out IAsyncResult result)
        {
            return this.invokeContinueGate.Unlock(out result);
        }

        internal void PrepareInvokeContinueGate()
        {
            this.invokeContinueGate = new SignalGate<IAsyncResult>();
        }

        private void IncrementBusyCount()
        {
            AspNetEnvironment.Current.IncrementBusyCount();
            if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
            {
                AspNetEnvironment.Current.TraceIncrementBusyCount(System.ServiceModel.SR.GetString("ServiceBusyCountTrace", new object[] { this.Operation.Action }));
            }
        }

        private void DecrementBusyCount()
        {
            AspNetEnvironment.Current.DecrementBusyCount();
            if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
            {
                AspNetEnvironment.Current.TraceDecrementBusyCount(System.ServiceModel.SR.GetString("ServiceBusyCountTrace", new object[] { this.Operation.Action }));
            }
        }

        static MessageRpc()
        {
            handleEndComplete = Fx.ThunkCallback(new AsyncCallback(MessageRpc.HandleEndComplete));
            handleEndAbandon = Fx.ThunkCallback(new AsyncCallback(MessageRpc.HandleEndAbandon));
        }
        private class CallbackState
        {
            public System.ServiceModel.Dispatcher.ChannelHandler ChannelHandler { get; set; }

            public ReceiveContextRPCFacet ReceiveContext { get; set; }
        }

        private class Wrapper : IResumeMessageRpc
        {
            private bool alreadyResumed;
            private MessageRpc rpc;

            internal Wrapper(ref MessageRpc rpc)
            {
                this.rpc = rpc;
                MessageRpcProcessor nextProcessor = rpc.NextProcessor;
                this.rpc.IncrementBusyCount();
            }

            public InstanceContext GetMessageInstanceContext()
            {
                return this.rpc.InstanceContext;
            }

            public void Resume()
            {
                using (ServiceModelActivity.BoundOperation(this.rpc.Activity, true))
                {
                    bool flag;
                    this.Resume(out flag);
                    if (flag)
                    {
                        Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMultipleCallbackFromAsyncOperation", new object[] { this.rpc.Operation.Name }));
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                }
            }

            public void Resume(out bool alreadyResumedNoLock)
            {
                try
                {
                    alreadyResumedNoLock = this.alreadyResumed;
                    this.alreadyResumed = true;
                    this.rpc.switchedThreads = true;
                    if (this.rpc.Process(false) && !this.rpc.InvokeNotification.DidInvokerEnsurePump)
                    {
                        this.rpc.EnsureReceive();
                    }
                }
                finally
                {
                    this.rpc.DecrementBusyCount();
                }
            }

            public void Resume(IAsyncResult result)
            {
                this.rpc.AsyncResult = result;
                this.Resume();
            }

            public void Resume(object instance)
            {
                this.rpc.Instance = instance;
                this.Resume();
            }

            public void SignalConditionalResume(IAsyncResult result)
            {
                if (this.rpc.invokeContinueGate.Signal(result))
                {
                    this.rpc.AsyncResult = result;
                    this.Resume();
                }
            }
        }
    }
}

