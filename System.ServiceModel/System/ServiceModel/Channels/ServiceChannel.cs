namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;

    internal sealed class ServiceChannel : CommunicationObject, IClientChannel, IDisposable, IDuplexContextChannel, IOutputChannel, IRequestChannel, IServiceChannel, IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>
    {
        private int activityCount;
        private bool allowInitializationUI;
        private bool allowOutputBatching;
        private bool autoClose;
        private CallOnceManager autoDisplayUIManager;
        private CallOnceManager autoOpenManager;
        private readonly IChannelBinder binder;
        private readonly System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher;
        private System.ServiceModel.Dispatcher.ClientRuntime clientRuntime;
        private readonly bool closeBinder;
        private bool closeFactory;
        private bool didInteractiveInitialization;
        private bool doneReceiving;
        private System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher;
        private bool explicitlyOpened;
        private ExtensionCollection<IContextChannel> extensions;
        private readonly ServiceChannelFactory factory;
        private bool hasChannelStartedAutoClosing;
        private bool hasCleanedUpChannelCollections;
        private bool hasIncrementedBusyCount;
        private readonly bool hasSession;
        private readonly SessionIdleManager idleManager;
        private System.ServiceModel.InstanceContext instanceContext;
        private System.ServiceModel.Dispatcher.ServiceThrottle instanceContextServiceThrottle;
        private bool isPending;
        private readonly bool isReplyChannel;
        private EndpointAddress localAddress;
        private readonly System.ServiceModel.Channels.MessageVersion messageVersion;
        private readonly bool openBinder;
        private TimeSpan operationTimeout;
        private object proxy;
        private System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle;
        private string terminatingOperationName;
        private EventHandler<UnknownMessageReceivedEventArgs> unknownMessageReceived;
        private System.ServiceModel.InstanceContext wmiInstanceContext;

        event EventHandler<UnknownMessageReceivedEventArgs> IClientChannel.UnknownMessageReceived
        {
            add
            {
                lock (base.ThisLock)
                {
                    this.unknownMessageReceived = (EventHandler<UnknownMessageReceivedEventArgs>) Delegate.Combine(this.unknownMessageReceived, value);
                }
            }
            remove
            {
                lock (base.ThisLock)
                {
                    this.unknownMessageReceived = (EventHandler<UnknownMessageReceivedEventArgs>) Delegate.Remove(this.unknownMessageReceived, value);
                }
            }
        }

        internal ServiceChannel(ServiceChannelFactory factory, IChannelBinder binder) : this(binder, factory.MessageVersion, factory)
        {
            this.factory = factory;
            this.clientRuntime = factory.ClientRuntime;
            System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime = factory.ClientRuntime.DispatchRuntime;
            if (dispatchRuntime != null)
            {
                this.autoClose = dispatchRuntime.AutomaticInputSessionShutdown;
            }
            factory.ChannelCreated(this);
        }

        private ServiceChannel(IChannelBinder binder, System.ServiceModel.Channels.MessageVersion messageVersion, IDefaultCommunicationTimeouts timeouts)
        {
            this.allowInitializationUI = true;
            this.autoClose = true;
            this.closeBinder = true;
            if (binder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binder");
            }
            this.messageVersion = messageVersion;
            this.binder = binder;
            this.isReplyChannel = this.binder.Channel is IReplyChannel;
            IChannel channel = binder.Channel;
            this.hasSession = ((channel is ISessionChannel<IDuplexSession>) || (channel is ISessionChannel<IInputSession>)) || (channel is ISessionChannel<IOutputSession>);
            this.binder.Channel.Faulted += new EventHandler(this.OnInnerChannelFaulted);
            this.IncrementActivity();
            this.openBinder = binder.Channel.State == CommunicationState.Created;
            this.operationTimeout = timeouts.SendTimeout;
        }

        internal ServiceChannel(IChannelBinder binder, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher, System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher, SessionIdleManager idleManager) : this(binder, channelDispatcher.MessageVersion, channelDispatcher.DefaultCommunicationTimeouts)
        {
            if (endpointDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointDispatcher");
            }
            this.channelDispatcher = channelDispatcher;
            this.endpointDispatcher = endpointDispatcher;
            this.clientRuntime = endpointDispatcher.DispatchRuntime.CallbackClientRuntime;
            this.autoClose = endpointDispatcher.DispatchRuntime.AutomaticInputSessionShutdown;
            this.isPending = true;
            IDefaultCommunicationTimeouts defaultCommunicationTimeouts = channelDispatcher.DefaultCommunicationTimeouts;
            this.idleManager = idleManager;
            if (!binder.HasSession)
            {
                this.closeBinder = false;
            }
            if (this.idleManager != null)
            {
                bool flag;
                this.idleManager.RegisterChannel(this, out flag);
                if (flag)
                {
                    base.Abort();
                }
            }
        }

        private void AddMessageProperties(Message message, OperationContext context)
        {
            if (this.allowOutputBatching)
            {
                message.Properties.AllowOutputBatching = true;
            }
            if ((context != null) && (context.InternalServiceChannel == this))
            {
                if (!context.OutgoingMessageVersion.IsMatch(message.Headers.MessageVersion))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxVersionMismatchInOperationContextAndMessage2", new object[] { context.OutgoingMessageVersion, message.Headers.MessageVersion })));
                }
                if (context.HasOutgoingMessageHeaders)
                {
                    message.Headers.CopyHeadersFrom(context.OutgoingMessageHeaders);
                }
                if (context.HasOutgoingMessageProperties)
                {
                    message.Properties.CopyProperties(context.OutgoingMessageProperties);
                }
            }
        }

        internal IAsyncResult BeginCall(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, AsyncCallback callback, object asyncState)
        {
            return this.BeginCall(action, oneway, operation, ins, this.operationTimeout, callback, asyncState);
        }

        internal IAsyncResult BeginCall(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, TimeSpan timeout, AsyncCallback callback, object asyncState)
        {
            SendAsyncResult result;
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfIdleAborted(operation);
            ServiceModelActivity activity = null;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                activity = ServiceModelActivity.CreateActivity(true);
                callback = TraceUtility.WrapExecuteUserCodeAsyncCallback(callback);
            }
            using (ServiceModelActivity.BoundOperation(activity, true))
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityProcessAction", new object[] { action }), ActivityType.ProcessAction);
                }
                result = new SendAsyncResult(this, operation, action, ins, oneway, timeout, callback, asyncState);
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    result.Rpc.Activity = activity;
                }
                result.Begin();
            }
            return result;
        }

        public IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state)
        {
            this.ThrowIfDisallowedInitializationUI();
            if (this.autoDisplayUIManager == null)
            {
                this.explicitlyOpened = true;
            }
            return this.ClientRuntime.GetRuntime().BeginDisplayInitializationUI(this, callback, state);
        }

        private IAsyncResult BeginEnsureDisplayUI(AsyncCallback callback, object state)
        {
            CallOnceManager autoDisplayUIManager = this.AutoDisplayUIManager;
            if (autoDisplayUIManager != null)
            {
                return autoDisplayUIManager.BeginCallOnce(TimeSpan.MaxValue, null, callback, state);
            }
            return new CallOnceCompletedAsyncResult(callback, state);
        }

        private IAsyncResult BeginEnsureOpened(TimeSpan timeout, AsyncCallback callback, object state)
        {
            CallOnceManager autoOpenManager = this.AutoOpenManager;
            if (autoOpenManager != null)
            {
                return autoOpenManager.BeginCallOnce(timeout, this.autoDisplayUIManager, callback, state);
            }
            this.ThrowIfOpening();
            base.ThrowIfDisposedOrNotOpen();
            return new CallOnceCompletedAsyncResult(callback, state);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, this.OperationTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ProxyOperationRuntime unhandledProxyOperation = this.UnhandledProxyOperation;
            return this.BeginCall(message.Headers.Action, false, unhandledProxyOperation, new object[] { message }, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.OperationTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ProxyOperationRuntime unhandledProxyOperation = this.UnhandledProxyOperation;
            return this.BeginCall(message.Headers.Action, true, unhandledProxyOperation, new object[] { message }, timeout, callback, state);
        }

        private void BindDuplexCallbacks()
        {
            if (((this.InnerChannel is IDuplexChannel) && (this.factory != null)) && ((this.instanceContext != null) && (this.binder is DuplexChannelBinder)))
            {
                ((DuplexChannelBinder) this.binder).EnsurePumping();
            }
        }

        internal object Call(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, object[] outs)
        {
            return this.Call(action, oneway, operation, ins, outs, this.operationTimeout);
        }

        internal object Call(string action, bool oneway, ProxyOperationRuntime operation, object[] ins, object[] outs, TimeSpan timeout)
        {
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfIdleAborted(operation);
            ProxyRpc rpc = new ProxyRpc(this, operation, action, ins, timeout);
            using (rpc.Activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(rpc.Activity, System.ServiceModel.SR.GetString("ActivityProcessAction", new object[] { action }), ActivityType.ProcessAction);
                }
                this.PrepareCall(operation, oneway, ref rpc);
                if (!this.explicitlyOpened)
                {
                    this.EnsureDisplayUI();
                    this.EnsureOpened(rpc.TimeoutHelper.RemainingTime());
                }
                else
                {
                    this.ThrowIfOpening();
                    base.ThrowIfDisposedOrNotOpen();
                }
                try
                {
                    ConcurrencyBehavior.UnlockInstanceBeforeCallout(OperationContext.Current);
                    if (oneway)
                    {
                        this.binder.Send(rpc.Request, rpc.TimeoutHelper.RemainingTime());
                    }
                    else
                    {
                        rpc.Reply = this.binder.Request(rpc.Request, rpc.TimeoutHelper.RemainingTime());
                        if (rpc.Reply == null)
                        {
                            base.ThrowIfFaulted();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxServerDidNotReply")));
                        }
                    }
                }
                finally
                {
                    this.CompletedIOOperation();
                    CallOnceManager.SignalNextIfNonNull(this.autoOpenManager);
                    ConcurrencyBehavior.LockInstanceAfterCallout(OperationContext.Current);
                }
                rpc.OutputParameters = outs;
                this.HandleReply(operation, ref rpc);
            }
            return rpc.ReturnValue;
        }

        internal bool CanCastTo(System.Type t)
        {
            if (t.IsAssignableFrom(typeof(IClientChannel)))
            {
                return true;
            }
            if (t.IsAssignableFrom(typeof(IDuplexContextChannel)))
            {
                return (this.InnerChannel is IDuplexChannel);
            }
            return t.IsAssignableFrom(typeof(IServiceChannel));
        }

        private void CleanupChannelCollections()
        {
            if (!this.hasCleanedUpChannelCollections)
            {
                lock (base.ThisLock)
                {
                    if (!this.hasCleanedUpChannelCollections)
                    {
                        if (this.InstanceContext != null)
                        {
                            this.InstanceContext.OutgoingChannels.Remove((IChannel) this.proxy);
                        }
                        if (this.WmiInstanceContext != null)
                        {
                            this.WmiInstanceContext.WmiChannels.Remove((IChannel) this.proxy);
                        }
                        this.hasCleanedUpChannelCollections = true;
                    }
                }
            }
        }

        internal void CompletedIOOperation()
        {
            if (this.idleManager != null)
            {
                this.idleManager.CompletedActivity();
            }
        }

        internal void DecrementActivity()
        {
            int num = Interlocked.Decrement(ref this.activityCount);
            if (num < 0)
            {
                throw Fx.AssertAndThrowFatal("ServiceChannel.DecrementActivity: (updatedActivityCount >= 0)");
            }
            if ((num == 0) && this.autoClose)
            {
                try
                {
                    if (base.State == CommunicationState.Opened)
                    {
                        if (this.IsClient)
                        {
                            ISessionChannel<IDuplexSession> innerChannel = this.InnerChannel as ISessionChannel<IDuplexSession>;
                            if (innerChannel != null)
                            {
                                this.hasChannelStartedAutoClosing = true;
                                innerChannel.Session.CloseOutputSession(this.CloseTimeout);
                            }
                        }
                        else
                        {
                            base.Close(this.CloseTimeout);
                        }
                    }
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (ObjectDisposedException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                catch (InvalidOperationException exception4)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                }
            }
        }

        private void DecrementBusyCount()
        {
            lock (base.ThisLock)
            {
                if (this.hasIncrementedBusyCount)
                {
                    AspNetEnvironment.Current.DecrementBusyCount();
                    if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceDecrementBusyCount(base.GetType().FullName);
                    }
                    this.hasIncrementedBusyCount = false;
                }
            }
        }

        public void DisplayInitializationUI()
        {
            this.ThrowIfDisallowedInitializationUI();
            if (this.autoDisplayUIManager == null)
            {
                this.explicitlyOpened = true;
            }
            this.ClientRuntime.GetRuntime().DisplayInitializationUI(this);
            this.didInteractiveInitialization = true;
        }

        internal object EndCall(string action, object[] outs, IAsyncResult result)
        {
            object returnValue;
            SendAsyncResult result2 = result as SendAsyncResult;
            if (result2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxInvalidCallbackIAsyncResult")));
            }
            using (ServiceModelActivity activity = result2.Rpc.Activity)
            {
                using (ServiceModelActivity.BoundOperation(activity, true))
                {
                    if ((result2.Rpc.Activity != null) && DiagnosticUtility.ShouldUseActivity)
                    {
                        result2.Rpc.Activity.Resume();
                    }
                    if (result2.Rpc.Channel != this)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("AsyncEndCalledOnWrongChannel"));
                    }
                    if ((action != "*") && (action != result2.Rpc.Action))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("AsyncEndCalledWithAnIAsyncResult"));
                    }
                    SendAsyncResult.End(result2);
                    result2.Rpc.OutputParameters = outs;
                    this.HandleReply(result2.Rpc.Operation, ref result2.Rpc);
                    if (result2.Rpc.Activity != null)
                    {
                        result2.Rpc.Activity = null;
                    }
                    returnValue = result2.Rpc.ReturnValue;
                }
            }
            return returnValue;
        }

        public void EndDisplayInitializationUI(IAsyncResult result)
        {
            this.ClientRuntime.GetRuntime().EndDisplayInitializationUI(result);
            this.didInteractiveInitialization = true;
        }

        private void EndEnsureDisplayUI(IAsyncResult result)
        {
            CallOnceManager autoDisplayUIManager = this.AutoDisplayUIManager;
            if (autoDisplayUIManager != null)
            {
                autoDisplayUIManager.EndCallOnce(result);
            }
            else
            {
                CallOnceCompletedAsyncResult.End(result);
            }
            this.ThrowIfInitializationUINotCalled();
        }

        private void EndEnsureOpened(IAsyncResult result)
        {
            CallOnceManager autoOpenManager = this.AutoOpenManager;
            if (autoOpenManager != null)
            {
                autoOpenManager.EndCallOnce(result);
            }
            else
            {
                CallOnceCompletedAsyncResult.End(result);
            }
        }

        public Message EndRequest(IAsyncResult result)
        {
            return (Message) this.EndCall("*", EmptyArray<object>.Instance, result);
        }

        public void EndSend(IAsyncResult result)
        {
            this.EndCall("*", EmptyArray<object>.Instance, result);
        }

        private void EnsureAutoOpenManagers()
        {
            lock (base.ThisLock)
            {
                if (!this.explicitlyOpened)
                {
                    if (this.autoOpenManager == null)
                    {
                        this.autoOpenManager = new CallOnceManager(this, CallOpenOnce.Instance);
                    }
                    if (this.autoDisplayUIManager == null)
                    {
                        this.autoDisplayUIManager = new CallOnceManager(this, CallDisplayUIOnce.Instance);
                    }
                }
            }
        }

        private void EnsureDisplayUI()
        {
            CallOnceManager autoDisplayUIManager = this.AutoDisplayUIManager;
            if (autoDisplayUIManager != null)
            {
                autoDisplayUIManager.CallOnce(TimeSpan.MaxValue, null);
            }
            this.ThrowIfInitializationUINotCalled();
        }

        private void EnsureOpened(TimeSpan timeout)
        {
            CallOnceManager autoOpenManager = this.AutoOpenManager;
            if (autoOpenManager != null)
            {
                autoOpenManager.CallOnce(timeout, this.autoDisplayUIManager);
            }
            this.ThrowIfOpening();
            base.ThrowIfDisposedOrNotOpen();
        }

        internal void FireUnknownMessageReceived(Message message)
        {
            EventHandler<UnknownMessageReceivedEventArgs> unknownMessageReceived = this.unknownMessageReceived;
            if (unknownMessageReceived != null)
            {
                unknownMessageReceived(this.proxy, new UnknownMessageReceivedEventArgs(message));
            }
        }

        private IDuplexSession GetDuplexSessionOrThrow()
        {
            if (this.InnerChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("channelIsNotAvailable0")));
            }
            ISessionChannel<IDuplexSession> innerChannel = this.InnerChannel as ISessionChannel<IDuplexSession>;
            if (innerChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("channelDoesNotHaveADuplexSession0")));
            }
            return innerChannel.Session;
        }

        private TimeoutException GetOpenTimeoutException(TimeSpan timeout)
        {
            EndpointAddress address = this.RemoteAddress ?? this.LocalAddress;
            if (address != null)
            {
                return new TimeoutException(System.ServiceModel.SR.GetString("TimeoutServiceChannelConcurrentOpen2", new object[] { address, timeout }));
            }
            return new TimeoutException(System.ServiceModel.SR.GetString("TimeoutServiceChannelConcurrentOpen1", new object[] { timeout }));
        }

        public T GetProperty<T>() where T: class
        {
            IChannel innerChannel = this.InnerChannel;
            if (innerChannel != null)
            {
                return innerChannel.GetProperty<T>();
            }
            return default(T);
        }

        internal void HandleReceiveComplete(RequestContext context)
        {
            if ((context == null) && this.HasSession)
            {
                bool flag;
                lock (base.ThisLock)
                {
                    flag = !this.doneReceiving;
                    this.doneReceiving = true;
                }
                if (flag)
                {
                    System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime = this.ClientRuntime.DispatchRuntime;
                    if (dispatchRuntime != null)
                    {
                        dispatchRuntime.GetRuntime().InputSessionDoneReceiving(this);
                    }
                    this.DecrementActivity();
                }
            }
        }

        private void HandleReply(ProxyOperationRuntime operation, ref ProxyRpc rpc)
        {
            try
            {
                if (TraceUtility.MessageFlowTracingOnly && (rpc.ActivityId != Guid.Empty))
                {
                    System.Runtime.Diagnostics.DiagnosticTrace.ActivityId = rpc.ActivityId;
                }
                if (System.ServiceModel.Diagnostics.Application.TD.ClientOperationCompletedIsEnabled())
                {
                    string destination = string.Empty;
                    if ((this.RemoteAddress != null) && (this.RemoteAddress.Uri != null))
                    {
                        destination = this.RemoteAddress.Uri.AbsoluteUri;
                    }
                    System.ServiceModel.Diagnostics.Application.TD.ClientOperationCompleted(rpc.Action, this.clientRuntime.ContractName, destination);
                }
                if (rpc.Reply != null)
                {
                    TraceUtility.MessageFlowAtMessageReceived(rpc.Reply, null, false);
                    if (MessageLogger.LogMessagesAtServiceLevel)
                    {
                        MessageLogger.LogMessage(ref rpc.Reply, MessageLoggingSource.LastChance | MessageLoggingSource.ServiceLevelReceiveReply);
                    }
                    operation.Parent.AfterReceiveReply(ref rpc);
                    if (((operation.ReplyAction != "*") && !rpc.Reply.IsFault) && ((rpc.Reply.Headers.Action != null) && (string.CompareOrdinal(operation.ReplyAction, rpc.Reply.Headers.Action) != 0)))
                    {
                        Exception exception = new ProtocolException(System.ServiceModel.SR.GetString("SFxReplyActionMismatch3", new object[] { operation.Name, rpc.Reply.Headers.Action, operation.ReplyAction }));
                        this.TerminateIfNecessary(ref rpc);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                    if (operation.DeserializeReply && this.clientRuntime.IsFault(ref rpc.Reply))
                    {
                        MessageFault fault = MessageFault.CreateFault(rpc.Reply, this.clientRuntime.MaxFaultSize);
                        string action = rpc.Reply.Headers.Action;
                        if (action == rpc.Reply.Version.Addressing.DefaultFaultAction)
                        {
                            action = null;
                        }
                        this.ThrowIfFaultUnderstood(rpc.Reply, fault, action, rpc.Reply.Version, rpc.Channel.GetProperty<FaultConverter>());
                        FaultException exception2 = rpc.Operation.FaultFormatter.Deserialize(fault, action);
                        this.TerminateIfNecessary(ref rpc);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(exception2);
                    }
                    operation.AfterReply(ref rpc);
                }
            }
            finally
            {
                if (operation.SerializeRequest)
                {
                    rpc.Request.Close();
                }
                OperationContext current = OperationContext.Current;
                bool closeMessage = (rpc.Reply != null) && (rpc.Reply.State != MessageState.Created);
                if ((current != null) && current.IsUserContext)
                {
                    current.SetClientReply(rpc.Reply, closeMessage);
                }
                else if (closeMessage)
                {
                    rpc.Reply.Close();
                }
                if (TraceUtility.MessageFlowTracingOnly && (rpc.ActivityId != Guid.Empty))
                {
                    System.Runtime.Diagnostics.DiagnosticTrace.ActivityId = Guid.Empty;
                    rpc.ActivityId = Guid.Empty;
                }
            }
            this.TerminateIfNecessary(ref rpc);
        }

        internal void IncrementActivity()
        {
            Interlocked.Increment(ref this.activityCount);
        }

        private void IncrementBusyCount()
        {
            lock (base.ThisLock)
            {
                if (base.State == CommunicationState.Opening)
                {
                    AspNetEnvironment.Current.IncrementBusyCount();
                    if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceIncrementBusyCount(base.GetType().FullName);
                    }
                    this.hasIncrementedBusyCount = true;
                }
            }
        }

        protected override void OnAbort()
        {
            if (this.idleManager != null)
            {
                this.idleManager.CancelTimer();
            }
            this.binder.Abort();
            if (this.factory != null)
            {
                this.factory.ChannelDisposed(this);
            }
            if (this.closeFactory && (this.factory != null))
            {
                this.factory.Abort();
            }
            this.CleanupChannelCollections();
            System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle != null)
            {
                serviceThrottle.DeactivateChannel();
            }
            if (((this.instanceContext != null) && this.HasSession) && this.instanceContext.HasTransaction)
            {
                this.instanceContext.Transaction.CompletePendingTransaction(this.instanceContext.Transaction.Attached, new Exception());
            }
            this.DecrementBusyCount();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.idleManager != null)
            {
                this.idleManager.CancelTimer();
            }
            if (this.factory != null)
            {
                this.factory.ChannelDisposed(this);
            }
            if ((this.InstanceContext != null) && this.InstanceContext.HasTransaction)
            {
                this.InstanceContext.CompleteAttachedTransaction();
            }
            if (this.closeBinder)
            {
                if (this.closeFactory)
                {
                    IChannel innerChannel = this.InnerChannel;
                    IChannel channel2 = this.InnerChannel;
                    return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(innerChannel.BeginClose), new ChainedEndHandler(channel2.EndClose), new ChainedBeginHandler(this.factory.BeginClose), new ChainedEndHandler(this.factory.EndClose));
                }
                return this.InnerChannel.BeginClose(timeout, callback, state);
            }
            if (this.closeFactory)
            {
                return this.factory.BeginClose(timeout, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfInitializationUINotCalled();
            if (this.autoOpenManager == null)
            {
                this.explicitlyOpened = true;
            }
            if (this.HasSession && !this.IsClient)
            {
                this.IncrementBusyCount();
            }
            this.TraceChannelOpen();
            if (this.openBinder)
            {
                return this.InnerChannel.BeginOpen(timeout, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.idleManager != null)
            {
                this.idleManager.CancelTimer();
            }
            if (this.factory != null)
            {
                this.factory.ChannelDisposed(this);
            }
            if ((this.InstanceContext != null) && this.InstanceContext.HasTransaction)
            {
                this.InstanceContext.CompleteAttachedTransaction();
            }
            if (this.closeBinder)
            {
                this.InnerChannel.Close(helper.RemainingTime());
            }
            if (this.closeFactory)
            {
                this.factory.Close(helper.RemainingTime());
            }
            this.CleanupChannelCollections();
            System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle != null)
            {
                serviceThrottle.DeactivateChannel();
            }
            this.DecrementBusyCount();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            if (this.closeBinder)
            {
                if (this.closeFactory)
                {
                    ChainedAsyncResult.End(result);
                }
                else
                {
                    this.InnerChannel.EndClose(result);
                }
            }
            else if (this.closeFactory)
            {
                this.factory.EndClose(result);
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
            this.CleanupChannelCollections();
            System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle != null)
            {
                serviceThrottle.DeactivateChannel();
            }
            this.DecrementBusyCount();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (this.openBinder)
            {
                this.InnerChannel.EndOpen(result);
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
            this.BindDuplexCallbacks();
            this.CompletedIOOperation();
        }

        private void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            base.Fault();
            if (this.HasSession)
            {
                System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime = this.ClientRuntime.DispatchRuntime;
                if (dispatchRuntime != null)
                {
                    dispatchRuntime.GetRuntime().InputSessionFaulted(this);
                }
            }
            if (this.autoClose && !this.IsClient)
            {
                base.Abort();
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.ThrowIfDisallowedInitializationUI();
            this.ThrowIfInitializationUINotCalled();
            if (this.autoOpenManager == null)
            {
                this.explicitlyOpened = true;
            }
            if (this.HasSession && !this.IsClient)
            {
                this.IncrementBusyCount();
            }
            this.TraceChannelOpen();
            if (this.openBinder)
            {
                this.InnerChannel.Open(timeout);
            }
            this.BindDuplexCallbacks();
            this.CompletedIOOperation();
        }

        private void PrepareCall(ProxyOperationRuntime operation, bool oneway, ref ProxyRpc rpc)
        {
            OperationContext current = OperationContext.Current;
            if (!oneway)
            {
                System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime = this.ClientRuntime.DispatchRuntime;
                if ((((dispatchRuntime != null) && (dispatchRuntime.ConcurrencyMode == ConcurrencyMode.Single)) && ((current != null) && !current.IsUserContext)) && (current.InternalServiceChannel == this))
                {
                    if (dispatchRuntime.IsOnServer)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCallbackRequestReplyInOrder1", new object[] { typeof(ServiceBehaviorAttribute).Name })));
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCallbackRequestReplyInOrder1", new object[] { typeof(CallbackBehaviorAttribute).Name })));
                }
            }
            if ((base.State == CommunicationState.Created) && !operation.IsInitiating)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNonInitiatingOperation1", new object[] { operation.Name })));
            }
            if (this.terminatingOperationName != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTerminatingOperationAlreadyCalled1", new object[] { this.terminatingOperationName })));
            }
            if (this.hasChannelStartedAutoClosing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("SFxClientOutputSessionAutoClosed")));
            }
            operation.BeforeRequest(ref rpc);
            this.AddMessageProperties(rpc.Request, current);
            if ((!oneway && !this.ClientRuntime.ManualAddressing) && (rpc.Request.Version.Addressing != AddressingVersion.None))
            {
                RequestReplyCorrelator.PrepareRequest(rpc.Request);
                MessageHeaders headers = rpc.Request.Headers;
                EndpointAddress localAddress = this.LocalAddress;
                EndpointAddress replyTo = headers.ReplyTo;
                if (replyTo == null)
                {
                    headers.ReplyTo = localAddress ?? EndpointAddress.AnonymousAddress;
                }
                if ((this.IsClient && (localAddress != null)) && !localAddress.IsAnonymous)
                {
                    Uri uri = localAddress.Uri;
                    if (((replyTo != null) && !replyTo.IsAnonymous) && (uri != replyTo.Uri))
                    {
                        Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRequestHasInvalidReplyToOnClient", new object[] { replyTo.Uri, uri }));
                        throw TraceUtility.ThrowHelperError(exception, rpc.Request);
                    }
                    EndpointAddress faultTo = headers.FaultTo;
                    if (((faultTo != null) && !faultTo.IsAnonymous) && (uri != faultTo.Uri))
                    {
                        Exception exception2 = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRequestHasInvalidFaultToOnClient", new object[] { faultTo.Uri, uri }));
                        throw TraceUtility.ThrowHelperError(exception2, rpc.Request);
                    }
                    if (this.messageVersion.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        EndpointAddress from = headers.From;
                        if (((from != null) && !from.IsAnonymous) && (uri != from.Uri))
                        {
                            Exception exception3 = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRequestHasInvalidFromOnClient", new object[] { from.Uri, uri }));
                            throw TraceUtility.ThrowHelperError(exception3, rpc.Request);
                        }
                    }
                }
            }
            if (TraceUtility.MessageFlowTracingOnly && (Trace.CorrelationManager.ActivityId == Guid.Empty))
            {
                rpc.ActivityId = Guid.NewGuid();
                System.ServiceModel.Diagnostics.Application.FxTrace.Trace.SetAndTraceTransfer(rpc.ActivityId, true);
            }
            if (rpc.Activity != null)
            {
                TraceUtility.SetActivity(rpc.Request, rpc.Activity);
                if (TraceUtility.ShouldPropagateActivity)
                {
                    TraceUtility.AddActivityHeader(rpc.Request);
                }
            }
            else if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
            {
                TraceUtility.AddAmbientActivityToMessage(rpc.Request);
            }
            operation.Parent.BeforeSendRequest(ref rpc);
            if (System.ServiceModel.Diagnostics.Application.TD.ClientOperationPreparedIsEnabled())
            {
                string destination = string.Empty;
                if ((this.RemoteAddress != null) && (this.RemoteAddress.Uri != null))
                {
                    destination = this.RemoteAddress.Uri.AbsoluteUri;
                }
                System.ServiceModel.Diagnostics.Application.TD.ClientOperationPrepared(rpc.Action, this.clientRuntime.ContractName, destination);
            }
            TraceUtility.MessageFlowAtMessageSent(rpc.Request);
            if (MessageLogger.LogMessagesAtServiceLevel)
            {
                MessageLogger.LogMessage(ref rpc.Request, (oneway ? MessageLoggingSource.ServiceLevelSendDatagram : MessageLoggingSource.ServiceLevelSendRequest) | MessageLoggingSource.LastChance);
            }
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.OperationTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            ProxyOperationRuntime unhandledProxyOperation = this.UnhandledProxyOperation;
            return (Message) this.Call(message.Headers.Action, false, unhandledProxyOperation, new object[] { message }, EmptyArray<object>.Instance, timeout);
        }

        public void Send(Message message)
        {
            this.Send(message, this.OperationTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            ProxyOperationRuntime unhandledProxyOperation = this.UnhandledProxyOperation;
            this.Call(message.Headers.Action, true, unhandledProxyOperation, new object[] { message }, EmptyArray<object>.Instance, timeout);
        }

        void IDisposable.Dispose()
        {
            base.Close();
        }

        IAsyncResult IDuplexContextChannel.BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.GetDuplexSessionOrThrow().BeginCloseOutputSession(timeout, callback, state);
        }

        void IDuplexContextChannel.CloseOutputSession(TimeSpan timeout)
        {
            this.GetDuplexSessionOrThrow().CloseOutputSession(timeout);
        }

        void IDuplexContextChannel.EndCloseOutputSession(IAsyncResult result)
        {
            this.GetDuplexSessionOrThrow().EndCloseOutputSession(result);
        }

        private void TerminateIfNecessary(ref ProxyRpc rpc)
        {
            if (rpc.Operation.IsTerminating)
            {
                this.terminatingOperationName = rpc.Operation.Name;
                TerminatingOperationBehavior.AfterReply(ref rpc);
            }
        }

        private void ThrowIfDisallowedInitializationUI()
        {
            if (!this.allowInitializationUI)
            {
                this.ThrowIfDisallowedInitializationUICore();
            }
        }

        private void ThrowIfDisallowedInitializationUICore()
        {
            if (this.ClientRuntime.InteractiveChannelInitializers.Count > 0)
            {
                IInteractiveChannelInitializer initializer = this.ClientRuntime.InteractiveChannelInitializers[0];
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInitializationUIDisallowed", new object[] { initializer.GetType().ToString() }));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        private void ThrowIfFaultUnderstood(Message reply, MessageFault fault, string action, System.ServiceModel.Channels.MessageVersion version, FaultConverter faultConverter)
        {
            Exception exception;
            bool isSenderFault;
            bool isReceiverFault;
            FaultCode subCode;
            if ((faultConverter != null) && faultConverter.TryCreateException(reply, fault, out exception))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(exception);
            }
            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                isSenderFault = true;
                isReceiverFault = true;
                subCode = fault.Code;
            }
            else
            {
                isSenderFault = fault.Code.IsSenderFault;
                isReceiverFault = fault.Code.IsReceiverFault;
                subCode = fault.Code.SubCode;
            }
            if ((subCode != null) && (subCode.Namespace != null))
            {
                if (isSenderFault)
                {
                    if (string.Compare(subCode.Namespace, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher", StringComparison.Ordinal) == 0)
                    {
                        if (string.Compare(subCode.Name, "SessionTerminated", StringComparison.Ordinal) == 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ChannelTerminatedException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                        }
                        if (string.Compare(subCode.Name, "TransactionAborted", StringComparison.Ordinal) == 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                        }
                    }
                    if ((string.Compare(subCode.Namespace, SecurityVersion.Default.HeaderNamespace.Value, StringComparison.Ordinal) == 0) && (string.Compare(subCode.Name, SecurityVersion.Default.FailedAuthenticationFaultCode.Value, StringComparison.Ordinal) == 0))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityAccessDeniedException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                    }
                }
                if (isReceiverFault && (string.Compare(subCode.Namespace, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher", StringComparison.Ordinal) == 0))
                {
                    if (string.Compare(subCode.Name, "InternalServiceFault", StringComparison.Ordinal) == 0)
                    {
                        if (this.HasSession)
                        {
                            base.Fault();
                        }
                        if (fault.HasDetail)
                        {
                            ExceptionDetail detail = fault.GetDetail<ExceptionDetail>();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new FaultException<ExceptionDetail>(detail, fault.Reason, fault.Code, action));
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new FaultException(fault, action));
                    }
                    if (string.Compare(subCode.Name, "DeserializationFailed", StringComparison.Ordinal) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text));
                    }
                }
            }
        }

        private void ThrowIfIdleAborted(ProxyOperationRuntime operation)
        {
            if ((this.idleManager != null) && this.idleManager.DidIdleAbort)
            {
                Exception exception = new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("SFxServiceChannelIdleAborted", new object[] { operation.Name }));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        private void ThrowIfInitializationUINotCalled()
        {
            if (!this.didInteractiveInitialization && (this.ClientRuntime.InteractiveChannelInitializers.Count > 0))
            {
                IInteractiveChannelInitializer initializer = this.ClientRuntime.InteractiveChannelInitializers[0];
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInitializationUINotCalled", new object[] { initializer.GetType().ToString() }));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        private void ThrowIfOpening()
        {
            if (base.State == CommunicationState.Opening)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCannotCallAutoOpenWhenExplicitOpenCalled")));
            }
        }

        private void TraceChannelOpen()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(4);
                bool flag = false;
                System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime = this.DispatchRuntime;
                if (dispatchRuntime != null)
                {
                    if (dispatchRuntime.Type != null)
                    {
                        dictionary["ServiceType"] = dispatchRuntime.Type.AssemblyQualifiedName;
                    }
                    dictionary["ContractNamespace"] = this.clientRuntime.ContractNamespace;
                    dictionary["ContractName"] = this.clientRuntime.ContractName;
                    flag = true;
                }
                if ((this.endpointDispatcher != null) && (this.endpointDispatcher.ListenUri != null))
                {
                    dictionary["Uri"] = this.endpointDispatcher.ListenUri.ToString();
                    flag = true;
                }
                if (flag)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0x8002b, System.ServiceModel.SR.GetString("TraceCodeServiceChannelLifetime"), new DictionaryTraceRecord(dictionary), this, null);
                }
            }
        }

        private CallOnceManager AutoDisplayUIManager
        {
            get
            {
                if (!this.explicitlyOpened && (this.autoDisplayUIManager == null))
                {
                    this.EnsureAutoOpenManagers();
                }
                return this.autoDisplayUIManager;
            }
        }

        private CallOnceManager AutoOpenManager
        {
            get
            {
                if (!this.explicitlyOpened && (this.autoOpenManager == null))
                {
                    this.EnsureAutoOpenManagers();
                }
                return this.autoOpenManager;
            }
        }

        internal IChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        internal System.ServiceModel.Dispatcher.ChannelDispatcher ChannelDispatcher
        {
            get
            {
                return this.channelDispatcher;
            }
        }

        internal System.ServiceModel.Dispatcher.ClientRuntime ClientRuntime
        {
            get
            {
                return this.clientRuntime;
            }
        }

        internal bool CloseFactory
        {
            get
            {
                return this.closeFactory;
            }
            set
            {
                this.closeFactory = value;
            }
        }

        internal TimeSpan CloseTimeout
        {
            get
            {
                if (this.IsClient)
                {
                    return this.factory.InternalCloseTimeout;
                }
                return this.ChannelDispatcher.InternalCloseTimeout;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.CloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return this.OpenTimeout;
            }
        }

        internal System.ServiceModel.Dispatcher.DispatchRuntime DispatchRuntime
        {
            get
            {
                if (this.endpointDispatcher != null)
                {
                    return this.endpointDispatcher.DispatchRuntime;
                }
                if (this.clientRuntime != null)
                {
                    return this.clientRuntime.DispatchRuntime;
                }
                return null;
            }
        }

        internal System.ServiceModel.Dispatcher.EndpointDispatcher EndpointDispatcher
        {
            get
            {
                return this.endpointDispatcher;
            }
            set
            {
                lock (base.ThisLock)
                {
                    this.endpointDispatcher = value;
                    this.clientRuntime = value.DispatchRuntime.CallbackClientRuntime;
                }
            }
        }

        internal ServiceChannelFactory Factory
        {
            get
            {
                return this.factory;
            }
        }

        internal bool HasSession
        {
            get
            {
                return this.hasSession;
            }
        }

        internal IChannel InnerChannel
        {
            get
            {
                return this.binder.Channel;
            }
        }

        internal System.ServiceModel.InstanceContext InstanceContext
        {
            get
            {
                return this.instanceContext;
            }
            set
            {
                this.instanceContext = value;
            }
        }

        internal System.ServiceModel.Dispatcher.ServiceThrottle InstanceContextServiceThrottle
        {
            get
            {
                return this.instanceContextServiceThrottle;
            }
            set
            {
                this.instanceContextServiceThrottle = value;
            }
        }

        internal bool IsClient
        {
            get
            {
                return (this.factory != null);
            }
        }

        internal bool IsPending
        {
            get
            {
                return this.isPending;
            }
            set
            {
                this.isPending = value;
            }
        }

        internal bool IsReplyChannel
        {
            get
            {
                return this.isReplyChannel;
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.binder.ListenUri;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                if (this.localAddress == null)
                {
                    if (this.endpointDispatcher != null)
                    {
                        this.localAddress = this.endpointDispatcher.EndpointAddress;
                    }
                    else
                    {
                        this.localAddress = this.binder.LocalAddress;
                    }
                }
                return this.localAddress;
            }
        }

        internal System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        internal TimeSpan OpenTimeout
        {
            get
            {
                if (this.IsClient)
                {
                    return this.factory.InternalOpenTimeout;
                }
                return this.ChannelDispatcher.InternalOpenTimeout;
            }
        }

        public TimeSpan OperationTimeout
        {
            get
            {
                return this.operationTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    string message = System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.operationTimeout = value;
            }
        }

        internal object Proxy
        {
            get
            {
                object proxy = this.proxy;
                if (proxy != null)
                {
                    return proxy;
                }
                return this;
            }
            set
            {
                this.proxy = value;
                base.EventSender = value;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                IOutputChannel innerChannel = this.InnerChannel as IOutputChannel;
                if (innerChannel != null)
                {
                    return innerChannel.RemoteAddress;
                }
                IRequestChannel channel2 = this.InnerChannel as IRequestChannel;
                if (channel2 != null)
                {
                    return channel2.RemoteAddress;
                }
                return null;
            }
        }

        internal System.ServiceModel.Dispatcher.ServiceThrottle ServiceThrottle
        {
            get
            {
                return this.serviceThrottle;
            }
            set
            {
                base.ThrowIfDisposed();
                this.serviceThrottle = value;
            }
        }

        bool IClientChannel.AllowInitializationUI
        {
            get
            {
                return this.allowInitializationUI;
            }
            set
            {
                base.ThrowIfDisposedOrImmutable();
                this.allowInitializationUI = value;
            }
        }

        bool IClientChannel.DidInteractiveInitialization
        {
            get
            {
                return this.didInteractiveInitialization;
            }
        }

        bool IContextChannel.AllowOutputBatching
        {
            get
            {
                return this.allowOutputBatching;
            }
            set
            {
                this.allowOutputBatching = value;
            }
        }

        IInputSession IContextChannel.InputSession
        {
            get
            {
                if (this.InnerChannel != null)
                {
                    ISessionChannel<IInputSession> innerChannel = this.InnerChannel as ISessionChannel<IInputSession>;
                    if (innerChannel != null)
                    {
                        return innerChannel.Session;
                    }
                    ISessionChannel<IDuplexSession> channel2 = this.InnerChannel as ISessionChannel<IDuplexSession>;
                    if (channel2 != null)
                    {
                        return channel2.Session;
                    }
                }
                return null;
            }
        }

        IOutputSession IContextChannel.OutputSession
        {
            get
            {
                if (this.InnerChannel != null)
                {
                    ISessionChannel<IOutputSession> innerChannel = this.InnerChannel as ISessionChannel<IOutputSession>;
                    if (innerChannel != null)
                    {
                        return innerChannel.Session;
                    }
                    ISessionChannel<IDuplexSession> channel2 = this.InnerChannel as ISessionChannel<IDuplexSession>;
                    if (channel2 != null)
                    {
                        return channel2.Session;
                    }
                }
                return null;
            }
        }

        string IContextChannel.SessionId
        {
            get
            {
                if (this.InnerChannel != null)
                {
                    ISessionChannel<IInputSession> innerChannel = this.InnerChannel as ISessionChannel<IInputSession>;
                    if (innerChannel != null)
                    {
                        return innerChannel.Session.Id;
                    }
                    ISessionChannel<IOutputSession> channel2 = this.InnerChannel as ISessionChannel<IOutputSession>;
                    if (channel2 != null)
                    {
                        return channel2.Session.Id;
                    }
                    ISessionChannel<IDuplexSession> channel3 = this.InnerChannel as ISessionChannel<IDuplexSession>;
                    if (channel3 != null)
                    {
                        return channel3.Session.Id;
                    }
                }
                return null;
            }
        }

        bool IDuplexContextChannel.AutomaticInputSessionShutdown
        {
            get
            {
                return this.autoClose;
            }
            set
            {
                this.autoClose = value;
            }
        }

        System.ServiceModel.InstanceContext IDuplexContextChannel.CallbackInstance
        {
            get
            {
                return this.instanceContext;
            }
            set
            {
                lock (base.ThisLock)
                {
                    if (this.instanceContext != null)
                    {
                        this.instanceContext.OutgoingChannels.Remove((IChannel) this.proxy);
                    }
                    this.instanceContext = value;
                    if (this.instanceContext != null)
                    {
                        this.instanceContext.OutgoingChannels.Add((IChannel) this.proxy);
                    }
                }
            }
        }

        IExtensionCollection<IContextChannel> IExtensibleObject<IContextChannel>.Extensions
        {
            get
            {
                lock (base.ThisLock)
                {
                    if (this.extensions == null)
                    {
                        this.extensions = new ExtensionCollection<IContextChannel>((IContextChannel) this.Proxy, base.ThisLock);
                    }
                    return this.extensions;
                }
            }
        }

        private ProxyOperationRuntime UnhandledProxyOperation
        {
            get
            {
                return this.ClientRuntime.GetRuntime().UnhandledProxyOperation;
            }
        }

        public Uri Via
        {
            get
            {
                IOutputChannel innerChannel = this.InnerChannel as IOutputChannel;
                if (innerChannel != null)
                {
                    return innerChannel.Via;
                }
                IRequestChannel channel2 = this.InnerChannel as IRequestChannel;
                if (channel2 != null)
                {
                    return channel2.Via;
                }
                return null;
            }
        }

        internal System.ServiceModel.InstanceContext WmiInstanceContext
        {
            get
            {
                return this.wmiInstanceContext;
            }
            set
            {
                this.wmiInstanceContext = value;
            }
        }

        private class CallDisplayUIOnce : ServiceChannel.ICallOnce
        {
            private static ServiceChannel.CallDisplayUIOnce instance;

            IAsyncResult ServiceChannel.ICallOnce.BeginCall(ServiceChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginDisplayInitializationUI(callback, state);
            }

            void ServiceChannel.ICallOnce.Call(ServiceChannel channel, TimeSpan timeout)
            {
                channel.DisplayInitializationUI();
            }

            void ServiceChannel.ICallOnce.EndCall(ServiceChannel channel, IAsyncResult result)
            {
                channel.EndDisplayInitializationUI(result);
            }

            [Conditional("DEBUG")]
            private void ValidateTimeoutIsMaxValue(TimeSpan timeout)
            {
                bool flag1 = timeout != TimeSpan.MaxValue;
            }

            internal static ServiceChannel.CallDisplayUIOnce Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new ServiceChannel.CallDisplayUIOnce();
                    }
                    return instance;
                }
            }
        }

        private class CallOnceCompletedAsyncResult : AsyncResult
        {
            internal CallOnceCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
                base.Complete(true);
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<ServiceChannel.CallOnceCompletedAsyncResult>(result);
            }
        }

        private class CallOnceManager
        {
            private readonly ServiceChannel.ICallOnce callOnce;
            private readonly ServiceChannel channel;
            private bool isFirst = true;
            private Queue<IWaiter> queue;
            private static Action<object> signalWaiter = new Action<object>(ServiceChannel.CallOnceManager.SignalWaiter);

            internal CallOnceManager(ServiceChannel channel, ServiceChannel.ICallOnce callOnce)
            {
                this.callOnce = callOnce;
                this.channel = channel;
                this.queue = new Queue<IWaiter>();
            }

            internal IAsyncResult BeginCallOnce(TimeSpan timeout, ServiceChannel.CallOnceManager cascade, AsyncCallback callback, object state)
            {
                AsyncWaiter item = null;
                bool flag = false;
                if (this.queue != null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.queue != null)
                        {
                            if (this.isFirst)
                            {
                                flag = true;
                                this.isFirst = false;
                            }
                            else
                            {
                                item = new AsyncWaiter(this, timeout, callback, state);
                                this.queue.Enqueue(item);
                            }
                        }
                    }
                }
                SignalNextIfNonNull(cascade);
                if (flag)
                {
                    bool flag3 = true;
                    try
                    {
                        IAsyncResult result = this.callOnce.BeginCall(this.channel, timeout, callback, state);
                        flag3 = false;
                        return result;
                    }
                    finally
                    {
                        if (flag3)
                        {
                            this.SignalNext();
                        }
                    }
                }
                if (item != null)
                {
                    return item;
                }
                return new ServiceChannel.CallOnceCompletedAsyncResult(callback, state);
            }

            internal void CallOnce(TimeSpan timeout, ServiceChannel.CallOnceManager cascade)
            {
                SyncWaiter item = null;
                bool flag = false;
                if (this.queue != null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.queue != null)
                        {
                            if (this.isFirst)
                            {
                                flag = true;
                                this.isFirst = false;
                            }
                            else
                            {
                                item = new SyncWaiter(this);
                                this.queue.Enqueue(item);
                            }
                        }
                    }
                }
                SignalNextIfNonNull(cascade);
                if (flag)
                {
                    bool flag3 = true;
                    try
                    {
                        this.callOnce.Call(this.channel, timeout);
                        flag3 = false;
                    }
                    finally
                    {
                        if (flag3)
                        {
                            this.SignalNext();
                        }
                    }
                }
                else if (item != null)
                {
                    item.Wait(timeout);
                }
            }

            internal void EndCallOnce(IAsyncResult result)
            {
                if (result is ServiceChannel.CallOnceCompletedAsyncResult)
                {
                    ServiceChannel.CallOnceCompletedAsyncResult.End(result);
                }
                else if (result is AsyncWaiter)
                {
                    AsyncWaiter.End(result);
                }
                else
                {
                    bool flag = true;
                    try
                    {
                        this.callOnce.EndCall(this.channel, result);
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.SignalNext();
                        }
                    }
                }
            }

            internal void SignalNext()
            {
                if (this.queue != null)
                {
                    IWaiter state = null;
                    lock (this.ThisLock)
                    {
                        if (this.queue != null)
                        {
                            if (this.queue.Count > 0)
                            {
                                state = this.queue.Dequeue();
                            }
                            else
                            {
                                this.queue = null;
                            }
                        }
                    }
                    if (state != null)
                    {
                        ActionItem.Schedule(signalWaiter, state);
                    }
                }
            }

            internal static void SignalNextIfNonNull(ServiceChannel.CallOnceManager manager)
            {
                if (manager != null)
                {
                    manager.SignalNext();
                }
            }

            private static void SignalWaiter(object state)
            {
                ((IWaiter) state).Signal();
            }

            private object ThisLock
            {
                get
                {
                    return this;
                }
            }

            private class AsyncWaiter : AsyncResult, ServiceChannel.CallOnceManager.IWaiter
            {
                private ServiceChannel.CallOnceManager manager;
                private TimeSpan timeout;
                private IOThreadTimer timer;
                private static Action<object> timerCallback = new Action<object>(ServiceChannel.CallOnceManager.AsyncWaiter.TimerCallback);

                internal AsyncWaiter(ServiceChannel.CallOnceManager manager, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.manager = manager;
                    this.timeout = timeout;
                    if (timeout != TimeSpan.MaxValue)
                    {
                        this.timer = new IOThreadTimer(timerCallback, this, false);
                        this.timer.Set(timeout);
                    }
                }

                internal static void End(IAsyncResult result)
                {
                    AsyncResult.End<ServiceChannel.CallOnceManager.AsyncWaiter>(result);
                }

                private void OnClosed(object sender, EventArgs e)
                {
                    if ((this.timer == null) || this.timer.Cancel())
                    {
                        base.Complete(false, this.manager.channel.CreateClosedException());
                    }
                }

                void ServiceChannel.CallOnceManager.IWaiter.Signal()
                {
                    if ((this.timer == null) || this.timer.Cancel())
                    {
                        base.Complete(false);
                        this.manager.channel.Closed -= new EventHandler(this.OnClosed);
                    }
                    else
                    {
                        this.manager.SignalNext();
                    }
                }

                private static void TimerCallback(object state)
                {
                    ServiceChannel.CallOnceManager.AsyncWaiter waiter = (ServiceChannel.CallOnceManager.AsyncWaiter) state;
                    waiter.Complete(false, waiter.manager.channel.GetOpenTimeoutException(waiter.timeout));
                }
            }

            private interface IWaiter
            {
                void Signal();
            }

            private class SyncWaiter : ServiceChannel.CallOnceManager.IWaiter
            {
                private bool isSignaled;
                private bool isTimedOut;
                private ServiceChannel.CallOnceManager manager;
                private ManualResetEvent wait = new ManualResetEvent(false);
                private int waitCount;

                internal SyncWaiter(ServiceChannel.CallOnceManager manager)
                {
                    this.manager = manager;
                }

                private void CloseWaitHandle()
                {
                    if (Interlocked.Increment(ref this.waitCount) == 2)
                    {
                        this.wait.Close();
                    }
                }

                void ServiceChannel.CallOnceManager.IWaiter.Signal()
                {
                    bool shouldSignalNext;
                    this.wait.Set();
                    this.CloseWaitHandle();
                    lock (this.manager.ThisLock)
                    {
                        this.isSignaled = true;
                        shouldSignalNext = this.ShouldSignalNext;
                    }
                    if (shouldSignalNext)
                    {
                        this.manager.SignalNext();
                    }
                }

                internal bool Wait(TimeSpan timeout)
                {
                    try
                    {
                        if (!TimeoutHelper.WaitOne(this.wait, timeout))
                        {
                            bool shouldSignalNext;
                            lock (this.manager.ThisLock)
                            {
                                this.isTimedOut = true;
                                shouldSignalNext = this.ShouldSignalNext;
                            }
                            if (shouldSignalNext)
                            {
                                this.manager.SignalNext();
                            }
                        }
                    }
                    finally
                    {
                        this.CloseWaitHandle();
                    }
                    return !this.isTimedOut;
                }

                private bool ShouldSignalNext
                {
                    get
                    {
                        return (this.isTimedOut && this.isSignaled);
                    }
                }
            }
        }

        private class CallOpenOnce : ServiceChannel.ICallOnce
        {
            private static ServiceChannel.CallOpenOnce instance;

            IAsyncResult ServiceChannel.ICallOnce.BeginCall(ServiceChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginOpen(timeout, callback, state);
            }

            void ServiceChannel.ICallOnce.Call(ServiceChannel channel, TimeSpan timeout)
            {
                channel.Open(timeout);
            }

            void ServiceChannel.ICallOnce.EndCall(ServiceChannel channel, IAsyncResult result)
            {
                channel.EndOpen(result);
            }

            internal static ServiceChannel.CallOpenOnce Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new ServiceChannel.CallOpenOnce();
                    }
                    return instance;
                }
            }
        }

        private interface ICallOnce
        {
            IAsyncResult BeginCall(ServiceChannel channel, TimeSpan timeout, AsyncCallback callback, object state);
            void Call(ServiceChannel channel, TimeSpan timeout);
            void EndCall(ServiceChannel channel, IAsyncResult result);
        }

        private class SendAsyncResult : TraceAsyncResult
        {
            private static AsyncCallback ensureInteractiveInitCallback = Fx.ThunkCallback(new AsyncCallback(ServiceChannel.SendAsyncResult.EnsureInteractiveInitCallback));
            private static AsyncCallback ensureOpenCallback = Fx.ThunkCallback(new AsyncCallback(ServiceChannel.SendAsyncResult.EnsureOpenCallback));
            private readonly bool isOneWay;
            private readonly ProxyOperationRuntime operation;
            private OperationContext operationContext;
            internal ProxyRpc Rpc;
            private static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(ServiceChannel.SendAsyncResult.SendCallback));

            internal SendAsyncResult(ServiceChannel channel, ProxyOperationRuntime operation, string action, object[] inputParameters, bool isOneWay, TimeSpan timeout, AsyncCallback userCallback, object userState) : base(userCallback, userState)
            {
                this.Rpc = new ProxyRpc(channel, operation, action, inputParameters, timeout);
                this.isOneWay = isOneWay;
                this.operation = operation;
                this.operationContext = OperationContext.Current;
            }

            internal void Begin()
            {
                this.Rpc.Channel.PrepareCall(this.operation, this.isOneWay, ref this.Rpc);
                if (this.Rpc.Channel.explicitlyOpened)
                {
                    this.Rpc.Channel.ThrowIfOpening();
                    this.Rpc.Channel.ThrowIfDisposedOrNotOpen();
                    this.StartSend(true);
                }
                else
                {
                    this.StartEnsureInteractiveInit();
                }
            }

            private void CallComplete(bool completedSynchronously, Exception exception)
            {
                this.Rpc.Channel.CompletedIOOperation();
                base.Complete(completedSynchronously, exception);
            }

            public static void End(ServiceChannel.SendAsyncResult result)
            {
                try
                {
                    AsyncResult.End<ServiceChannel.SendAsyncResult>(result);
                }
                finally
                {
                    ConcurrencyBehavior.LockInstanceAfterCallout(result.operationContext);
                }
            }

            private static void EnsureInteractiveInitCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ServiceChannel.SendAsyncResult) result.AsyncState).FinishEnsureInteractiveInit(result, false);
                }
            }

            private static void EnsureOpenCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ServiceChannel.SendAsyncResult) result.AsyncState).FinishEnsureOpen(result, false);
                }
            }

            private void FinishEnsureInteractiveInit(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    this.Rpc.Channel.EndEnsureDisplayUI(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.StartEnsureOpen(completedSynchronously);
                }
            }

            private void FinishEnsureOpen(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                using (ServiceModelActivity.BoundOperation(this.Rpc.Activity))
                {
                    try
                    {
                        this.Rpc.Channel.EndEnsureOpened(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2) || completedSynchronously)
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (exception != null)
                    {
                        this.CallComplete(completedSynchronously, exception);
                    }
                    else
                    {
                        this.StartSend(completedSynchronously);
                    }
                }
            }

            private void FinishSend(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    if (this.isOneWay)
                    {
                        this.Rpc.Channel.binder.EndSend(result);
                    }
                    else
                    {
                        this.Rpc.Reply = this.Rpc.Channel.binder.EndRequest(result);
                        if (this.Rpc.Reply == null)
                        {
                            this.Rpc.Channel.ThrowIfFaulted();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxServerDidNotReply")));
                        }
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    if (completedSynchronously)
                    {
                        ConcurrencyBehavior.LockInstanceAfterCallout(this.operationContext);
                        throw;
                    }
                    exception = exception2;
                }
                this.CallComplete(completedSynchronously, exception);
            }

            private static void SendCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ServiceChannel.SendAsyncResult) result.AsyncState).FinishSend(result, false);
                }
            }

            private void StartEnsureInteractiveInit()
            {
                IAsyncResult result = this.Rpc.Channel.BeginEnsureDisplayUI(ensureInteractiveInitCallback, this);
                if (result.CompletedSynchronously)
                {
                    this.FinishEnsureInteractiveInit(result, true);
                }
            }

            private void StartEnsureOpen(bool completedSynchronously)
            {
                TimeSpan timeout = this.Rpc.TimeoutHelper.RemainingTime();
                IAsyncResult result = null;
                Exception exception = null;
                try
                {
                    result = this.Rpc.Channel.BeginEnsureOpened(timeout, ensureOpenCallback, this);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishEnsureOpen(result, completedSynchronously);
                }
            }

            private void StartSend(bool completedSynchronously)
            {
                TimeSpan timeout = this.Rpc.TimeoutHelper.RemainingTime();
                IAsyncResult result = null;
                Exception exception = null;
                try
                {
                    ConcurrencyBehavior.UnlockInstanceBeforeCallout(this.operationContext);
                    if (this.isOneWay)
                    {
                        result = this.Rpc.Channel.binder.BeginSend(this.Rpc.Request, timeout, sendCallback, this);
                    }
                    else
                    {
                        result = this.Rpc.Channel.binder.BeginRequest(this.Rpc.Request, timeout, sendCallback, this);
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    if (completedSynchronously)
                    {
                        ConcurrencyBehavior.LockInstanceAfterCallout(this.operationContext);
                        throw;
                    }
                    exception = exception2;
                }
                finally
                {
                    ServiceChannel.CallOnceManager.SignalNextIfNonNull(this.Rpc.Channel.autoOpenManager);
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishSend(result, completedSynchronously);
                }
            }
        }

        internal class SessionIdleManager
        {
            private readonly IChannelBinder binder;
            private ServiceChannel channel;
            private bool didIdleAbort;
            private readonly long idleTicks;
            private bool isTimerCancelled;
            private long lastActivity;
            private object thisLock;
            private readonly IOThreadTimer timer;
            private static Action<object> timerCallback;

            private SessionIdleManager(IChannelBinder binder, TimeSpan idle)
            {
                this.binder = binder;
                this.timer = new IOThreadTimer(GetTimerCallback(), this, false);
                this.idleTicks = Ticks.FromTimeSpan(idle);
                this.timer.SetAt(Ticks.Now + this.idleTicks);
                this.thisLock = new object();
            }

            internal void CancelTimer()
            {
                lock (this.thisLock)
                {
                    this.isTimerCancelled = true;
                    this.timer.Cancel();
                }
            }

            internal void CompletedActivity()
            {
                Interlocked.Exchange(ref this.lastActivity, Ticks.Now);
            }

            internal static ServiceChannel.SessionIdleManager CreateIfNeeded(IChannelBinder binder, TimeSpan idle)
            {
                if (binder.HasSession && (idle != TimeSpan.MaxValue))
                {
                    return new ServiceChannel.SessionIdleManager(binder, idle);
                }
                return null;
            }

            private static Action<object> GetTimerCallback()
            {
                if (timerCallback == null)
                {
                    timerCallback = new Action<object>(ServiceChannel.SessionIdleManager.TimerCallback);
                }
                return timerCallback;
            }

            internal void RegisterChannel(ServiceChannel channel, out bool didIdleAbort)
            {
                lock (this.thisLock)
                {
                    this.channel = channel;
                    didIdleAbort = this.didIdleAbort;
                }
            }

            private void TimerCallback()
            {
                long dueTime = Interlocked.CompareExchange(ref this.lastActivity, 0L, 0L) + this.idleTicks;
                lock (this.thisLock)
                {
                    if (Ticks.Now > dueTime)
                    {
                        this.didIdleAbort = true;
                        if (this.channel != null)
                        {
                            this.channel.Abort();
                        }
                        else
                        {
                            this.binder.Abort();
                        }
                    }
                    else if ((!this.isTimerCancelled && (this.binder.Channel.State != CommunicationState.Faulted)) && (this.binder.Channel.State != CommunicationState.Closed))
                    {
                        this.timer.SetAt(dueTime);
                    }
                }
            }

            private static void TimerCallback(object state)
            {
                ((ServiceChannel.SessionIdleManager) state).TimerCallback();
            }

            internal bool DidIdleAbort
            {
                get
                {
                    lock (this.thisLock)
                    {
                        return this.didIdleAbort;
                    }
                }
            }
        }
    }
}

