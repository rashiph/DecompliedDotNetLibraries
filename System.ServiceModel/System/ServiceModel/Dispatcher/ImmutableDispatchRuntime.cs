namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Transactions;

    internal class ImmutableDispatchRuntime
    {
        private readonly AuthenticationBehavior authenticationBehavior;
        private readonly AuthorizationBehavior authorizationBehavior;
        private readonly ConcurrencyBehavior concurrency;
        private readonly int correlationCount;
        private readonly IDemuxer demuxer;
        private bool didTraceProcessMessage1;
        private bool didTraceProcessMessage2;
        private bool didTraceProcessMessage3;
        private bool didTraceProcessMessage31;
        private bool didTraceProcessMessage4;
        private bool didTraceProcessMessage41;
        private readonly bool enableFaults;
        private readonly System.ServiceModel.Dispatcher.ErrorBehavior error;
        private readonly bool ignoreTransactionFlow;
        private readonly IInputSessionShutdown[] inputSessionShutdownHandlers;
        private readonly System.ServiceModel.Dispatcher.InstanceBehavior instance;
        private readonly bool isOnServer;
        private readonly bool manualAddressing;
        private readonly IDispatchMessageInspector[] messageInspectors;
        private static AsyncCallback onFinalizeCorrelationCompleted = Fx.ThunkCallback(new AsyncCallback(ImmutableDispatchRuntime.OnFinalizeCorrelationCompletedCallback));
        private static AsyncCallback onReplyCompleted = Fx.ThunkCallback(new AsyncCallback(ImmutableDispatchRuntime.OnReplyCompletedCallback));
        private readonly int parameterInspectorCorrelationOffset;
        private readonly MessageRpcProcessor processMessage1;
        private readonly MessageRpcProcessor processMessage11;
        private readonly MessageRpcProcessor processMessage2;
        private readonly MessageRpcProcessor processMessage3;
        private readonly MessageRpcProcessor processMessage31;
        private readonly MessageRpcProcessor processMessage4;
        private readonly MessageRpcProcessor processMessage41;
        private readonly MessageRpcProcessor processMessage5;
        private readonly MessageRpcProcessor processMessage6;
        private readonly MessageRpcProcessor processMessage7;
        private readonly MessageRpcProcessor processMessage8;
        private readonly MessageRpcProcessor processMessage9;
        private readonly MessageRpcProcessor processMessageCleanup;
        private readonly MessageRpcProcessor processMessageCleanupError;
        private readonly bool receiveContextEnabledChannel;
        private readonly IRequestReplyCorrelator requestReplyCorrelator;
        private readonly SecurityImpersonationBehavior securityImpersonation;
        private readonly bool sendAsynchronously;
        private readonly TerminatingOperationBehavior terminate;
        private readonly ThreadBehavior thread;
        private readonly TransactionBehavior transaction;
        private readonly bool validateMustUnderstand;

        internal ImmutableDispatchRuntime(DispatchRuntime dispatch)
        {
            this.authenticationBehavior = AuthenticationBehavior.TryCreate(dispatch);
            this.authorizationBehavior = AuthorizationBehavior.TryCreate(dispatch);
            this.concurrency = new ConcurrencyBehavior(dispatch);
            this.error = new System.ServiceModel.Dispatcher.ErrorBehavior(dispatch.ChannelDispatcher);
            this.enableFaults = dispatch.EnableFaults;
            this.inputSessionShutdownHandlers = EmptyArray<IInputSessionShutdown>.ToArray(dispatch.InputSessionShutdownHandlers);
            this.instance = new System.ServiceModel.Dispatcher.InstanceBehavior(dispatch, this);
            this.isOnServer = dispatch.IsOnServer;
            this.manualAddressing = dispatch.ManualAddressing;
            this.messageInspectors = EmptyArray<IDispatchMessageInspector>.ToArray(dispatch.MessageInspectors);
            this.requestReplyCorrelator = new System.ServiceModel.Channels.RequestReplyCorrelator();
            this.securityImpersonation = SecurityImpersonationBehavior.CreateIfNecessary(dispatch);
            this.terminate = TerminatingOperationBehavior.CreateIfNecessary(dispatch);
            this.thread = new ThreadBehavior(dispatch);
            this.validateMustUnderstand = dispatch.ValidateMustUnderstand;
            this.ignoreTransactionFlow = dispatch.IgnoreTransactionMessageProperty;
            this.transaction = TransactionBehavior.CreateIfNeeded(dispatch);
            this.receiveContextEnabledChannel = dispatch.ChannelDispatcher.ReceiveContextEnabled;
            this.sendAsynchronously = dispatch.ChannelDispatcher.SendAsynchronously;
            this.parameterInspectorCorrelationOffset = dispatch.MessageInspectors.Count + dispatch.MaxCallContextInitializers;
            this.correlationCount = this.parameterInspectorCorrelationOffset + dispatch.MaxParameterInspectors;
            DispatchOperationRuntime runtime = new DispatchOperationRuntime(dispatch.UnhandledDispatchOperation, this);
            if (dispatch.OperationSelector == null)
            {
                ActionDemuxer demuxer = new ActionDemuxer();
                for (int i = 0; i < dispatch.Operations.Count; i++)
                {
                    DispatchOperation operation = dispatch.Operations[i];
                    DispatchOperationRuntime runtime2 = new DispatchOperationRuntime(operation, this);
                    demuxer.Add(operation.Action, runtime2);
                }
                demuxer.SetUnhandled(runtime);
                this.demuxer = demuxer;
            }
            else
            {
                CustomDemuxer demuxer2 = new CustomDemuxer(dispatch.OperationSelector);
                for (int j = 0; j < dispatch.Operations.Count; j++)
                {
                    DispatchOperation operation2 = dispatch.Operations[j];
                    DispatchOperationRuntime runtime3 = new DispatchOperationRuntime(operation2, this);
                    demuxer2.Add(operation2.Name, runtime3);
                }
                demuxer2.SetUnhandled(runtime);
                this.demuxer = demuxer2;
            }
            this.processMessage1 = new MessageRpcProcessor(this.ProcessMessage1);
            this.processMessage11 = new MessageRpcProcessor(this.ProcessMessage11);
            this.processMessage2 = new MessageRpcProcessor(this.ProcessMessage2);
            this.processMessage3 = new MessageRpcProcessor(this.ProcessMessage3);
            this.processMessage31 = new MessageRpcProcessor(this.ProcessMessage31);
            this.processMessage4 = new MessageRpcProcessor(this.ProcessMessage4);
            this.processMessage41 = new MessageRpcProcessor(this.ProcessMessage41);
            this.processMessage5 = new MessageRpcProcessor(this.ProcessMessage5);
            this.processMessage6 = new MessageRpcProcessor(this.ProcessMessage6);
            this.processMessage7 = new MessageRpcProcessor(this.ProcessMessage7);
            this.processMessage8 = new MessageRpcProcessor(this.ProcessMessage8);
            this.processMessage9 = new MessageRpcProcessor(this.ProcessMessage9);
            this.processMessageCleanup = new MessageRpcProcessor(this.ProcessMessageCleanup);
            this.processMessageCleanupError = new MessageRpcProcessor(this.ProcessMessageCleanupError);
        }

        private bool AcquireDynamicInstanceContext(ref MessageRpc rpc)
        {
            if (rpc.InstanceContext.QuotaThrottle != null)
            {
                return this.AcquireDynamicInstanceContextCore(ref rpc);
            }
            return true;
        }

        private bool AcquireDynamicInstanceContextCore(ref MessageRpc rpc)
        {
            bool flag = rpc.InstanceContext.QuotaThrottle.Acquire(rpc.Pause());
            if (flag)
            {
                rpc.UnPause();
            }
            return flag;
        }

        private void AddMessageProperties(Message message, OperationContext context, ServiceChannel replyChannel)
        {
            if (context.InternalServiceChannel == replyChannel)
            {
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

        internal void AfterReceiveRequest(ref MessageRpc rpc)
        {
            if (this.messageInspectors.Length > 0)
            {
                this.AfterReceiveRequestCore(ref rpc);
            }
        }

        internal void AfterReceiveRequestCore(ref MessageRpc rpc)
        {
            int messageInspectorCorrelationOffset = this.MessageInspectorCorrelationOffset;
            try
            {
                for (int i = 0; i < this.messageInspectors.Length; i++)
                {
                    rpc.Correlation[messageInspectorCorrelationOffset + i] = this.messageInspectors[i].AfterReceiveRequest(ref rpc.Request, (IClientChannel) rpc.Channel.Proxy, rpc.InstanceContext);
                    if (TD.MessageInspectorAfterReceiveInvokedIsEnabled())
                    {
                        TD.MessageInspectorAfterReceiveInvoked(this.messageInspectors[i].GetType().FullName);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.Dispatcher.ErrorBehavior.ShouldRethrowExceptionAsIs(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
        }

        private void BeforeSendReply(ref MessageRpc rpc, ref Exception exception, ref bool thereIsAnUnhandledException)
        {
            if (this.messageInspectors.Length > 0)
            {
                this.BeforeSendReplyCore(ref rpc, ref exception, ref thereIsAnUnhandledException);
            }
        }

        internal void BeforeSendReplyCore(ref MessageRpc rpc, ref Exception exception, ref bool thereIsAnUnhandledException)
        {
            int messageInspectorCorrelationOffset = this.MessageInspectorCorrelationOffset;
            for (int i = 0; i < this.messageInspectors.Length; i++)
            {
                try
                {
                    Message reply = rpc.Reply;
                    Message message2 = reply;
                    this.messageInspectors[i].BeforeSendReply(ref message2, rpc.Correlation[messageInspectorCorrelationOffset + i]);
                    if (TD.MessageInspectorBeforeSendInvokedIsEnabled())
                    {
                        TD.MessageInspectorBeforeSendInvoked(this.messageInspectors[i].GetType().FullName);
                    }
                    if ((message2 == null) && (reply != null))
                    {
                        object[] args = new object[] { this.messageInspectors[i].GetType().ToString(), rpc.Operation.Name ?? "" };
                        System.ServiceModel.Dispatcher.ErrorBehavior.ThrowAndCatch(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNullReplyFromExtension2", args)));
                    }
                    rpc.Reply = message2;
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    if (!System.ServiceModel.Dispatcher.ErrorBehavior.ShouldRethrowExceptionAsIs(exception2))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception2);
                    }
                    if (exception == null)
                    {
                        exception = exception2;
                    }
                    thereIsAnUnhandledException = !this.error.HandleError(exception2) ? true : thereIsAnUnhandledException;
                }
            }
        }

        private void BeginFinalizeCorrelation(ref MessageRpc rpc)
        {
            CorrelationCallbackMessageProperty property;
            Message reply = rpc.Reply;
            if ((((reply != null) && (rpc.Error == null)) && (((rpc.transaction == null) || (rpc.transaction.Current == null)) || (rpc.transaction.Current.TransactionInformation.Status == TransactionStatus.Active))) && CorrelationCallbackMessageProperty.TryGet(reply, out property))
            {
                if (property.IsFullyDefined)
                {
                    bool flag = false;
                    try
                    {
                        try
                        {
                            rpc.RequestContextThrewOnReply = true;
                            rpc.CorrelationCallback = property;
                            IResumeMessageRpc state = rpc.Pause();
                            rpc.AsyncResult = rpc.CorrelationCallback.BeginFinalizeCorrelation(reply, rpc.ReplyTimeoutHelper.RemainingTime(), onFinalizeCorrelationCompleted, state);
                            flag = true;
                            if (rpc.AsyncResult.CompletedSynchronously)
                            {
                                rpc.UnPause();
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            if (!this.error.HandleError(exception))
                            {
                                rpc.CorrelationCallback = null;
                                rpc.CanSendReply = false;
                            }
                        }
                        return;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            rpc.UnPause();
                        }
                    }
                }
                rpc.CorrelationCallback = new RpcCorrelationCallbackMessageProperty(property, this, ref rpc);
                reply.Properties[CorrelationCallbackMessageProperty.Name] = rpc.CorrelationCallback;
            }
        }

        private void BeginReply(ref MessageRpc rpc)
        {
            bool flag = false;
            try
            {
                IResumeMessageRpc state = rpc.Pause();
                rpc.AsyncResult = rpc.RequestContext.BeginReply(rpc.Reply, rpc.ReplyTimeoutHelper.RemainingTime(), onReplyCompleted, state);
                flag = true;
                if (rpc.AsyncResult.CompletedSynchronously)
                {
                    rpc.UnPause();
                }
            }
            catch (CommunicationException exception)
            {
                this.error.HandleError(exception);
            }
            catch (TimeoutException exception2)
            {
                this.error.HandleError(exception2);
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x80034, System.ServiceModel.SR.GetString("TraceCodeServiceOperationExceptionOnReply"), this, exception3);
                }
                if (!this.error.HandleError(exception3))
                {
                    rpc.RequestContextThrewOnReply = true;
                    rpc.CanSendReply = false;
                }
            }
            finally
            {
                if (!flag)
                {
                    rpc.UnPause();
                }
            }
        }

        internal bool Dispatch(ref MessageRpc rpc, bool isOperationContextSet)
        {
            rpc.ErrorProcessor = this.processMessage8;
            rpc.NextProcessor = this.processMessage1;
            return rpc.Process(isOperationContextSet);
        }

        private void EndFinalizeCorrelation(ref MessageRpc rpc)
        {
            try
            {
                rpc.Reply = rpc.CorrelationCallback.EndFinalizeCorrelation(rpc.AsyncResult);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!this.error.HandleError(exception))
                {
                    rpc.CanSendReply = false;
                }
            }
        }

        private bool EndReply(ref MessageRpc rpc)
        {
            bool flag = false;
            try
            {
                rpc.RequestContext.EndReply(rpc.AsyncResult);
                rpc.RequestContextThrewOnReply = false;
                flag = true;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.error.HandleError(exception);
            }
            return flag;
        }

        private void FinalizeCorrelation(ref MessageRpc rpc)
        {
            CorrelationCallbackMessageProperty property;
            Message reply = rpc.Reply;
            if ((((reply != null) && (rpc.Error == null)) && (((rpc.transaction == null) || (rpc.transaction.Current == null)) || (rpc.transaction.Current.TransactionInformation.Status == TransactionStatus.Active))) && CorrelationCallbackMessageProperty.TryGet(reply, out property))
            {
                if (property.IsFullyDefined)
                {
                    try
                    {
                        rpc.RequestContextThrewOnReply = true;
                        rpc.CorrelationCallback = property;
                        rpc.Reply = rpc.CorrelationCallback.FinalizeCorrelation(reply, rpc.ReplyTimeoutHelper.RemainingTime());
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!this.error.HandleError(exception))
                        {
                            rpc.CorrelationCallback = null;
                            rpc.CanSendReply = false;
                        }
                    }
                }
                else
                {
                    rpc.CorrelationCallback = new RpcCorrelationCallbackMessageProperty(property, this, ref rpc);
                    reply.Properties[CorrelationCallbackMessageProperty.Name] = rpc.CorrelationCallback;
                }
            }
        }

        internal DispatchOperationRuntime GetOperation(ref Message message)
        {
            return this.demuxer.GetOperation(ref message);
        }

        internal static void GotDynamicInstanceContext(object state)
        {
            bool flag;
            ((IResumeMessageRpc) state).Resume(out flag);
        }

        internal void InputSessionDoneReceiving(ServiceChannel channel)
        {
            if (this.inputSessionShutdownHandlers.Length > 0)
            {
                this.InputSessionDoneReceivingCore(channel);
            }
        }

        private void InputSessionDoneReceivingCore(ServiceChannel channel)
        {
            IDuplexContextChannel proxy = channel.Proxy as IDuplexContextChannel;
            if (proxy != null)
            {
                IInputSessionShutdown[] inputSessionShutdownHandlers = this.inputSessionShutdownHandlers;
                try
                {
                    for (int i = 0; i < inputSessionShutdownHandlers.Length; i++)
                    {
                        inputSessionShutdownHandlers[i].DoneReceiving(proxy);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!this.error.HandleError(exception))
                    {
                        proxy.Abort();
                    }
                }
            }
        }

        internal void InputSessionFaulted(ServiceChannel channel)
        {
            if (this.inputSessionShutdownHandlers.Length > 0)
            {
                this.InputSessionFaultedCore(channel);
            }
        }

        private void InputSessionFaultedCore(ServiceChannel channel)
        {
            IDuplexContextChannel proxy = channel.Proxy as IDuplexContextChannel;
            if (proxy != null)
            {
                IInputSessionShutdown[] inputSessionShutdownHandlers = this.inputSessionShutdownHandlers;
                try
                {
                    for (int i = 0; i < inputSessionShutdownHandlers.Length; i++)
                    {
                        inputSessionShutdownHandlers[i].ChannelFaulted(proxy);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!this.error.HandleError(exception))
                    {
                        proxy.Abort();
                    }
                }
            }
        }

        internal bool IsConcurrent(ref MessageRpc rpc)
        {
            return this.concurrency.IsConcurrent(ref rpc);
        }

        private static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                IResumeMessageRpc asyncState = result.AsyncState as IResumeMessageRpc;
                if (asyncState == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxInvalidAsyncResultState0"));
                }
                asyncState.Resume(result);
            }
        }

        private static void OnReplyCompletedCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                IResumeMessageRpc asyncState = result.AsyncState as IResumeMessageRpc;
                if (asyncState == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxInvalidAsyncResultState0"));
                }
                asyncState.Resume(result);
            }
        }

        private bool PrepareAndAddressReply(ref MessageRpc rpc)
        {
            bool flag = true;
            if (!this.manualAddressing)
            {
                if (!object.ReferenceEquals(rpc.RequestID, null))
                {
                    System.ServiceModel.Channels.RequestReplyCorrelator.PrepareReply(rpc.Reply, rpc.RequestID);
                }
                if (!rpc.Channel.HasSession)
                {
                    flag = System.ServiceModel.Channels.RequestReplyCorrelator.AddressReply(rpc.Reply, rpc.ReplyToInfo);
                }
            }
            this.AddMessageProperties(rpc.Reply, rpc.OperationContext, rpc.Channel);
            return flag;
        }

        private void PrepareReply(ref MessageRpc rpc)
        {
            RequestContext requestContext = rpc.OperationContext.RequestContext;
            Exception exception = null;
            bool thereIsAnUnhandledException = false;
            if (!rpc.Operation.IsOneWay)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    if ((rpc.Reply == null) && (requestContext != null))
                    {
                        object[] args = new object[] { rpc.Operation.Name ?? string.Empty };
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x80032, System.ServiceModel.SR.GetString("TraceCodeServiceOperationMissingReply", args), (Exception) null, (Message) null);
                    }
                    else if ((requestContext == null) && (rpc.Reply != null))
                    {
                        object[] objArray2 = new object[] { rpc.Operation.Name ?? string.Empty };
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x80033, System.ServiceModel.SR.GetString("TraceCodeServiceOperationMissingReplyContext", objArray2), (Exception) null, (Message) null);
                    }
                }
                if ((requestContext != null) && (rpc.Reply != null))
                {
                    try
                    {
                        rpc.CanSendReply = this.PrepareAndAddressReply(ref rpc);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        thereIsAnUnhandledException = !this.error.HandleError(exception2) || thereIsAnUnhandledException;
                        exception = exception2;
                    }
                }
            }
            this.BeforeSendReply(ref rpc, ref exception, ref thereIsAnUnhandledException);
            if (rpc.Operation.IsOneWay)
            {
                rpc.CanSendReply = false;
            }
            if ((!rpc.Operation.IsOneWay && (requestContext != null)) && (rpc.Reply != null))
            {
                if (exception == null)
                {
                    return;
                }
                rpc.Error = exception;
                this.error.ProvideOnlyFaultOfLastResort(ref rpc);
                try
                {
                    rpc.CanSendReply = this.PrepareAndAddressReply(ref rpc);
                    return;
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    this.error.HandleError(exception3);
                    return;
                }
            }
            if ((exception != null) && thereIsAnUnhandledException)
            {
                rpc.Abort();
            }
        }

        internal void ProcessMessage1(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage11;
            if (this.receiveContextEnabledChannel)
            {
                ReceiveContextRPCFacet.CreateIfRequired(this, ref rpc);
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessage11(ref rpc);
            }
            else if ((this.isOnServer && DiagnosticUtility.ShouldTraceInformation) && !this.didTraceProcessMessage1)
            {
                this.didTraceProcessMessage1 = true;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80027, System.ServiceModel.SR.GetString("TraceCodeProcessMessage31Paused", new object[] { rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName, rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress }));
            }
        }

        internal void ProcessMessage11(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage2;
            if (rpc.Operation.IsOneWay)
            {
                rpc.RequestContext.Reply(null);
                rpc.OperationContext.RequestContext = null;
            }
            else
            {
                if ((!rpc.Channel.IsReplyChannel && (rpc.RequestID == null)) && (rpc.Operation.Action != "*"))
                {
                    CommunicationException exception = new CommunicationException(System.ServiceModel.SR.GetString("SFxOneWayMessageToTwoWayMethod0"));
                    throw TraceUtility.ThrowHelperError(exception, rpc.Request);
                }
                if (!this.manualAddressing)
                {
                    EndpointAddress replyTo = rpc.ReplyToInfo.ReplyTo;
                    if (((replyTo != null) && replyTo.IsNone) && rpc.Channel.IsReplyChannel)
                    {
                        CommunicationException exception2 = new CommunicationException(System.ServiceModel.SR.GetString("SFxRequestReplyNone"));
                        throw TraceUtility.ThrowHelperError(exception2, rpc.Request);
                    }
                    if (this.isOnServer)
                    {
                        EndpointAddress remoteAddress = rpc.Channel.RemoteAddress;
                        if ((remoteAddress != null) && !remoteAddress.IsAnonymous)
                        {
                            MessageHeaders headers = rpc.Request.Headers;
                            Uri uri = remoteAddress.Uri;
                            if (((replyTo != null) && !replyTo.IsAnonymous) && (uri != replyTo.Uri))
                            {
                                Exception exception3 = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRequestHasInvalidReplyToOnServer", new object[] { replyTo.Uri, uri }));
                                throw TraceUtility.ThrowHelperError(exception3, rpc.Request);
                            }
                            EndpointAddress faultTo = headers.FaultTo;
                            if (((faultTo != null) && !faultTo.IsAnonymous) && (uri != faultTo.Uri))
                            {
                                Exception exception4 = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRequestHasInvalidFaultToOnServer", new object[] { faultTo.Uri, uri }));
                                throw TraceUtility.ThrowHelperError(exception4, rpc.Request);
                            }
                            if (rpc.RequestVersion.Addressing == AddressingVersion.WSAddressingAugust2004)
                            {
                                EndpointAddress from = headers.From;
                                if (((from != null) && !from.IsAnonymous) && (uri != from.Uri))
                                {
                                    Exception exception5 = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRequestHasInvalidFromOnServer", new object[] { from.Uri, uri }));
                                    throw TraceUtility.ThrowHelperError(exception5, rpc.Request);
                                }
                            }
                        }
                    }
                }
            }
            if (this.concurrency.IsConcurrent(ref rpc))
            {
                rpc.Channel.IncrementActivity();
                rpc.SuccessfullyIncrementedActivity = true;
            }
            if (this.authenticationBehavior != null)
            {
                this.authenticationBehavior.Authenticate(ref rpc);
            }
            if (this.authorizationBehavior != null)
            {
                this.authorizationBehavior.Authorize(ref rpc);
            }
            this.instance.EnsureInstanceContext(ref rpc);
            this.TransferChannelFromPendingList(ref rpc);
            this.AcquireDynamicInstanceContext(ref rpc);
            if (!rpc.IsPaused)
            {
                this.ProcessMessage2(ref rpc);
            }
        }

        private void ProcessMessage2(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage3;
            this.AfterReceiveRequest(ref rpc);
            if (!this.ignoreTransactionFlow)
            {
                rpc.TransactionMessageProperty = TransactionMessageProperty.TryGet(rpc.Request);
            }
            this.concurrency.LockInstance(ref rpc);
            if (!rpc.IsPaused)
            {
                this.ProcessMessage3(ref rpc);
            }
            else if ((this.isOnServer && DiagnosticUtility.ShouldTraceInformation) && !this.didTraceProcessMessage2)
            {
                this.didTraceProcessMessage2 = true;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80027, System.ServiceModel.SR.GetString("TraceCodeProcessMessage2Paused", new object[] { rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName, rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress }));
            }
        }

        private void ProcessMessage3(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage31;
            rpc.SuccessfullyLockedInstance = true;
            if (this.transaction != null)
            {
                this.transaction.ResolveTransaction(ref rpc);
                if (rpc.Operation.TransactionRequired)
                {
                    this.transaction.SetCurrent(ref rpc);
                }
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessage31(ref rpc);
            }
            else if ((this.isOnServer && DiagnosticUtility.ShouldTraceInformation) && !this.didTraceProcessMessage3)
            {
                this.didTraceProcessMessage3 = true;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80027, System.ServiceModel.SR.GetString("TraceCodeProcessMessage3Paused", new object[] { rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName, rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress }));
            }
        }

        private void ProcessMessage31(ref MessageRpc rpc)
        {
            rpc.NextProcessor = new MessageRpcProcessor(this.ProcessMessage4);
            if ((this.transaction != null) && rpc.Operation.TransactionRequired)
            {
                ReceiveContextRPCFacet receiveContext = rpc.ReceiveContext;
                if (receiveContext != null)
                {
                    rpc.ReceiveContext = null;
                    receiveContext.Complete(this, ref rpc, TimeSpan.MaxValue, rpc.Transaction.Current);
                }
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessage4(ref rpc);
            }
            else if ((this.isOnServer && DiagnosticUtility.ShouldTraceInformation) && !this.didTraceProcessMessage31)
            {
                this.didTraceProcessMessage31 = true;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80027, System.ServiceModel.SR.GetString("TraceCodeProcessMessage31Paused", new object[] { rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName, rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress }));
            }
        }

        private void ProcessMessage4(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage41;
            try
            {
                this.thread.BindThread(ref rpc);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessage41(ref rpc);
            }
            else if ((this.isOnServer && DiagnosticUtility.ShouldTraceInformation) && !this.didTraceProcessMessage4)
            {
                this.didTraceProcessMessage4 = true;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80027, System.ServiceModel.SR.GetString("TraceCodeProcessMessage4Paused", new object[] { rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName, rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress }));
            }
        }

        private void ProcessMessage41(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage5;
            if (this.concurrency.IsConcurrent(ref rpc) && !(rpc.Operation.Invoker is IManualConcurrencyOperationInvoker))
            {
                rpc.EnsureReceive();
            }
            this.instance.EnsureServiceInstance(ref rpc);
            if (!rpc.IsPaused)
            {
                this.ProcessMessage5(ref rpc);
            }
            else if ((this.isOnServer && DiagnosticUtility.ShouldTraceInformation) && !this.didTraceProcessMessage41)
            {
                this.didTraceProcessMessage41 = true;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80027, System.ServiceModel.SR.GetString("TraceCodeProcessMessage4Paused", new object[] { rpc.Channel.DispatchRuntime.EndpointDispatcher.ContractName, rpc.Channel.DispatchRuntime.EndpointDispatcher.EndpointAddress }));
            }
        }

        private void ProcessMessage5(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage6;
            try
            {
                bool flag = false;
                try
                {
                    if (!rpc.Operation.IsSynchronous)
                    {
                        rpc.PrepareInvokeContinueGate();
                    }
                    if (this.transaction != null)
                    {
                        this.transaction.InitializeCallContext(ref rpc);
                    }
                    rpc.Operation.InvokeBegin(ref rpc);
                    flag = true;
                }
                finally
                {
                    try
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.ClearCallContext(ref rpc);
                            }
                        }
                        finally
                        {
                            if ((!rpc.Operation.IsSynchronous && rpc.IsPaused) && rpc.UnlockInvokeContinueGate(out rpc.AsyncResult))
                            {
                                rpc.UnPause();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (flag && (rpc.Operation.IsSynchronous || !rpc.IsPaused))
                        {
                            throw;
                        }
                        this.error.HandleError(exception);
                    }
                }
            }
            catch
            {
                throw;
            }
            if (!rpc.IsPaused)
            {
                if (rpc.Operation.IsSynchronous)
                {
                    this.ProcessMessage8(ref rpc);
                }
                else
                {
                    this.ProcessMessage6(ref rpc);
                }
            }
        }

        private void ProcessMessage6(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage7;
            try
            {
                this.thread.BindEndThread(ref rpc);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessage7(ref rpc);
            }
        }

        private void ProcessMessage7(ref MessageRpc rpc)
        {
            rpc.NextProcessor = null;
            try
            {
                bool flag = false;
                try
                {
                    if (this.transaction != null)
                    {
                        this.transaction.InitializeCallContext(ref rpc);
                    }
                    rpc.Operation.InvokeEnd(ref rpc);
                    flag = true;
                }
                finally
                {
                    try
                    {
                        if (this.transaction != null)
                        {
                            this.transaction.ClearCallContext(ref rpc);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (flag)
                        {
                            throw;
                        }
                        this.error.HandleError(exception);
                    }
                }
            }
            catch
            {
                throw;
            }
            this.ProcessMessage8(ref rpc);
        }

        private void ProcessMessage8(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessage9;
            try
            {
                this.error.ProvideMessageFault(ref rpc);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.error.HandleError(exception);
            }
            this.PrepareReply(ref rpc);
            if (rpc.CanSendReply)
            {
                rpc.ReplyTimeoutHelper = new TimeoutHelper(rpc.Channel.OperationTimeout);
                if (this.sendAsynchronously)
                {
                    this.BeginFinalizeCorrelation(ref rpc);
                }
                else
                {
                    this.FinalizeCorrelation(ref rpc);
                }
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessage9(ref rpc);
            }
        }

        private void ProcessMessage9(ref MessageRpc rpc)
        {
            rpc.NextProcessor = this.processMessageCleanup;
            if (rpc.FinalizeCorrelationImplicitly && this.sendAsynchronously)
            {
                this.EndFinalizeCorrelation(ref rpc);
            }
            if ((rpc.CorrelationCallback == null) || rpc.FinalizeCorrelationImplicitly)
            {
                this.ResolveTransactionOutcome(ref rpc);
            }
            if (rpc.CanSendReply)
            {
                TraceUtility.MessageFlowAtMessageSent(rpc.Reply);
                if (this.sendAsynchronously)
                {
                    this.BeginReply(ref rpc);
                }
                else
                {
                    this.Reply(ref rpc);
                }
            }
            if (!rpc.IsPaused)
            {
                this.ProcessMessageCleanup(ref rpc);
            }
        }

        private void ProcessMessageCleanup(ref MessageRpc rpc)
        {
            rpc.ErrorProcessor = this.processMessageCleanupError;
            bool successfullySendReply = false;
            if (rpc.CanSendReply)
            {
                if (this.sendAsynchronously)
                {
                    successfullySendReply = this.EndReply(ref rpc);
                }
                else
                {
                    successfullySendReply = rpc.SuccessfullySendReply;
                }
            }
            try
            {
                try
                {
                    if (rpc.DidDeserializeRequestBody)
                    {
                        rpc.Request.Close();
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.error.HandleError(exception);
                }
                if (rpc.HostingProperty != null)
                {
                    try
                    {
                        rpc.HostingProperty.Close();
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
                IManualConcurrencyOperationInvoker invoker = rpc.Operation.Invoker as IManualConcurrencyOperationInvoker;
                rpc.DisposeParameters((invoker != null) && invoker.OwnsFormatter);
                if (rpc.FaultInfo.IsConsideredUnhandled)
                {
                    if (!successfullySendReply)
                    {
                        rpc.AbortRequestContext();
                        rpc.AbortChannel();
                    }
                    else
                    {
                        rpc.CloseRequestContext();
                        rpc.CloseChannel();
                    }
                    rpc.AbortInstanceContext();
                }
                else if (rpc.RequestContextThrewOnReply)
                {
                    rpc.AbortRequestContext();
                }
                else
                {
                    rpc.CloseRequestContext();
                }
                if ((rpc.Reply != null) && (rpc.Reply != rpc.ReturnParameter))
                {
                    try
                    {
                        rpc.Reply.Close();
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        this.error.HandleError(exception3);
                    }
                }
                if ((rpc.FaultInfo.Fault != null) && (rpc.FaultInfo.Fault.State != MessageState.Closed))
                {
                    try
                    {
                        rpc.FaultInfo.Fault.Close();
                    }
                    catch (Exception exception4)
                    {
                        if (Fx.IsFatal(exception4))
                        {
                            throw;
                        }
                        this.error.HandleError(exception4);
                    }
                }
                try
                {
                    rpc.OperationContext.FireOperationCompleted();
                }
                catch (Exception exception5)
                {
                    if (Fx.IsFatal(exception5))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception5);
                }
                this.instance.AfterReply(ref rpc, this.error);
                if (rpc.SuccessfullyLockedInstance)
                {
                    try
                    {
                        this.concurrency.UnlockInstance(ref rpc);
                    }
                    catch (Exception exception6)
                    {
                        if (Fx.IsFatal(exception6))
                        {
                            throw;
                        }
                        rpc.InstanceContext.FaultInternal();
                        this.error.HandleError(exception6);
                    }
                }
                if (this.terminate != null)
                {
                    try
                    {
                        this.terminate.AfterReply(ref rpc);
                    }
                    catch (Exception exception7)
                    {
                        if (Fx.IsFatal(exception7))
                        {
                            throw;
                        }
                        this.error.HandleError(exception7);
                    }
                }
                if (rpc.SuccessfullyIncrementedActivity)
                {
                    try
                    {
                        rpc.Channel.DecrementActivity();
                    }
                    catch (Exception exception8)
                    {
                        if (Fx.IsFatal(exception8))
                        {
                            throw;
                        }
                        this.error.HandleError(exception8);
                    }
                }
            }
            finally
            {
                if (rpc.MessageRpcOwnsInstanceContextThrottle && (rpc.channelHandler.InstanceContextServiceThrottle != null))
                {
                    rpc.channelHandler.InstanceContextServiceThrottle.DeactivateInstanceContext();
                }
                if ((rpc.Activity != null) && DiagnosticUtility.ShouldUseActivity)
                {
                    rpc.Activity.Stop();
                }
            }
            this.error.HandleError(ref rpc);
        }

        private void ProcessMessageCleanupError(ref MessageRpc rpc)
        {
            this.error.HandleError(ref rpc);
        }

        private void Reply(ref MessageRpc rpc)
        {
            rpc.RequestContextThrewOnReply = true;
            rpc.SuccessfullySendReply = false;
            try
            {
                rpc.RequestContext.Reply(rpc.Reply, rpc.ReplyTimeoutHelper.RemainingTime());
                rpc.RequestContextThrewOnReply = false;
                rpc.SuccessfullySendReply = true;
            }
            catch (CommunicationException exception)
            {
                this.error.HandleError(exception);
            }
            catch (TimeoutException exception2)
            {
                this.error.HandleError(exception2);
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x80034, System.ServiceModel.SR.GetString("TraceCodeServiceOperationExceptionOnReply"), this, exception3);
                }
                if (!this.error.HandleError(exception3))
                {
                    rpc.RequestContextThrewOnReply = true;
                    rpc.CanSendReply = false;
                }
            }
        }

        private void ResolveTransactionOutcome(ref MessageRpc rpc)
        {
            if (this.transaction != null)
            {
                try
                {
                    bool flag = rpc.Error != null;
                    try
                    {
                        this.transaction.ResolveOutcome(ref rpc);
                    }
                    catch (FaultException exception)
                    {
                        if (rpc.Error == null)
                        {
                            rpc.Error = exception;
                        }
                    }
                    finally
                    {
                        if (!flag && (rpc.Error != null))
                        {
                            this.error.ProvideMessageFault(ref rpc);
                            this.PrepareAndAddressReply(ref rpc);
                        }
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    this.error.HandleError(exception2);
                }
            }
        }

        private void TransferChannelFromPendingList(ref MessageRpc rpc)
        {
            if (rpc.Channel.IsPending)
            {
                rpc.Channel.IsPending = false;
                ChannelDispatcher channelDispatcher = rpc.Channel.ChannelDispatcher;
                IInstanceContextProvider instanceContextProvider = this.instance.InstanceContextProvider;
                if (!InstanceContextProviderBase.IsProviderSessionful(instanceContextProvider) && !InstanceContextProviderBase.IsProviderSingleton(instanceContextProvider))
                {
                    IChannel proxy = rpc.Channel.Proxy as IChannel;
                    if (!rpc.InstanceContext.IncomingChannels.Contains(proxy))
                    {
                        channelDispatcher.Channels.Add(proxy);
                    }
                }
                channelDispatcher.PendingChannels.Remove(rpc.Channel.Binder.Channel);
            }
        }

        internal int CallContextCorrelationOffset
        {
            get
            {
                return this.messageInspectors.Length;
            }
        }

        internal int CorrelationCount
        {
            get
            {
                return this.correlationCount;
            }
        }

        internal bool EnableFaults
        {
            get
            {
                return this.enableFaults;
            }
        }

        internal System.ServiceModel.Dispatcher.ErrorBehavior ErrorBehavior
        {
            get
            {
                return this.error;
            }
        }

        internal System.ServiceModel.Dispatcher.InstanceBehavior InstanceBehavior
        {
            get
            {
                return this.instance;
            }
        }

        internal bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        internal int MessageInspectorCorrelationOffset
        {
            get
            {
                return 0;
            }
        }

        internal int ParameterInspectorCorrelationOffset
        {
            get
            {
                return this.parameterInspectorCorrelationOffset;
            }
        }

        internal IRequestReplyCorrelator RequestReplyCorrelator
        {
            get
            {
                return this.requestReplyCorrelator;
            }
        }

        internal SecurityImpersonationBehavior SecurityImpersonation
        {
            get
            {
                return this.securityImpersonation;
            }
        }

        internal bool ValidateMustUnderstand
        {
            get
            {
                return this.validateMustUnderstand;
            }
        }

        private class ActionDemuxer : ImmutableDispatchRuntime.IDemuxer
        {
            private HybridDictionary map = new HybridDictionary();
            private DispatchOperationRuntime unhandled;

            internal ActionDemuxer()
            {
            }

            internal void Add(string action, DispatchOperationRuntime operation)
            {
                if (this.map.Contains(action))
                {
                    DispatchOperationRuntime runtime = (DispatchOperationRuntime) this.map[action];
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxActionDemuxerDuplicate", new object[] { runtime.Name, operation.Name, action })));
                }
                this.map.Add(action, operation);
            }

            public DispatchOperationRuntime GetOperation(ref Message request)
            {
                string action = request.Headers.Action;
                if (action == null)
                {
                    action = "*";
                }
                DispatchOperationRuntime runtime = (DispatchOperationRuntime) this.map[action];
                if (runtime != null)
                {
                    return runtime;
                }
                return this.unhandled;
            }

            internal void SetUnhandled(DispatchOperationRuntime operation)
            {
                this.unhandled = operation;
            }
        }

        private class CustomDemuxer : ImmutableDispatchRuntime.IDemuxer
        {
            private Dictionary<string, DispatchOperationRuntime> map;
            private IDispatchOperationSelector selector;
            private DispatchOperationRuntime unhandled;

            internal CustomDemuxer(IDispatchOperationSelector selector)
            {
                this.selector = selector;
                this.map = new Dictionary<string, DispatchOperationRuntime>();
            }

            internal void Add(string name, DispatchOperationRuntime operation)
            {
                this.map.Add(name, operation);
            }

            public DispatchOperationRuntime GetOperation(ref Message request)
            {
                string key = this.selector.SelectOperation(ref request);
                DispatchOperationRuntime runtime = null;
                if (this.map.TryGetValue(key, out runtime))
                {
                    return runtime;
                }
                return this.unhandled;
            }

            internal void SetUnhandled(DispatchOperationRuntime operation)
            {
                this.unhandled = operation;
            }
        }

        private interface IDemuxer
        {
            DispatchOperationRuntime GetOperation(ref Message request);
        }

        private class RpcCorrelationCallbackMessageProperty : CorrelationCallbackMessageProperty
        {
            private CorrelationCallbackMessageProperty innerCallback;
            private MessageRpc rpc;
            private ImmutableDispatchRuntime runtime;
            private TransactionScope scope;

            public RpcCorrelationCallbackMessageProperty(ImmutableDispatchRuntime.RpcCorrelationCallbackMessageProperty rpcCallbackMessageProperty) : base(rpcCallbackMessageProperty)
            {
                this.innerCallback = rpcCallbackMessageProperty.innerCallback;
                this.runtime = rpcCallbackMessageProperty.runtime;
                this.rpc = rpcCallbackMessageProperty.rpc;
            }

            public RpcCorrelationCallbackMessageProperty(CorrelationCallbackMessageProperty innerCallback, ImmutableDispatchRuntime runtime, ref MessageRpc rpc) : base(innerCallback)
            {
                this.innerCallback = innerCallback;
                this.runtime = runtime;
                this.rpc = rpc;
            }

            private void CompleteTransaction()
            {
                this.runtime.ResolveTransactionOutcome(ref this.rpc);
            }

            public override IMessageProperty CreateCopy()
            {
                return new ImmutableDispatchRuntime.RpcCorrelationCallbackMessageProperty(this);
            }

            private void Enter()
            {
                if ((this.rpc.transaction != null) && (this.rpc.transaction.Current != null))
                {
                    this.scope = new TransactionScope(this.rpc.transaction.Current);
                }
            }

            private void Leave(bool complete)
            {
                if (this.scope != null)
                {
                    if (complete)
                    {
                        this.scope.Complete();
                    }
                    this.scope.Dispose();
                    this.scope = null;
                }
            }

            protected override IAsyncResult OnBeginFinalizeCorrelation(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                IAsyncResult result2;
                bool complete = false;
                this.Enter();
                try
                {
                    IAsyncResult result = this.innerCallback.BeginFinalizeCorrelation(message, timeout, callback, state);
                    complete = true;
                    result2 = result;
                }
                finally
                {
                    this.Leave(complete);
                }
                return result2;
            }

            protected override Message OnEndFinalizeCorrelation(IAsyncResult result)
            {
                Message message2;
                bool complete = false;
                this.Enter();
                try
                {
                    Message message = this.innerCallback.EndFinalizeCorrelation(result);
                    complete = true;
                    message2 = message;
                }
                finally
                {
                    this.Leave(complete);
                    this.CompleteTransaction();
                }
                return message2;
            }

            protected override Message OnFinalizeCorrelation(Message message, TimeSpan timeout)
            {
                Message message3;
                bool complete = false;
                this.Enter();
                try
                {
                    Message message2 = this.innerCallback.FinalizeCorrelation(message, timeout);
                    complete = true;
                    message3 = message2;
                }
                finally
                {
                    this.Leave(complete);
                    this.CompleteTransaction();
                }
                return message3;
            }
        }
    }
}

