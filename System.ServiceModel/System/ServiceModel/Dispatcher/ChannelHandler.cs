namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Transactions;
    using System.Xml;

    internal class ChannelHandler
    {
        private WrappedTransaction acceptTransaction;
        private readonly IChannelBinder binder;
        private ServiceChannel channel;
        public static readonly TimeSpan CloseAfterFaultTimeout = TimeSpan.FromSeconds(10.0);
        private bool doneReceiving;
        private readonly DuplexChannelBinder duplexBinder;
        private bool hasRegisterBeenCalled;
        private bool hasSession;
        private readonly ServiceHostBase host;
        private readonly ServiceChannel.SessionIdleManager idleManager;
        private readonly bool incrementedActivityCountInConstructor;
        private ServiceThrottle instanceContextThrottle;
        private readonly bool isCallback;
        private bool isChannelTerminated;
        private bool isConcurrent;
        private bool isMainTransactedBatchHandler;
        private bool isManualAddressing;
        private int isPumpAcquired;
        private readonly ListenerHandler listener;
        public const string MessageBufferPropertyName = "_RequestMessageBuffer_";
        private MessageVersion messageVersion;
        private static AsyncCallback onAsyncReceiveComplete = Fx.ThunkCallback(new AsyncCallback(ChannelHandler.OnAsyncReceiveComplete));
        private static Action<object> onContinueAsyncReceive = new Action<object>(ChannelHandler.OnContinueAsyncReceive);
        private static Action<object> onStartAsyncMessagePump = new Action<object>(ChannelHandler.OnStartAsyncMessagePump);
        private static Action<object> onStartSingleTransactedBatch = new Action<object>(ChannelHandler.OnStartSingleTransactedBatch);
        private static Action<object> onStartSyncMessagePump = new Action<object>(ChannelHandler.OnStartSyncMessagePump);
        private static Action<object> openAndEnsurePump = new Action<object>(ChannelHandler.OpenAndEnsurePump);
        private ErrorHandlingReceiver receiver;
        private bool receiveSynchronously;
        private bool receiveWithTransaction;
        private RequestContext replied;
        private RequestInfo requestInfo;
        private RequestContext requestWaitingForThrottle;
        private SharedTransactedBatchContext sharedTransactedBatchContext;
        private readonly ServiceThrottle throttle;
        private TransactedBatchContext transactedBatchContext;
        private readonly bool wasChannelThrottled;

        internal ChannelHandler(ChannelHandler handler, TransactedBatchContext context)
        {
            this.messageVersion = handler.messageVersion;
            this.isManualAddressing = handler.isManualAddressing;
            this.binder = handler.binder;
            this.listener = handler.listener;
            this.wasChannelThrottled = handler.wasChannelThrottled;
            this.host = handler.host;
            this.receiveSynchronously = true;
            this.receiveWithTransaction = true;
            this.duplexBinder = handler.duplexBinder;
            this.hasSession = handler.hasSession;
            this.isConcurrent = handler.isConcurrent;
            this.receiver = handler.receiver;
            this.sharedTransactedBatchContext = context.Shared;
            this.transactedBatchContext = context;
            this.requestInfo = new RequestInfo(this);
        }

        internal ChannelHandler(MessageVersion messageVersion, IChannelBinder binder, ServiceChannel channel)
        {
            ClientRuntime clientRuntime = channel.ClientRuntime;
            this.messageVersion = messageVersion;
            this.isManualAddressing = clientRuntime.ManualAddressing;
            this.binder = binder;
            this.channel = channel;
            this.isConcurrent = true;
            this.duplexBinder = binder as DuplexChannelBinder;
            this.hasSession = binder.HasSession;
            this.isCallback = true;
            DispatchRuntime dispatchRuntime = clientRuntime.DispatchRuntime;
            if (dispatchRuntime == null)
            {
                this.receiver = new ErrorHandlingReceiver(binder, null);
            }
            else
            {
                this.receiver = new ErrorHandlingReceiver(binder, dispatchRuntime.ChannelDispatcher);
            }
            this.requestInfo = new RequestInfo(this);
        }

        internal ChannelHandler(MessageVersion messageVersion, IChannelBinder binder, ServiceThrottle throttle, ListenerHandler listener, bool wasChannelThrottled, WrappedTransaction acceptTransaction, ServiceChannel.SessionIdleManager idleManager)
        {
            ChannelDispatcher channelDispatcher = listener.ChannelDispatcher;
            this.messageVersion = messageVersion;
            this.isManualAddressing = channelDispatcher.ManualAddressing;
            this.binder = binder;
            this.throttle = throttle;
            this.listener = listener;
            this.wasChannelThrottled = wasChannelThrottled;
            this.host = listener.Host;
            this.receiveSynchronously = channelDispatcher.ReceiveSynchronously;
            this.duplexBinder = binder as DuplexChannelBinder;
            this.hasSession = binder.HasSession;
            this.isConcurrent = ConcurrencyBehavior.IsConcurrent(channelDispatcher, this.hasSession);
            if (channelDispatcher.MaxPendingReceives > 1)
            {
                this.binder = new MultipleReceiveBinder(this.binder, channelDispatcher.MaxPendingReceives, !this.isConcurrent);
            }
            if (channelDispatcher.BufferedReceiveEnabled)
            {
                this.binder = new BufferedReceiveBinder(this.binder);
            }
            this.receiver = new ErrorHandlingReceiver(this.binder, channelDispatcher);
            this.idleManager = idleManager;
            if (!channelDispatcher.IsTransactedReceive || channelDispatcher.ReceiveContextEnabled)
            {
                if ((channelDispatcher.IsTransactedReceive && channelDispatcher.ReceiveContextEnabled) && (channelDispatcher.MaxTransactedBatchSize > 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IncompatibleBehaviors")));
                }
            }
            else
            {
                this.receiveSynchronously = true;
                this.receiveWithTransaction = true;
                if (channelDispatcher.MaxTransactedBatchSize > 0)
                {
                    int maxConcurrentBatches = 1;
                    if ((throttle != null) && (throttle.MaxConcurrentCalls > 1))
                    {
                        maxConcurrentBatches = throttle.MaxConcurrentCalls;
                        foreach (EndpointDispatcher dispatcher2 in channelDispatcher.Endpoints)
                        {
                            if (ConcurrencyMode.Multiple != dispatcher2.DispatchRuntime.ConcurrencyMode)
                            {
                                maxConcurrentBatches = 1;
                                break;
                            }
                        }
                    }
                    this.sharedTransactedBatchContext = new SharedTransactedBatchContext(this, channelDispatcher, maxConcurrentBatches);
                    this.isMainTransactedBatchHandler = true;
                    this.throttle = null;
                }
            }
            this.acceptTransaction = acceptTransaction;
            this.requestInfo = new RequestInfo(this);
            if (!this.hasSession && (this.listener.State == CommunicationState.Opened))
            {
                this.listener.ChannelDispatcher.Channels.IncrementActivityCount();
                this.incrementedActivityCountInConstructor = true;
            }
        }

        private void AsyncMessagePump()
        {
            IAsyncResult result = this.BeginTryReceive();
            if ((result != null) && result.CompletedSynchronously)
            {
                this.AsyncMessagePump(result);
            }
        }

        private void AsyncMessagePump(IAsyncResult result)
        {
            RequestContext context;
        Label_0016:
            while (!this.EndTryReceive(result, out context))
            {
                result = this.BeginTryReceive();
                if ((result == null) || !result.CompletedSynchronously)
                {
                    return;
                }
            }
            if (this.HandleRequest(context, null) && this.TryAcquirePump())
            {
                result = this.BeginTryReceive();
                if ((result != null) && result.CompletedSynchronously)
                {
                    goto Label_0016;
                }
            }
        }

        private IAsyncResult BeginTryReceive()
        {
            this.requestInfo.Cleanup();
            return this.receiver.BeginTryReceive(TimeSpan.MaxValue, onAsyncReceiveComplete, this);
        }

        private Transaction CreateOrGetAttachedTransaction()
        {
            if (this.acceptTransaction != null)
            {
                lock (this.ThisLock)
                {
                    if (this.acceptTransaction != null)
                    {
                        Transaction transaction = this.acceptTransaction.Transaction;
                        this.acceptTransaction = null;
                        return transaction;
                    }
                }
            }
            if ((this.InstanceContext != null) && this.InstanceContext.HasTransaction)
            {
                return this.InstanceContext.Transaction.Attached;
            }
            return TransactionBehavior.CreateTransaction(this.listener.ChannelDispatcher.TransactionIsolationLevel, TransactionBehavior.NormalizeTimeout(this.listener.ChannelDispatcher.TransactionTimeout));
        }

        private bool DispatchAndReleasePump(RequestContext request, bool cleanThread, OperationContext currentOperationContext)
        {
            bool flag3;
            ServiceChannel channel = this.requestInfo.Channel;
            EndpointDispatcher endpoint = this.requestInfo.Endpoint;
            bool flag = false;
            try
            {
                Message requestMessage;
                bool flag2;
                DispatchRuntime dispatchRuntime = this.requestInfo.DispatchRuntime;
                if ((channel == null) || (dispatchRuntime == null))
                {
                    return true;
                }
                MessageBuffer property = null;
                if (dispatchRuntime.PreserveMessage)
                {
                    object obj2 = null;
                    if (request.RequestMessage.Properties.TryGetValue("_RequestMessageBuffer_", out obj2))
                    {
                        property = (MessageBuffer) obj2;
                        requestMessage = property.CreateMessage();
                    }
                    else
                    {
                        property = request.RequestMessage.CreateBufferedCopy(0x7fffffff);
                        requestMessage = property.CreateMessage();
                    }
                }
                else
                {
                    requestMessage = request.RequestMessage;
                }
                DispatchOperationRuntime operation = dispatchRuntime.GetOperation(ref requestMessage);
                if (operation == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No DispatchOperationRuntime found to process message.", new object[0])));
                }
                if (MessageLogger.LoggingEnabled)
                {
                    MessageLogger.LogMessage(ref requestMessage, (operation.IsOneWay ? MessageLoggingSource.ServiceLevelReceiveDatagram : MessageLoggingSource.ServiceLevelReceiveRequest) | MessageLoggingSource.LastChance);
                }
                if (operation.IsTerminating && this.hasSession)
                {
                    this.isChannelTerminated = true;
                }
                if (currentOperationContext != null)
                {
                    flag2 = true;
                    currentOperationContext.ReInit(request, requestMessage, channel);
                }
                else
                {
                    flag2 = false;
                    currentOperationContext = new OperationContext(request, requestMessage, channel, this.host);
                }
                if (dispatchRuntime.PreserveMessage)
                {
                    currentOperationContext.IncomingMessageProperties.Add("_RequestMessageBuffer_", property);
                }
                if ((currentOperationContext.EndpointDispatcher == null) && (this.listener != null))
                {
                    currentOperationContext.EndpointDispatcher = endpoint;
                }
                MessageRpc rpc = new MessageRpc(request, requestMessage, operation, channel, this.host, this, cleanThread, currentOperationContext, this.requestInfo.ExistingInstanceContext);
                TraceUtility.MessageFlowAtMessageReceived(requestMessage, currentOperationContext, true);
                rpc.TransactedBatchContext = this.transactedBatchContext;
                this.requestInfo.ChannelHandlerOwnsCallThrottle = false;
                rpc.MessageRpcOwnsInstanceContextThrottle = this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle;
                this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = false;
                this.ReleasePump();
                flag = true;
                flag3 = operation.Parent.Dispatch(ref rpc, flag2);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                flag3 = this.HandleError(exception, request, channel);
            }
            finally
            {
                if (!flag)
                {
                    this.ReleasePump();
                }
            }
            return flag3;
        }

        internal void DispatchDone()
        {
            if (this.throttle != null)
            {
                this.throttle.DeactivateCall();
            }
        }

        private bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            bool flag = this.receiver.EndTryReceive(result, out requestContext);
            if (flag)
            {
                this.HandleReceiveComplete(requestContext);
            }
            return flag;
        }

        private void EnsureChannelAndEndpoint(RequestContext request)
        {
            this.requestInfo.Channel = this.channel;
            if (this.requestInfo.Channel == null)
            {
                bool flag;
                if (this.hasSession)
                {
                    this.requestInfo.Channel = this.GetSessionChannel(request.RequestMessage, out this.requestInfo.Endpoint, out flag);
                }
                else
                {
                    this.requestInfo.Channel = this.GetDatagramChannel(request.RequestMessage, out this.requestInfo.Endpoint, out flag);
                }
                if (this.requestInfo.Channel == null)
                {
                    this.host.RaiseUnknownMessageReceived(request.RequestMessage);
                    if (flag)
                    {
                        this.ReplyContractFilterDidNotMatch(request);
                    }
                    else
                    {
                        this.ReplyAddressFilterDidNotMatch(request);
                    }
                }
            }
            else
            {
                this.requestInfo.Endpoint = this.requestInfo.Channel.EndpointDispatcher;
                if ((this.InstanceContextServiceThrottle != null) && (this.requestInfo.Channel.InstanceContextServiceThrottle == null))
                {
                    this.requestInfo.Channel.InstanceContextServiceThrottle = this.InstanceContextServiceThrottle;
                }
            }
            this.requestInfo.EndpointLookupDone = true;
            if (this.requestInfo.Channel == null)
            {
                TraceUtility.TraceDroppedMessage(request.RequestMessage, this.requestInfo.Endpoint);
                request.Close();
            }
            else if (this.requestInfo.Channel.HasSession || this.isCallback)
            {
                this.requestInfo.DispatchRuntime = this.requestInfo.Channel.DispatchRuntime;
            }
            else
            {
                this.requestInfo.DispatchRuntime = this.requestInfo.Endpoint.DispatchRuntime;
            }
        }

        private void EnsurePump()
        {
            if ((this.sharedTransactedBatchContext == null) || this.isMainTransactedBatchHandler)
            {
                if (this.TryAcquirePump())
                {
                    if (this.receiveSynchronously)
                    {
                        ActionItem.Schedule(onStartSyncMessagePump, this);
                    }
                    else if (!Thread.CurrentThread.IsThreadPoolThread)
                    {
                        ActionItem.Schedule(onStartAsyncMessagePump, this);
                    }
                    else
                    {
                        IAsyncResult state = this.BeginTryReceive();
                        if ((state != null) && state.CompletedSynchronously)
                        {
                            ActionItem.Schedule(onContinueAsyncReceive, state);
                        }
                    }
                }
            }
            else
            {
                ActionItem.Schedule(onStartSingleTransactedBatch, this);
            }
        }

        private ServiceChannel GetDatagramChannel(Message message, out EndpointDispatcher endpoint, out bool addressMatched)
        {
            addressMatched = false;
            endpoint = this.GetEndpointDispatcher(message, out addressMatched);
            if (endpoint == null)
            {
                return null;
            }
            if (endpoint.DatagramChannel == null)
            {
                lock (this.listener.ThisLock)
                {
                    if (endpoint.DatagramChannel == null)
                    {
                        endpoint.DatagramChannel = new ServiceChannel(this.binder, endpoint, this.listener.ChannelDispatcher, this.idleManager);
                        this.InitializeServiceChannel(endpoint.DatagramChannel);
                    }
                }
            }
            return endpoint.DatagramChannel;
        }

        private EndpointDispatcher GetEndpointDispatcher(Message message, out bool addressMatched)
        {
            return this.listener.Endpoints.Lookup(message, out addressMatched);
        }

        private ServiceChannel GetSessionChannel(Message message, out EndpointDispatcher endpoint, out bool addressMatched)
        {
            addressMatched = false;
            if (this.channel == null)
            {
                lock (this.ThisLock)
                {
                    if (this.channel == null)
                    {
                        endpoint = this.GetEndpointDispatcher(message, out addressMatched);
                        if (endpoint != null)
                        {
                            this.channel = new ServiceChannel(this.binder, endpoint, this.listener.ChannelDispatcher, this.idleManager);
                            this.InitializeServiceChannel(this.channel);
                        }
                    }
                }
            }
            if (this.channel == null)
            {
                endpoint = null;
            }
            else
            {
                endpoint = this.channel.EndpointDispatcher;
            }
            return this.channel;
        }

        internal bool HandleError(Exception e)
        {
            ErrorHandlerFaultInfo faultInfo = new ErrorHandlerFaultInfo();
            return this.HandleError(e, ref faultInfo);
        }

        private bool HandleError(Exception e, ref ErrorHandlerFaultInfo faultInfo)
        {
            if (e == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString(System.ServiceModel.SR.GetString("SFxNonExceptionThrown"))));
            }
            if (this.listener != null)
            {
                return this.listener.ChannelDispatcher.HandleError(e, ref faultInfo);
            }
            return ((this.channel != null) && this.channel.ClientRuntime.CallbackDispatchRuntime.ChannelDispatcher.HandleError(e, ref faultInfo));
        }

        private bool HandleError(Exception e, RequestContext request, ServiceChannel channel)
        {
            bool flag;
            ErrorHandlerFaultInfo faultInfo = new ErrorHandlerFaultInfo(this.messageVersion.Addressing.DefaultFaultAction);
            this.ProvideFaultAndReplyFailure(request, e, ref faultInfo, out flag);
            if (flag)
            {
                try
                {
                    request.Close();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.HandleError(exception);
                }
            }
            else
            {
                request.Abort();
            }
            if (!this.HandleError(e, ref faultInfo) && this.hasSession)
            {
                if (channel != null)
                {
                    if (flag)
                    {
                        TimeoutHelper helper = new TimeoutHelper(CloseAfterFaultTimeout);
                        try
                        {
                            channel.Close(helper.RemainingTime());
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            this.HandleError(exception2);
                        }
                        try
                        {
                            this.binder.CloseAfterFault(helper.RemainingTime());
                            goto Label_0117;
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            this.HandleError(exception3);
                            goto Label_0117;
                        }
                    }
                    channel.Abort();
                    this.binder.Abort();
                }
                else
                {
                    if (flag)
                    {
                        try
                        {
                            this.binder.CloseAfterFault(CloseAfterFaultTimeout);
                            goto Label_0117;
                        }
                        catch (Exception exception4)
                        {
                            if (Fx.IsFatal(exception4))
                            {
                                throw;
                            }
                            this.HandleError(exception4);
                            goto Label_0117;
                        }
                    }
                    this.binder.Abort();
                }
            }
        Label_0117:
            return true;
        }

        private void HandleReceiveComplete(RequestContext context)
        {
            if ((context == null) && this.incrementedActivityCountInConstructor)
            {
                this.listener.ChannelDispatcher.Channels.DecrementActivityCount();
            }
            if (this.channel != null)
            {
                this.channel.HandleReceiveComplete(context);
            }
            else if ((context == null) && this.hasSession)
            {
                bool flag;
                lock (this.ThisLock)
                {
                    flag = !this.doneReceiving;
                    this.doneReceiving = true;
                }
                if (flag)
                {
                    this.receiver.Close();
                    if (this.idleManager != null)
                    {
                        this.idleManager.CancelTimer();
                    }
                    ServiceThrottle throttle = this.throttle;
                    if (throttle != null)
                    {
                        throttle.DeactivateChannel();
                    }
                }
            }
        }

        private bool HandleRequest(RequestContext request, OperationContext currentOperationContext)
        {
            if (request == null)
            {
                return false;
            }
            ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(request.RequestMessage) : null;
            using (ServiceModelActivity.BoundOperation(activity))
            {
                if (this.HandleRequestAsReply(request))
                {
                    this.ReleasePump();
                    return true;
                }
                if (this.isChannelTerminated)
                {
                    this.ReleasePump();
                    this.ReplyChannelTerminated(request);
                    return true;
                }
                RequestContext requestContext = this.requestInfo.RequestContext;
                this.requestInfo.RequestContext = request;
                if (!this.TryAcquireCallThrottle(request))
                {
                    return false;
                }
                bool channelHandlerOwnsCallThrottle = this.requestInfo.ChannelHandlerOwnsCallThrottle;
                this.requestInfo.ChannelHandlerOwnsCallThrottle = true;
                if (!this.TryRetrievingInstanceContext(request))
                {
                    return true;
                }
                this.requestInfo.Channel.CompletedIOOperation();
                if (!this.TryAcquireThrottle(request, this.requestInfo.ExistingInstanceContext == null))
                {
                    return false;
                }
                bool channelHandlerOwnsInstanceContextThrottle = this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle;
                this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = this.requestInfo.ExistingInstanceContext == null;
                if (!this.DispatchAndReleasePump(request, true, currentOperationContext))
                {
                    return false;
                }
            }
            return true;
        }

        private bool HandleRequestAsReply(RequestContext request)
        {
            return ((this.duplexBinder != null) && this.duplexBinder.HandleRequestAsReply(request.RequestMessage));
        }

        private void InitializeServiceChannel(ServiceChannel channel)
        {
            if (this.wasChannelThrottled)
            {
                channel.ServiceThrottle = this.throttle;
            }
            if (this.InstanceContextServiceThrottle != null)
            {
                channel.InstanceContextServiceThrottle = this.InstanceContextServiceThrottle;
            }
            ClientRuntime clientRuntime = channel.ClientRuntime;
            if (clientRuntime != null)
            {
                System.Type contractClientType = clientRuntime.ContractClientType;
                System.Type callbackClientType = clientRuntime.CallbackClientType;
                if (contractClientType != null)
                {
                    channel.Proxy = ServiceChannelFactory.CreateProxy(contractClientType, callbackClientType, MessageDirection.Output, channel);
                }
            }
            if (this.listener != null)
            {
                this.listener.ChannelDispatcher.InitializeChannel((IClientChannel) channel.Proxy);
            }
            channel.Open();
        }

        private static void OnAsyncReceiveComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((ChannelHandler) result.AsyncState).AsyncMessagePump(result);
            }
        }

        private static void OnContinueAsyncReceive(object state)
        {
            IAsyncResult result = (IAsyncResult) state;
            ((ChannelHandler) result.AsyncState).AsyncMessagePump(result);
        }

        private static void OnStartAsyncMessagePump(object state)
        {
            ((ChannelHandler) state).AsyncMessagePump();
        }

        private static void OnStartSingleTransactedBatch(object state)
        {
            (state as ChannelHandler).TransactedBatchLoop();
        }

        private static void OnStartSyncMessagePump(object state)
        {
            ChannelHandler handler = state as ChannelHandler;
            if (handler.receiveWithTransaction)
            {
                handler.SyncTransactionalMessagePump();
            }
            else
            {
                handler.SyncMessagePump();
            }
        }

        private void OpenAndEnsurePump()
        {
            Exception e = null;
            try
            {
                this.binder.Channel.Open();
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                e = exception2;
            }
            if (e != null)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x8003f, System.ServiceModel.SR.GetString("TraceCodeFailedToOpenIncomingChannel"));
                }
                ServiceChannel.SessionIdleManager idleManager = this.idleManager;
                if (idleManager != null)
                {
                    idleManager.CancelTimer();
                }
                if ((this.throttle != null) && this.hasSession)
                {
                    this.throttle.DeactivateChannel();
                }
                bool flag = this.HandleError(e);
                if (this.incrementedActivityCountInConstructor)
                {
                    this.listener.ChannelDispatcher.Channels.DecrementActivityCount();
                }
                if (!flag)
                {
                    this.binder.Channel.Abort();
                }
            }
            else
            {
                this.EnsurePump();
            }
        }

        private static void OpenAndEnsurePump(object state)
        {
            ((ChannelHandler) state).OpenAndEnsurePump();
        }

        private void ProvideFault(Exception e, ref ErrorHandlerFaultInfo faultInfo)
        {
            if (this.listener != null)
            {
                this.listener.ChannelDispatcher.ProvideFault(e, (this.requestInfo.Channel == null) ? this.binder.Channel.GetProperty<FaultConverter>() : this.requestInfo.Channel.GetProperty<FaultConverter>(), ref faultInfo);
            }
            else if (this.channel != null)
            {
                this.channel.ClientRuntime.CallbackDispatchRuntime.ChannelDispatcher.ProvideFault(e, this.channel.GetProperty<FaultConverter>(), ref faultInfo);
            }
        }

        private void ProvideFaultAndReplyFailure(RequestContext request, Exception exception, ref ErrorHandlerFaultInfo faultInfo, out bool replied)
        {
            replied = false;
            bool isFault = false;
            try
            {
                isFault = request.RequestMessage.IsFault;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
            }
            bool enableFaults = false;
            if (this.listener != null)
            {
                enableFaults = this.listener.ChannelDispatcher.EnableFaults;
            }
            else if ((this.channel != null) && this.channel.IsClient)
            {
                enableFaults = this.channel.ClientRuntime.EnableFaults;
            }
            if (!isFault && enableFaults)
            {
                this.ProvideFault(exception, ref faultInfo);
                if (faultInfo.Fault != null)
                {
                    Message fault = faultInfo.Fault;
                    replied = this.TryReply(request, fault);
                    try
                    {
                        fault.Close();
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        this.HandleError(exception3);
                    }
                }
            }
        }

        private void Register()
        {
            this.hasRegisterBeenCalled = true;
            if (this.binder.Channel.State == CommunicationState.Created)
            {
                ActionItem.Schedule(openAndEnsurePump, this);
            }
            else
            {
                this.EnsurePump();
            }
        }

        internal static void Register(ChannelHandler handler)
        {
            handler.Register();
        }

        internal static void Register(ChannelHandler handler, RequestContext request)
        {
            (handler.Binder as BufferedReceiveBinder).InjectRequest(request);
            handler.Register();
        }

        private void ReleasePump()
        {
            if (this.isConcurrent)
            {
                this.isPumpAcquired = 0;
            }
        }

        private bool Reply(RequestContext request, Message reply)
        {
            if (this.replied != request)
            {
                this.replied = request;
                bool flag = true;
                Message objA = null;
                try
                {
                    objA = request.RequestMessage;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
                if (!object.ReferenceEquals(objA, null))
                {
                    UniqueId messageId = null;
                    try
                    {
                        messageId = objA.Headers.MessageId;
                    }
                    catch (MessageHeaderException)
                    {
                    }
                    if (!object.ReferenceEquals(messageId, null) && !this.isManualAddressing)
                    {
                        RequestReplyCorrelator.PrepareReply(reply, messageId);
                    }
                    if (!this.hasSession && !this.isManualAddressing)
                    {
                        try
                        {
                            flag = RequestReplyCorrelator.AddressReply(reply, objA);
                        }
                        catch (MessageHeaderException)
                        {
                        }
                    }
                }
                if (this.IsOpen && flag)
                {
                    request.Reply(reply);
                    return true;
                }
            }
            return false;
        }

        private void ReplyAddressFilterDidNotMatch(RequestContext request)
        {
            FaultCode code = FaultCode.CreateSenderFaultCode("DestinationUnreachable", this.messageVersion.Addressing.Namespace);
            string reason = System.ServiceModel.SR.GetString("SFxNoEndpointMatchingAddress", new object[] { request.RequestMessage.Headers.To });
            this.ReplyFailure(request, code, reason);
        }

        private void ReplyChannelTerminated(RequestContext request)
        {
            FaultCode faultCode = FaultCode.CreateSenderFaultCode("SessionTerminated", "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher");
            string reason = System.ServiceModel.SR.GetString("SFxChannelTerminated0");
            string action = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault";
            Message fault = Message.CreateMessage(this.messageVersion, faultCode, reason, action);
            this.ReplyFailure(request, fault, action, reason, faultCode);
        }

        private void ReplyContractFilterDidNotMatch(RequestContext request)
        {
            AddressingVersion addressing = this.messageVersion.Addressing;
            if ((addressing != AddressingVersion.None) && (request.RequestMessage.Headers.Action == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("SFxMissingActionHeader", new object[] { addressing.Namespace }), "Action", addressing.Namespace));
            }
            FaultCode code = FaultCode.CreateSenderFaultCode("ActionNotSupported", this.messageVersion.Addressing.Namespace);
            string reason = System.ServiceModel.SR.GetString("SFxNoEndpointMatchingContract", new object[] { request.RequestMessage.Headers.Action });
            this.ReplyFailure(request, code, reason, this.messageVersion.Addressing.FaultAction);
        }

        private void ReplyFailure(RequestContext request, FaultCode code, string reason)
        {
            string defaultFaultAction = this.messageVersion.Addressing.DefaultFaultAction;
            this.ReplyFailure(request, code, reason, defaultFaultAction);
        }

        private void ReplyFailure(RequestContext request, FaultCode code, string reason, string action)
        {
            Message fault = Message.CreateMessage(this.messageVersion, code, reason, action);
            this.ReplyFailure(request, fault, action, reason, code);
        }

        private void ReplyFailure(RequestContext request, Message fault, string action, string reason, FaultCode code)
        {
            bool flag;
            FaultException e = new FaultException(reason, code);
            System.ServiceModel.Dispatcher.ErrorBehavior.ThrowAndCatch(e);
            ErrorHandlerFaultInfo faultInfo = new ErrorHandlerFaultInfo(action) {
                Fault = fault
            };
            this.ProvideFaultAndReplyFailure(request, e, ref faultInfo, out flag);
            this.HandleError(e, ref faultInfo);
        }

        private void SyncMessagePump()
        {
            OperationContext current = OperationContext.Current;
            try
            {
                RequestContext context3;
                OperationContext currentOperationContext = new OperationContext(this.host);
                OperationContext.Current = currentOperationContext;
            Label_0018:
                this.requestInfo.Cleanup();
                while (!this.TryReceive(TimeSpan.MaxValue, out context3))
                {
                }
                if (this.HandleRequest(context3, currentOperationContext) && this.TryAcquirePump())
                {
                    currentOperationContext.Recycle();
                    goto Label_0018;
                }
            }
            finally
            {
                OperationContext.Current = current;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SyncTransactionalMessagePump()
        {
            bool flag;
            do
            {
                if (this.sharedTransactedBatchContext == null)
                {
                    flag = this.TransactedLoop();
                }
                else
                {
                    flag = this.TransactedBatchLoop();
                }
            }
            while (flag);
        }

        internal void ThrottleAcquired()
        {
            RequestContext requestWaitingForThrottle = this.requestWaitingForThrottle;
            this.requestWaitingForThrottle = null;
            bool channelHandlerOwnsInstanceContextThrottle = this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle;
            this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = this.requestInfo.ExistingInstanceContext == null;
            if (this.DispatchAndReleasePump(requestWaitingForThrottle, false, null))
            {
                this.EnsurePump();
            }
        }

        internal void ThrottleAcquiredForCall()
        {
            RequestContext requestWaitingForThrottle = this.requestWaitingForThrottle;
            this.requestWaitingForThrottle = null;
            bool channelHandlerOwnsCallThrottle = this.requestInfo.ChannelHandlerOwnsCallThrottle;
            this.requestInfo.ChannelHandlerOwnsCallThrottle = true;
            if (!this.TryRetrievingInstanceContext(requestWaitingForThrottle))
            {
                this.EnsurePump();
            }
            else
            {
                this.requestInfo.Channel.CompletedIOOperation();
                if (this.TryAcquireThrottle(requestWaitingForThrottle, this.requestInfo.ExistingInstanceContext == null))
                {
                    bool channelHandlerOwnsInstanceContextThrottle = this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle;
                    this.requestInfo.ChannelHandlerOwnsInstanceContextThrottle = this.requestInfo.ExistingInstanceContext == null;
                    if (this.DispatchAndReleasePump(requestWaitingForThrottle, false, null))
                    {
                        this.EnsurePump();
                    }
                }
            }
        }

        private bool TransactedBatchLoop()
        {
            if (this.transactedBatchContext != null)
            {
                if (this.transactedBatchContext.InDispatch)
                {
                    this.transactedBatchContext.ForceRollback();
                    this.transactedBatchContext.InDispatch = false;
                }
                if (!this.transactedBatchContext.IsActive)
                {
                    if (!this.isMainTransactedBatchHandler)
                    {
                        return false;
                    }
                    this.transactedBatchContext = null;
                }
            }
            if (this.transactedBatchContext == null)
            {
                try
                {
                    this.receiver.WaitForMessage();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!this.HandleError(exception))
                    {
                        throw;
                    }
                }
                this.transactedBatchContext = this.sharedTransactedBatchContext.CreateTransactedBatchContext();
            }
            OperationContext current = OperationContext.Current;
            try
            {
                OperationContext currentOperationContext = new OperationContext(this.host);
                OperationContext.Current = currentOperationContext;
                while (this.transactedBatchContext.IsActive)
                {
                    RequestContext context3;
                    this.requestInfo.Cleanup();
                    if (!this.TryTransactionalReceive(this.transactedBatchContext.Transaction, out context3))
                    {
                        if (this.IsOpen)
                        {
                            this.transactedBatchContext.ForceCommit();
                            return true;
                        }
                        this.transactedBatchContext.ForceRollback();
                        return false;
                    }
                    if (context3 == null)
                    {
                        this.transactedBatchContext.ForceRollback();
                        return false;
                    }
                    TransactionMessageProperty.Set(this.transactedBatchContext.Transaction, context3.RequestMessage);
                    this.transactedBatchContext.InDispatch = true;
                    if (!this.HandleRequest(context3, currentOperationContext))
                    {
                        return false;
                    }
                    if (this.transactedBatchContext.InDispatch)
                    {
                        this.transactedBatchContext.ForceRollback();
                        this.transactedBatchContext.InDispatch = false;
                        return true;
                    }
                    if (!this.TryAcquirePump())
                    {
                        return false;
                    }
                    currentOperationContext.Recycle();
                }
            }
            finally
            {
                OperationContext.Current = current;
            }
            return true;
        }

        private bool TransactedLoop()
        {
            bool flag2;
            try
            {
                this.receiver.WaitForMessage();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!this.HandleError(exception))
                {
                    throw;
                }
            }
            Transaction tx = this.CreateOrGetAttachedTransaction();
            OperationContext current = OperationContext.Current;
            try
            {
                RequestContext context;
                OperationContext currentOperationContext = new OperationContext(this.host);
                OperationContext.Current = currentOperationContext;
            Label_0046:
                this.requestInfo.Cleanup();
                if (!this.TryTransactionalReceive(tx, out context))
                {
                    return this.IsOpen;
                }
                if (context == null)
                {
                    return false;
                }
                TransactionMessageProperty.Set(tx, context.RequestMessage);
                if (!this.HandleRequest(context, currentOperationContext))
                {
                    return false;
                }
                if (!this.TryAcquirePump())
                {
                    flag2 = false;
                }
                else
                {
                    tx = this.CreateOrGetAttachedTransaction();
                    currentOperationContext.Recycle();
                    goto Label_0046;
                }
            }
            finally
            {
                OperationContext.Current = current;
            }
            return flag2;
        }

        private bool TryAcquireCallThrottle(RequestContext request)
        {
            ServiceThrottle throttle = this.throttle;
            if ((throttle == null) || !throttle.IsActive)
            {
                return true;
            }
            this.requestWaitingForThrottle = request;
            if (throttle.AcquireCall(this))
            {
                this.requestWaitingForThrottle = null;
                return true;
            }
            return false;
        }

        private bool TryAcquirePump()
        {
            return (!this.isConcurrent || ((this.isPumpAcquired == 0) && (Interlocked.CompareExchange(ref this.isPumpAcquired, 1, 0) == 0)));
        }

        private bool TryAcquireThrottle(RequestContext request, bool acquireInstanceContextThrottle)
        {
            ServiceThrottle throttle = this.throttle;
            if ((throttle == null) || !throttle.IsActive)
            {
                return true;
            }
            this.requestWaitingForThrottle = request;
            if (throttle.AcquireInstanceContextAndDynamic(this, acquireInstanceContextThrottle))
            {
                this.requestWaitingForThrottle = null;
                return true;
            }
            return false;
        }

        private bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            bool flag = this.receiver.TryReceive(timeout, out requestContext);
            if (flag)
            {
                this.HandleReceiveComplete(requestContext);
            }
            return flag;
        }

        private bool TryReply(RequestContext request, Message reply)
        {
            try
            {
                return this.Reply(request, reply);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.HandleError(exception);
            }
            return false;
        }

        private bool TryRetrievingInstanceContext(RequestContext request)
        {
            bool flag = true;
            try
            {
                if (!this.requestInfo.EndpointLookupDone)
                {
                    this.EnsureChannelAndEndpoint(request);
                }
                if (this.requestInfo.Channel != null)
                {
                    if (this.requestInfo.DispatchRuntime != null)
                    {
                        IContextChannel proxy = this.requestInfo.Channel.Proxy as IContextChannel;
                        try
                        {
                            this.requestInfo.ExistingInstanceContext = this.requestInfo.DispatchRuntime.InstanceContextProvider.GetExistingInstanceContext(request.RequestMessage, proxy);
                            flag = false;
                            goto Label_00F2;
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            this.requestInfo.Channel = null;
                            this.HandleError(exception, request, this.channel);
                            return false;
                        }
                    }
                    TraceUtility.TraceDroppedMessage(request.RequestMessage, this.requestInfo.Endpoint);
                    request.Close();
                }
                return false;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.HandleError(exception2, request, this.channel);
                return false;
            }
            finally
            {
                if (flag)
                {
                    this.ReleasePump();
                }
            }
        Label_00F2:
            return true;
        }

        private bool TryTransactionalReceive(Transaction tx, out RequestContext request)
        {
            request = null;
            bool flag = false;
            try
            {
                using (TransactionScope scope = new TransactionScope(tx))
                {
                    if (this.sharedTransactedBatchContext != null)
                    {
                        lock (this.sharedTransactedBatchContext.ReceiveLock)
                        {
                            if (this.transactedBatchContext.AboutToExpire)
                            {
                                return false;
                            }
                            flag = this.receiver.TryReceive(TimeSpan.Zero, out request);
                            goto Label_009D;
                        }
                    }
                    TimeSpan timeout = TimeoutHelper.Min(this.listener.ChannelDispatcher.TransactionTimeout, this.listener.ChannelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
                    flag = this.receiver.TryReceive(TransactionBehavior.NormalizeTimeout(timeout), out request);
                Label_009D:
                    scope.Complete();
                }
                if (flag)
                {
                    this.HandleReceiveComplete(request);
                }
            }
            catch (ObjectDisposedException exception)
            {
                this.HandleError(exception);
                request = null;
                return false;
            }
            catch (TransactionException exception2)
            {
                this.HandleError(exception2);
                request = null;
                return false;
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (!this.HandleError(exception3))
                {
                    throw;
                }
            }
            return flag;
        }

        internal IChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        internal ServiceChannel Channel
        {
            get
            {
                return this.channel;
            }
        }

        internal bool HasRegisterBeenCalled
        {
            get
            {
                return this.hasRegisterBeenCalled;
            }
        }

        internal System.ServiceModel.InstanceContext InstanceContext
        {
            get
            {
                if (this.channel == null)
                {
                    return null;
                }
                return this.channel.InstanceContext;
            }
        }

        internal ServiceThrottle InstanceContextServiceThrottle
        {
            get
            {
                return this.instanceContextThrottle;
            }
            set
            {
                this.instanceContextThrottle = value;
            }
        }

        private bool IsOpen
        {
            get
            {
                return (this.binder.Channel.State == CommunicationState.Opened);
            }
        }

        private EndpointAddress LocalAddress
        {
            get
            {
                if (this.binder != null)
                {
                    IInputChannel channel = this.binder.Channel as IInputChannel;
                    if (channel != null)
                    {
                        return channel.LocalAddress;
                    }
                    IReplyChannel channel2 = this.binder.Channel as IReplyChannel;
                    if (channel2 != null)
                    {
                        return channel2.LocalAddress;
                    }
                }
                return null;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RequestInfo
        {
            public EndpointDispatcher Endpoint;
            public InstanceContext ExistingInstanceContext;
            public ServiceChannel Channel;
            public bool EndpointLookupDone;
            public System.ServiceModel.Dispatcher.DispatchRuntime DispatchRuntime;
            public System.ServiceModel.Channels.RequestContext RequestContext;
            public System.ServiceModel.Dispatcher.ChannelHandler ChannelHandler;
            public bool ChannelHandlerOwnsCallThrottle;
            public bool ChannelHandlerOwnsInstanceContextThrottle;
            public RequestInfo(System.ServiceModel.Dispatcher.ChannelHandler channelHandler)
            {
                this.Endpoint = null;
                this.ExistingInstanceContext = null;
                this.Channel = null;
                this.EndpointLookupDone = false;
                this.DispatchRuntime = null;
                this.RequestContext = null;
                this.ChannelHandler = channelHandler;
                this.ChannelHandlerOwnsCallThrottle = false;
                this.ChannelHandlerOwnsInstanceContextThrottle = false;
            }

            public void Cleanup()
            {
                if (this.ChannelHandlerOwnsInstanceContextThrottle)
                {
                    this.ChannelHandler.throttle.DeactivateInstanceContext();
                    this.ChannelHandlerOwnsInstanceContextThrottle = false;
                }
                this.Endpoint = null;
                this.ExistingInstanceContext = null;
                this.Channel = null;
                this.EndpointLookupDone = false;
                this.RequestContext = null;
                if (this.ChannelHandlerOwnsCallThrottle)
                {
                    this.ChannelHandler.DispatchDone();
                    this.ChannelHandlerOwnsCallThrottle = false;
                }
            }
        }
    }
}

