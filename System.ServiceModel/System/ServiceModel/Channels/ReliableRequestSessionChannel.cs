namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;
    using System.Xml;

    internal sealed class ReliableRequestSessionChannel : RequestChannel, IRequestSessionChannel, IRequestChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
    {
        private IClientReliableChannelBinder binder;
        private ChannelParameterCollection channelParameters;
        private ReliableRequestor closeRequestor;
        private ReliableOutputConnection connection;
        private bool isLastKnown;
        private Exception maxRetryCountException;
        private static AsyncCallback onPollingComplete = Fx.ThunkCallback(new AsyncCallback(ReliableRequestSessionChannel.OnPollingComplete));
        private SequenceRangeCollection ranges;
        private Guard replyAckConsistencyGuard;
        private ClientReliableSession session;
        private IReliableFactorySettings settings;
        private InterruptibleWaitObject shutdownHandle;
        private ReliableRequestor terminateRequestor;

        public ReliableRequestSessionChannel(ChannelManagerBase factory, IReliableFactorySettings settings, IClientReliableChannelBinder binder, FaultHelper faultHelper, LateBoundChannelParameterCollection channelParameters, UniqueId inputID) : base(factory, binder.RemoteAddress, binder.Via, true)
        {
            this.ranges = SequenceRangeCollection.Empty;
            this.settings = settings;
            this.binder = binder;
            this.session = new ClientReliableSession(this, settings, binder, faultHelper, inputID);
            this.session.PollingCallback = new ClientReliableSession.PollingHandler(this.PollingCallback);
            this.session.UnblockChannelCloseCallback = new ChannelReliableSession.UnblockChannelCloseHandler(this.UnblockClose);
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.shutdownHandle = new InterruptibleWaitObject(false);
            }
            else
            {
                this.replyAckConsistencyGuard = new Guard(0x7fffffff);
            }
            this.binder.Faulted += new BinderExceptionHandler(this.OnBinderFaulted);
            this.binder.OnException += new BinderExceptionHandler(this.OnBinderException);
            this.channelParameters = channelParameters;
            channelParameters.SetChannel(this);
        }

        private void AddAcknowledgementHeader(Message message, bool force)
        {
            if (this.ranges.Count != 0)
            {
                WsrmUtilities.AddAcknowledgementHeader(this.settings.ReliableMessagingVersion, message, this.session.InputID, this.ranges, this.isLastKnown);
            }
        }

        private IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        private IAsyncResult BeginCloseSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CreateCloseRequestor();
            return this.closeRequestor.BeginRequest(timeout, callback, state);
        }

        private IAsyncResult BeginSendAckRequestedMessage(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
        {
            this.session.OnLocalActivity();
            ReliableBinderRequestAsyncResult result = new ReliableBinderRequestAsyncResult(callback, state) {
                Binder = this.binder,
                MaskingMode = maskingMode,
                Message = this.CreateAckRequestedMessage()
            };
            result.Begin(timeout);
            return result;
        }

        private IAsyncResult BeginTerminateSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CreateTerminateRequestor();
            return this.terminateRequestor.BeginRequest(timeout, callback, state);
        }

        private IAsyncResult BeginWaitForShutdown(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return this.shutdownHandle.BeginWait(timeout, callback, state);
            }
            this.isLastKnown = true;
            return this.replyAckConsistencyGuard.BeginClose(timeout, callback, state);
        }

        private void CloseSequence(TimeSpan timeout)
        {
            this.CreateCloseRequestor();
            Message reply = this.closeRequestor.Request(timeout);
            this.ProcessCloseOrTerminateReply(true, reply);
        }

        private void ConfigureRequestor(ReliableRequestor requestor)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            requestor.MessageVersion = this.settings.MessageVersion;
            requestor.Binder = this.binder;
            requestor.SetRequestResponsePattern();
            requestor.MessageHeader = new WsrmAcknowledgmentHeader(reliableMessagingVersion, this.session.InputID, this.ranges, true, -1);
        }

        private Message CreateAckRequestedMessage()
        {
            Message message = WsrmUtilities.CreateAckRequestedMessage(this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.session.OutputID);
            this.AddAcknowledgementHeader(message, true);
            return message;
        }

        protected override IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state)
        {
            return new AsyncRequest(this, callback, state);
        }

        private void CreateCloseRequestor()
        {
            RequestReliableRequestor requestor = new RequestReliableRequestor();
            this.ConfigureRequestor(requestor);
            requestor.TimeoutString1Index = "TimeoutOnClose";
            requestor.MessageAction = WsrmIndex.GetCloseSequenceActionHeader(this.settings.MessageVersion.Addressing);
            requestor.MessageBody = new System.ServiceModel.Channels.CloseSequence(this.session.OutputID, this.connection.Last);
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.closeRequestor = requestor;
            }
        }

        protected override IRequest CreateRequest(Message message)
        {
            return new SyncRequest(this);
        }

        private void CreateTerminateRequestor()
        {
            RequestReliableRequestor requestor = new RequestReliableRequestor();
            this.ConfigureRequestor(requestor);
            requestor.MessageAction = WsrmIndex.GetTerminateSequenceActionHeader(this.settings.MessageVersion.Addressing, this.settings.ReliableMessagingVersion);
            requestor.MessageBody = new System.ServiceModel.Channels.TerminateSequence(this.settings.ReliableMessagingVersion, this.session.OutputID, this.connection.Last);
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.terminateRequestor = requestor;
                this.session.CloseSession();
            }
        }

        private void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        private void EndCloseSequence(IAsyncResult result)
        {
            Message reply = this.closeRequestor.EndRequest(result);
            this.ProcessCloseOrTerminateReply(true, reply);
        }

        private void EndSendAckRequestedMessage(IAsyncResult result)
        {
            Message reply = ReliableBinderRequestAsyncResult.End(result);
            if (reply != null)
            {
                this.ProcessReply(reply, null, 0L);
            }
        }

        private void EndTerminateSequence(IAsyncResult result)
        {
            Message reply = this.terminateRequestor.EndRequest(result);
            if (reply != null)
            {
                this.ProcessCloseOrTerminateReply(false, reply);
            }
        }

        private void EndWaitForShutdown(IAsyncResult result)
        {
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.shutdownHandle.EndWait(result);
            }
            else
            {
                this.replyAckConsistencyGuard.EndClose(result);
            }
        }

        private Exception GetInvalidAddException()
        {
            if (base.State == CommunicationState.Faulted)
            {
                return base.GetTerminalException();
            }
            return base.CreateClosedException();
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IRequestSessionChannel))
            {
                return (T) this;
            }
            if (typeof(T) == typeof(ChannelParameterCollection))
            {
                return (T) this.channelParameters;
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            T local2 = this.binder.Channel.GetProperty<T>();
            if ((local2 == null) && (typeof(T) == typeof(FaultConverter)))
            {
                return (T) FaultConverter.GetDefaultFaultConverter(this.settings.MessageVersion);
            }
            return local2;
        }

        protected override void OnAbort()
        {
            if (this.connection != null)
            {
                this.connection.Abort(this);
            }
            if (this.shutdownHandle != null)
            {
                this.shutdownHandle.Abort(this);
            }
            ReliableRequestor closeRequestor = this.closeRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Abort(this);
            }
            closeRequestor = this.terminateRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Abort(this);
            }
            this.session.Abort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool flag = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.connection.BeginClose), new OperationWithTimeoutBeginCallback(this.BeginWaitForShutdown), flag ? new OperationWithTimeoutBeginCallback(this.BeginCloseSequence) : null, new OperationWithTimeoutBeginCallback(this.BeginTerminateSequence), new OperationWithTimeoutBeginCallback(this.session.BeginClose), new OperationWithTimeoutBeginCallback(this.BeginCloseBinder) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { new OperationEndCallback(this.connection.EndClose), new OperationEndCallback(this.EndWaitForShutdown), flag ? new OperationEndCallback(this.EndCloseSequence) : null, new OperationEndCallback(this.EndTerminateSequence), new OperationEndCallback(this.session.EndClose), new OperationEndCallback(this.EndCloseBinder) };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReliableChannelOpenAsyncResult(this.binder, this.session, timeout, callback, state);
        }

        private void OnBinderException(IReliableChannelBinder sender, Exception exception)
        {
            if (exception is QuotaExceededException)
            {
                if (((base.State == CommunicationState.Opening) || (base.State == CommunicationState.Opened)) || (base.State == CommunicationState.Closing))
                {
                    this.session.OnLocalFault(exception, SequenceTerminatedFault.CreateQuotaExceededFault(this.session.OutputID), null);
                }
            }
            else
            {
                base.AddPendingException(exception);
            }
        }

        private void OnBinderFaulted(IReliableChannelBinder sender, Exception exception)
        {
            this.binder.Abort();
            if (((base.State == CommunicationState.Opening) || (base.State == CommunicationState.Opened)) || (base.State == CommunicationState.Closing))
            {
                exception = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityFaulted"), exception);
                this.session.OnLocalFault(exception, (Message) null, null);
            }
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.connection.Close(helper.RemainingTime());
            this.WaitForShutdown(helper.RemainingTime());
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.CloseSequence(helper.RemainingTime());
            }
            this.TerminateSequence(helper.RemainingTime());
            this.session.Close(helper.RemainingTime());
            this.binder.Close(helper.RemainingTime(), MaskingMode.Handled);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.binder.Faulted -= new BinderExceptionHandler(this.OnBinderFaulted);
        }

        private void OnComponentException(Exception exception)
        {
            this.session.OnUnknownException(exception);
        }

        private void OnComponentFaulted(Exception faultException, WsrmFault fault)
        {
            this.session.OnLocalFault(faultException, fault, null);
        }

        private IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
            {
                this.session.OnLocalFault(new CommunicationException(System.ServiceModel.SR.GetString("MaximumRetryCountExceeded"), this.maxRetryCountException), SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                return new CompletedAsyncResult(callback, state);
            }
            this.session.OnLocalActivity();
            this.AddAcknowledgementHeader(attemptInfo.Message, false);
            ReliableBinderRequestAsyncResult result = new ReliableBinderRequestAsyncResult(callback, state) {
                Binder = this.binder,
                MessageAttemptInfo = attemptInfo,
                MaskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None
            };
            if (attemptInfo.RetryCount < this.settings.MaxRetryCount)
            {
                result.MaskingMode |= MaskingMode.Handled;
                result.SaveHandledException = false;
            }
            else
            {
                result.SaveHandledException = true;
            }
            result.Begin(timeout);
            return result;
        }

        private IAsyncResult OnConnectionBeginSendAckRequested(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        private void OnConnectionEndSend(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                Exception exception;
                Message reply = ReliableBinderRequestAsyncResult.End(result, out exception);
                ReliableBinderRequestAsyncResult result2 = (ReliableBinderRequestAsyncResult) result;
                if (result2.MessageAttemptInfo.RetryCount == this.settings.MaxRetryCount)
                {
                    this.maxRetryCountException = exception;
                }
                if (reply != null)
                {
                    this.ProcessReply(reply, (IReliableRequest) result2.MessageAttemptInfo.State, result2.MessageAttemptInfo.GetSequenceNumber());
                }
            }
        }

        private void OnConnectionEndSendAckRequested(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        private void OnConnectionSend(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException)
        {
            using (attemptInfo.Message)
            {
                if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
                {
                    this.session.OnLocalFault(new CommunicationException(System.ServiceModel.SR.GetString("MaximumRetryCountExceeded"), this.maxRetryCountException), SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                }
                else
                {
                    this.AddAcknowledgementHeader(attemptInfo.Message, false);
                    this.session.OnLocalActivity();
                    Message reply = null;
                    MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;
                    if (attemptInfo.RetryCount < this.settings.MaxRetryCount)
                    {
                        maskingMode |= MaskingMode.Handled;
                        reply = this.binder.Request(attemptInfo.Message, timeout, maskingMode);
                    }
                    else
                    {
                        try
                        {
                            reply = this.binder.Request(attemptInfo.Message, timeout, maskingMode);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            if (!this.binder.IsHandleable(exception))
                            {
                                throw;
                            }
                            this.maxRetryCountException = exception;
                        }
                    }
                    if (reply != null)
                    {
                        this.ProcessReply(reply, (IReliableRequest) attemptInfo.State, attemptInfo.GetSequenceNumber());
                    }
                }
            }
        }

        private void OnConnectionSendAckRequested(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ReliableChannelOpenAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            this.session.OnFaulted();
            this.UnblockClose();
            base.OnFaulted();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            bool flag = true;
            try
            {
                this.binder.Open(helper.RemainingTime());
                this.session.Open(helper.RemainingTime());
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.binder.Close(helper.RemainingTime());
                }
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.connection = new ReliableOutputConnection(this.session.OutputID, this.settings.MaxTransferWindowSize, this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.session.InitiationTime, false, base.DefaultSendTimeout);
            this.connection.Faulted = (ComponentFaultedHandler) Delegate.Combine(this.connection.Faulted, new ComponentFaultedHandler(this.OnComponentFaulted));
            this.connection.OnException = (ComponentExceptionHandler) Delegate.Combine(this.connection.OnException, new ComponentExceptionHandler(this.OnComponentException));
            this.connection.BeginSendHandler = new BeginSendHandler(this.OnConnectionBeginSend);
            this.connection.EndSendHandler = new EndSendHandler(this.OnConnectionEndSend);
            this.connection.SendHandler = new SendHandler(this.OnConnectionSend);
            this.connection.BeginSendAckRequestedHandler = new OperationWithTimeoutBeginCallback(this.OnConnectionBeginSendAckRequested);
            this.connection.EndSendAckRequestedHandler = new OperationEndCallback(this.OnConnectionEndSendAckRequested);
            this.connection.SendAckRequestedHandler = new OperationWithTimeoutCallback(this.OnConnectionSendAckRequested);
        }

        private static void OnPollingComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((ReliableRequestSessionChannel) result.AsyncState).EndSendAckRequestedMessage(result);
            }
        }

        private void PollingCallback()
        {
            IAsyncResult result = this.BeginSendAckRequestedMessage(base.DefaultSendTimeout, MaskingMode.All, onPollingComplete, this);
            if (result.CompletedSynchronously)
            {
                this.EndSendAckRequestedMessage(result);
            }
        }

        private void ProcessCloseOrTerminateReply(bool close, Message reply)
        {
            if (reply == null)
            {
                throw Fx.AssertAndThrow("Argument reply cannot be null.");
            }
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (close)
                {
                    throw Fx.AssertAndThrow("Close does not exist in Feb2005.");
                }
                reply.Close();
            }
            else
            {
                if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                {
                    if (this.closeRequestor.GetInfo() != null)
                    {
                        return;
                    }
                    try
                    {
                        WsrmMessageInfo info = WsrmMessageInfo.Get(this.settings.MessageVersion, reliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), reply);
                        this.session.ProcessInfo(info, null, true);
                        this.session.VerifyDuplexProtocolElements(info, null, true);
                        WsrmFault fault = close ? WsrmUtilities.ValidateCloseSequenceResponse(this.session, this.closeRequestor.MessageId, info, this.connection.Last) : WsrmUtilities.ValidateTerminateSequenceResponse(this.session, this.terminateRequestor.MessageId, info, this.connection.Last);
                        if (fault != null)
                        {
                            this.session.OnLocalFault(null, fault, null);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
                        }
                        return;
                    }
                    finally
                    {
                        reply.Close();
                    }
                }
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        private void ProcessReply(Message reply, IReliableRequest request, long requestSequenceNumber)
        {
            WsrmMessageInfo info = WsrmMessageInfo.Get(this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), reply);
            if (this.session.ProcessInfo(info, null) && this.session.VerifyDuplexProtocolElements(info, null))
            {
                bool flag = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                if (info.WsrmHeaderFault != null)
                {
                    info.Message.Close();
                    if (!(info.WsrmHeaderFault is UnknownSequenceFault))
                    {
                        throw Fx.AssertAndThrow("Fault must be UnknownSequence fault.");
                    }
                    if (this.terminateRequestor == null)
                    {
                        throw Fx.AssertAndThrow("If we start getting UnknownSequence, terminateRequestor cannot be null.");
                    }
                    this.terminateRequestor.SetInfo(info);
                }
                else if (info.AcknowledgementInfo == null)
                {
                    WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedReplyMissingAcknowledgement"), System.ServiceModel.SR.GetString("ReplyMissingAcknowledgement"));
                    info.Message.Close();
                    this.session.OnLocalFault(fault.CreateException(), fault, null);
                }
                else if (flag && (info.TerminateSequenceInfo != null))
                {
                    UniqueId sequenceID = (info.TerminateSequenceInfo.Identifier == this.session.OutputID) ? this.session.InputID : this.session.OutputID;
                    WsrmFault fault2 = SequenceTerminatedFault.CreateProtocolFault(sequenceID, System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedTerminateSequence"), System.ServiceModel.SR.GetString("UnsupportedTerminateSequenceExceptionString"));
                    info.Message.Close();
                    this.session.OnLocalFault(fault2.CreateException(), fault2, null);
                }
                else if (flag && info.AcknowledgementInfo.Final)
                {
                    info.Message.Close();
                    if (this.closeRequestor == null)
                    {
                        string exceptionMessage = System.ServiceModel.SR.GetString("UnsupportedCloseExceptionString");
                        string faultReason = System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedClose");
                        WsrmFault fault3 = SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, faultReason, exceptionMessage);
                        this.session.OnLocalFault(fault3.CreateException(), fault3, null);
                    }
                    else
                    {
                        WsrmFault fault4 = WsrmUtilities.ValidateFinalAck(this.session, info, this.connection.Last);
                        if (fault4 == null)
                        {
                            this.closeRequestor.SetInfo(info);
                        }
                        else
                        {
                            this.session.OnLocalFault(fault4.CreateException(), fault4, null);
                        }
                    }
                }
                else
                {
                    int quotaRemaining = -1;
                    if (this.settings.FlowControlEnabled)
                    {
                        quotaRemaining = info.AcknowledgementInfo.BufferRemaining;
                    }
                    if ((info.SequencedMessageInfo != null) && !ReliableInputConnection.CanMerge(info.SequencedMessageInfo.SequenceNumber, this.ranges))
                    {
                        info.Message.Close();
                    }
                    else
                    {
                        bool flag2 = (this.replyAckConsistencyGuard != null) ? this.replyAckConsistencyGuard.Enter() : false;
                        try
                        {
                            this.connection.ProcessTransferred(requestSequenceNumber, info.AcknowledgementInfo.Ranges, quotaRemaining);
                            this.session.OnRemoteActivity(this.connection.Strategy.QuotaRemaining == 0);
                            if (info.SequencedMessageInfo != null)
                            {
                                lock (base.ThisLock)
                                {
                                    this.ranges = this.ranges.MergeWith(info.SequencedMessageInfo.SequenceNumber);
                                }
                            }
                        }
                        finally
                        {
                            if (flag2)
                            {
                                this.replyAckConsistencyGuard.Exit();
                            }
                        }
                        if (request != null)
                        {
                            if (WsrmUtilities.IsWsrmAction(this.settings.ReliableMessagingVersion, info.Action))
                            {
                                info.Message.Close();
                                request.Set(null);
                            }
                            else
                            {
                                request.Set(info.Message);
                            }
                        }
                        if ((this.shutdownHandle != null) && this.connection.CheckForTermination())
                        {
                            this.shutdownHandle.Set();
                        }
                        if (request != null)
                        {
                            request.Complete();
                        }
                    }
                }
            }
        }

        private void TerminateSequence(TimeSpan timeout)
        {
            this.CreateTerminateRequestor();
            Message reply = this.terminateRequestor.Request(timeout);
            if (reply != null)
            {
                this.ProcessCloseOrTerminateReply(false, reply);
            }
        }

        private void UnblockClose()
        {
            base.FaultPendingRequests();
            if (this.connection != null)
            {
                this.connection.Fault(this);
            }
            if (this.shutdownHandle != null)
            {
                this.shutdownHandle.Fault(this);
            }
            ReliableRequestor closeRequestor = this.closeRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Fault(this);
            }
            closeRequestor = this.terminateRequestor;
            if (closeRequestor != null)
            {
                closeRequestor.Fault(this);
            }
        }

        private void WaitForShutdown(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.shutdownHandle.Wait(helper.RemainingTime());
            }
            else
            {
                this.isLastKnown = true;
                this.replyAckConsistencyGuard.Close(helper.RemainingTime());
            }
        }

        public IOutputSession Session
        {
            get
            {
                return this.session;
            }
        }

        private class AsyncRequest : AsyncResult, ReliableRequestSessionChannel.IReliableRequest, IAsyncRequest, IAsyncResult, IRequestBase
        {
            private bool completed;
            private ReliableRequestSessionChannel parent;
            private Message reply;
            private bool set;
            private object thisLock;

            public AsyncRequest(ReliableRequestSessionChannel parent, AsyncCallback callback, object state) : base(callback, state)
            {
                this.thisLock = new object();
                this.parent = parent;
            }

            public void Abort(RequestChannel channel)
            {
                if (this.ShouldComplete())
                {
                    base.Complete(false, this.parent.CreateClosedException());
                }
            }

            private void AddCompleted(IAsyncResult result)
            {
                Exception invalidAddException = null;
                try
                {
                    if (!this.parent.connection.EndAddMessage(result))
                    {
                        invalidAddException = this.parent.GetInvalidAddException();
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    invalidAddException = exception2;
                }
                if ((invalidAddException != null) && this.ShouldComplete())
                {
                    base.Complete(result.CompletedSynchronously, invalidAddException);
                }
            }

            public void BeginSendRequest(Message message, TimeSpan timeout)
            {
                this.parent.connection.BeginAddMessage(message, timeout, this, Fx.ThunkCallback(new AsyncCallback(this.AddCompleted)), null);
            }

            public void Complete()
            {
                if (this.ShouldComplete())
                {
                    base.Complete(false, null);
                }
            }

            public Message End()
            {
                AsyncResult.End<ReliableRequestSessionChannel.AsyncRequest>(this);
                return this.reply;
            }

            public void Fault(RequestChannel channel)
            {
                if (this.ShouldComplete())
                {
                    base.Complete(false, this.parent.GetTerminalException());
                }
            }

            public void Set(Message reply)
            {
                lock (this.ThisLock)
                {
                    if (!this.set)
                    {
                        this.reply = reply;
                        this.set = true;
                        return;
                    }
                }
                if (reply != null)
                {
                    reply.Close();
                }
            }

            private bool ShouldComplete()
            {
                lock (this.ThisLock)
                {
                    if (this.completed)
                    {
                        return false;
                    }
                    this.completed = true;
                }
                return true;
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }

        private interface IReliableRequest : IRequestBase
        {
            void Complete();
            void Set(Message reply);
        }

        private class SyncRequest : ReliableRequestSessionChannel.IReliableRequest, IRequest, IRequestBase
        {
            private bool aborted;
            private bool completed;
            private ManualResetEvent completedHandle;
            private bool faulted;
            private TimeSpan originalTimeout;
            private ReliableRequestSessionChannel parent;
            private Message reply;
            private object thisLock = new object();

            public SyncRequest(ReliableRequestSessionChannel parent)
            {
                this.parent = parent;
            }

            public void Abort(RequestChannel channel)
            {
                lock (this.ThisLock)
                {
                    if (!this.completed)
                    {
                        this.aborted = true;
                        this.completed = true;
                        if (this.completedHandle != null)
                        {
                            this.completedHandle.Set();
                        }
                    }
                }
            }

            public void Complete()
            {
            }

            public void Fault(RequestChannel channel)
            {
                lock (this.ThisLock)
                {
                    if (!this.completed)
                    {
                        this.faulted = true;
                        this.completed = true;
                        if (this.completedHandle != null)
                        {
                            this.completedHandle.Set();
                        }
                    }
                }
            }

            public void SendRequest(Message message, TimeSpan timeout)
            {
                this.originalTimeout = timeout;
                if (!this.parent.connection.AddMessage(message, timeout, this))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.GetInvalidAddException());
                }
            }

            public void Set(Message reply)
            {
                lock (this.ThisLock)
                {
                    if (!this.completed)
                    {
                        this.reply = reply;
                        this.completed = true;
                        if (this.completedHandle != null)
                        {
                            this.completedHandle.Set();
                        }
                        return;
                    }
                }
                if (reply != null)
                {
                    reply.Close();
                }
            }

            public Message WaitForReply(TimeSpan timeout)
            {
                Message reply;
                bool flag = true;
                try
                {
                    bool flag2 = false;
                    if (!this.completed)
                    {
                        bool flag3 = false;
                        lock (this.ThisLock)
                        {
                            if (!this.completed)
                            {
                                flag3 = true;
                                this.completedHandle = new ManualResetEvent(false);
                            }
                        }
                        if (flag3)
                        {
                            flag2 = !TimeoutHelper.WaitOne(this.completedHandle, timeout);
                            lock (this.ThisLock)
                            {
                                if (!this.completed)
                                {
                                    this.completed = true;
                                }
                                else
                                {
                                    flag2 = false;
                                }
                            }
                            this.completedHandle.Close();
                        }
                    }
                    if (this.aborted)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.CreateClosedException());
                    }
                    if (this.faulted)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.GetTerminalException());
                    }
                    if (flag2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnRequest", new object[] { this.originalTimeout })));
                    }
                    flag = false;
                    reply = this.reply;
                }
                finally
                {
                    if (flag)
                    {
                        WsrmFault fault = SequenceTerminatedFault.CreateCommunicationFault(this.parent.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedReliableRequestThrew"), null);
                        this.parent.session.OnLocalFault(null, fault, null);
                        if (this.completedHandle != null)
                        {
                            this.completedHandle.Close();
                        }
                    }
                }
                return reply;
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }
    }
}

