namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class ReliableOutputSessionChannel : OutputChannel, IOutputSessionChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
    {
        private IClientReliableChannelBinder binder;
        private ChannelParameterCollection channelParameters;
        private ReliableRequestor closeRequestor;
        private ReliableOutputConnection connection;
        private Exception maxRetryCountException;
        private ClientReliableSession session;
        private IReliableFactorySettings settings;
        private ReliableRequestor terminateRequestor;

        protected ReliableOutputSessionChannel(ChannelManagerBase factory, IReliableFactorySettings settings, IClientReliableChannelBinder binder, FaultHelper faultHelper, LateBoundChannelParameterCollection channelParameters) : base(factory)
        {
            this.settings = settings;
            this.binder = binder;
            this.session = new ClientReliableSession(this, settings, binder, faultHelper, null);
            this.session.PollingCallback = new ClientReliableSession.PollingHandler(this.PollingCallback);
            this.session.UnblockChannelCloseCallback = new ChannelReliableSession.UnblockChannelCloseHandler(this.UnblockClose);
            this.binder.Faulted += new BinderExceptionHandler(this.OnBinderFaulted);
            this.binder.OnException += new BinderExceptionHandler(this.OnBinderException);
            this.channelParameters = channelParameters;
            channelParameters.SetChannel(this);
        }

        private IAsyncResult BeginCloseSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CreateCloseRequestor();
            return this.closeRequestor.BeginRequest(timeout, callback, state);
        }

        private IAsyncResult BeginTerminateSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.session.CloseSession();
                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion, reliableMessagingVersion, this.session.OutputID);
                return this.OnConnectionBeginSendMessage(message, timeout, callback, state);
            }
            if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            this.CreateTerminateRequestor();
            return this.terminateRequestor.BeginRequest(timeout, callback, state);
        }

        private void CloseSequence(TimeSpan timeout)
        {
            this.CreateCloseRequestor();
            Message reply = this.closeRequestor.Request(timeout);
            this.ProcessCloseOrTerminateReply(true, reply);
        }

        private void ConfigureRequestor(ReliableRequestor requestor)
        {
            requestor.MessageVersion = this.settings.MessageVersion;
            requestor.Binder = this.binder;
            requestor.SetRequestResponsePattern();
        }

        private void CreateCloseRequestor()
        {
            ReliableRequestor requestor = this.CreateRequestor();
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

        protected abstract ReliableRequestor CreateRequestor();
        private void CreateTerminateRequestor()
        {
            ReliableRequestor requestor = this.CreateRequestor();
            this.ConfigureRequestor(requestor);
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            requestor.MessageAction = WsrmIndex.GetTerminateSequenceActionHeader(this.settings.MessageVersion.Addressing, reliableMessagingVersion);
            requestor.MessageBody = new System.ServiceModel.Channels.TerminateSequence(reliableMessagingVersion, this.session.OutputID, this.connection.Last);
            lock (base.ThisLock)
            {
                base.ThrowIfClosed();
                this.terminateRequestor = requestor;
                this.session.CloseSession();
            }
        }

        private void EndCloseSequence(IAsyncResult result)
        {
            Message reply = this.closeRequestor.EndRequest(result);
            this.ProcessCloseOrTerminateReply(true, reply);
        }

        private void EndTerminateSequence(IAsyncResult result)
        {
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.OnConnectionEndSendMessage(result);
            }
            else
            {
                Message reply = this.terminateRequestor.EndRequest(result);
                if (reply != null)
                {
                    this.ProcessCloseOrTerminateReply(false, reply);
                }
            }
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IOutputSessionChannel))
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
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool flag = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
            OperationWithTimeoutBeginCallback[] beginCallbacks = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.connection.BeginClose), flag ? new OperationWithTimeoutBeginCallback(this.BeginCloseSequence) : null, new OperationWithTimeoutBeginCallback(this.BeginTerminateSequence), new OperationWithTimeoutBeginCallback(this.session.BeginClose) };
            return new ReliableChannelCloseAsyncResult(beginCallbacks, new OperationEndCallback[] { new OperationEndCallback(this.connection.EndClose), flag ? new OperationEndCallback(this.EndCloseSequence) : null, new OperationEndCallback(this.EndTerminateSequence), new OperationEndCallback(this.session.EndClose) }, this.binder, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReliableChannelOpenAsyncResult(this.binder, this.session, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.connection.BeginAddMessage(message, timeout, null, callback, state);
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
            this.ReliableSession.OnUnknownException(exception);
        }

        private void OnComponentFaulted(Exception faultException, WsrmFault fault)
        {
            this.session.OnLocalFault(faultException, fault, null);
        }

        protected abstract IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state);
        private IAsyncResult OnConnectionBeginSendAckRequestedHandler(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.session.OnLocalActivity();
            Message message = WsrmUtilities.CreateAckRequestedMessage(this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.ReliableSession.OutputID);
            return this.OnConnectionBeginSendMessage(message, timeout, callback, state);
        }

        private IAsyncResult OnConnectionBeginSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
            {
                this.session.OnLocalFault(new CommunicationException(System.ServiceModel.SR.GetString("MaximumRetryCountExceeded"), this.maxRetryCountException), SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                return new CompletedAsyncResult(callback, state);
            }
            this.session.OnLocalActivity();
            return this.OnConnectionBeginSend(attemptInfo, timeout, maskUnhandledException, callback, state);
        }

        protected abstract IAsyncResult OnConnectionBeginSendMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnConnectionEndSend(IAsyncResult result);
        private void OnConnectionEndSendAckRequestedHandler(IAsyncResult result)
        {
            this.OnConnectionEndSendMessage(result);
        }

        private void OnConnectionEndSendHandler(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.OnConnectionEndSend(result);
            }
        }

        protected abstract void OnConnectionEndSendMessage(IAsyncResult result);
        protected abstract void OnConnectionSend(Message message, TimeSpan timeout, bool saveHandledException, bool maskUnhandledException);
        private void OnConnectionSendAckRequestedHandler(TimeSpan timeout)
        {
            this.session.OnLocalActivity();
            using (Message message = WsrmUtilities.CreateAckRequestedMessage(this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.ReliableSession.OutputID))
            {
                this.OnConnectionSend(message, timeout, false, true);
            }
        }

        private void OnConnectionSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException)
        {
            using (attemptInfo.Message)
            {
                if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
                {
                    this.session.OnLocalFault(new CommunicationException(System.ServiceModel.SR.GetString("MaximumRetryCountExceeded"), this.maxRetryCountException), SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                }
                else
                {
                    this.session.OnLocalActivity();
                    this.OnConnectionSend(attemptInfo.Message, timeout, attemptInfo.RetryCount == this.settings.MaxRetryCount, maskUnhandledException);
                }
            }
        }

        protected abstract void OnConnectionSendMessage(Message message, TimeSpan timeout, MaskingMode maskingMode);
        protected override void OnEndClose(IAsyncResult result)
        {
            ReliableChannelCloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ReliableChannelOpenAsyncResult.End(result);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            if (!this.connection.EndAddMessage(result))
            {
                this.ThrowInvalidAddException();
            }
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
                    this.Binder.Close(helper.RemainingTime());
                }
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.connection = new ReliableOutputConnection(this.session.OutputID, this.Settings.MaxTransferWindowSize, this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.session.InitiationTime, this.RequestAcks, base.DefaultSendTimeout);
            this.connection.Faulted = (ComponentFaultedHandler) Delegate.Combine(this.connection.Faulted, new ComponentFaultedHandler(this.OnComponentFaulted));
            this.connection.OnException = (ComponentExceptionHandler) Delegate.Combine(this.connection.OnException, new ComponentExceptionHandler(this.OnComponentException));
            this.connection.BeginSendHandler = new BeginSendHandler(this.OnConnectionBeginSendHandler);
            this.connection.EndSendHandler = new EndSendHandler(this.OnConnectionEndSendHandler);
            this.connection.SendHandler = new SendHandler(this.OnConnectionSendHandler);
            this.connection.BeginSendAckRequestedHandler = new OperationWithTimeoutBeginCallback(this.OnConnectionBeginSendAckRequestedHandler);
            this.connection.EndSendAckRequestedHandler = new OperationEndCallback(this.OnConnectionEndSendAckRequestedHandler);
            this.connection.SendAckRequestedHandler = new OperationWithTimeoutCallback(this.OnConnectionSendAckRequestedHandler);
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            if (!this.connection.AddMessage(message, timeout, null))
            {
                this.ThrowInvalidAddException();
            }
        }

        private void PollingCallback()
        {
            using (Message message = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID))
            {
                this.OnConnectionSendMessage(message, base.DefaultSendTimeout, MaskingMode.All);
            }
        }

        private void ProcessCloseOrTerminateReply(bool close, Message reply)
        {
            if (reply == null)
            {
                throw Fx.AssertAndThrow("Argument reply cannot be null.");
            }
            ReliableRequestor requestor = close ? this.closeRequestor : this.terminateRequestor;
            if (requestor.GetInfo() == null)
            {
                try
                {
                    WsrmMessageInfo info = WsrmMessageInfo.Get(this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), reply);
                    this.ReliableSession.ProcessInfo(info, null, true);
                    this.ReliableSession.VerifyDuplexProtocolElements(info, null, true);
                    WsrmFault fault = close ? WsrmUtilities.ValidateCloseSequenceResponse(this.session, requestor.MessageId, info, this.connection.Last) : WsrmUtilities.ValidateTerminateSequenceResponse(this.session, requestor.MessageId, info, this.connection.Last);
                    if (fault != null)
                    {
                        this.ReliableSession.OnLocalFault(null, fault, null);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
                    }
                }
                finally
                {
                    reply.Close();
                }
            }
        }

        protected void ProcessMessage(Message message)
        {
            bool flag = true;
            WsrmMessageInfo info = WsrmMessageInfo.Get(this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), message);
            bool flag2 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
            try
            {
                if (!this.session.ProcessInfo(info, null))
                {
                    flag = false;
                    return;
                }
                if (!this.ReliableSession.VerifySimplexProtocolElements(info, null))
                {
                    flag = false;
                    return;
                }
                bool flag3 = false;
                if (info.AcknowledgementInfo != null)
                {
                    flag3 = flag2 && info.AcknowledgementInfo.Final;
                    int quotaRemaining = -1;
                    if (this.settings.FlowControlEnabled)
                    {
                        quotaRemaining = info.AcknowledgementInfo.BufferRemaining;
                    }
                    this.connection.ProcessTransferred(info.AcknowledgementInfo.Ranges, quotaRemaining);
                }
                if (!flag2)
                {
                    goto Label_0300;
                }
                WsrmFault fault = null;
                if (info.TerminateSequenceResponseInfo != null)
                {
                    fault = WsrmUtilities.ValidateTerminateSequenceResponse(this.session, this.terminateRequestor.MessageId, info, this.connection.Last);
                    if (fault == null)
                    {
                        fault = this.ProcessRequestorResponse(this.terminateRequestor, "TerminateSequence", info);
                    }
                }
                else if (info.CloseSequenceResponseInfo != null)
                {
                    fault = WsrmUtilities.ValidateCloseSequenceResponse(this.session, this.closeRequestor.MessageId, info, this.connection.Last);
                    if (fault == null)
                    {
                        fault = this.ProcessRequestorResponse(this.closeRequestor, "CloseSequence", info);
                    }
                }
                else
                {
                    if (info.TerminateSequenceInfo != null)
                    {
                        if (WsrmUtilities.ValidateWsrmRequest(this.session, info.TerminateSequenceInfo, this.binder, null))
                        {
                            WsrmAcknowledgmentInfo acknowledgementInfo = info.AcknowledgementInfo;
                            fault = WsrmUtilities.ValidateFinalAckExists(this.session, acknowledgementInfo);
                            if ((fault == null) && !this.connection.IsFinalAckConsistent(acknowledgementInfo.Ranges))
                            {
                                fault = new InvalidAcknowledgementFault(this.session.OutputID, acknowledgementInfo.Ranges);
                            }
                            if (fault != null)
                            {
                                goto Label_02E5;
                            }
                            Message message2 = WsrmUtilities.CreateTerminateResponseMessage(this.settings.MessageVersion, info.TerminateSequenceInfo.MessageId, this.session.OutputID);
                            try
                            {
                                this.OnConnectionSend(message2, base.DefaultSendTimeout, false, true);
                            }
                            finally
                            {
                                message2.Close();
                            }
                            this.session.OnRemoteFault(new ProtocolException(System.ServiceModel.SR.GetString("UnsupportedTerminateSequenceExceptionString")));
                        }
                        return;
                    }
                    if (flag3)
                    {
                        if (this.closeRequestor == null)
                        {
                            string exceptionMessage = System.ServiceModel.SR.GetString("UnsupportedCloseExceptionString");
                            string faultReason = System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedClose");
                            fault = SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, faultReason, exceptionMessage);
                        }
                        else
                        {
                            fault = WsrmUtilities.ValidateFinalAck(this.session, info, this.connection.Last);
                            if (fault == null)
                            {
                                this.closeRequestor.SetInfo(info);
                            }
                        }
                    }
                    else if (info.WsrmHeaderFault != null)
                    {
                        if (!(info.WsrmHeaderFault is UnknownSequenceFault))
                        {
                            throw Fx.AssertAndThrow("Fault must be UnknownSequence fault.");
                        }
                        if (this.terminateRequestor == null)
                        {
                            throw Fx.AssertAndThrow("In wsrm11, if we start getting UnknownSequence, terminateRequestor cannot be null.");
                        }
                        this.terminateRequestor.SetInfo(info);
                    }
                }
            Label_02E5:
                if (fault != null)
                {
                    this.session.OnLocalFault(fault.CreateException(), fault, null);
                    return;
                }
            Label_0300:
                this.session.OnRemoteActivity(this.connection.Strategy.QuotaRemaining == 0);
            }
            finally
            {
                if (flag)
                {
                    info.Message.Close();
                }
            }
        }

        protected abstract WsrmFault ProcessRequestorResponse(ReliableRequestor requestor, string requestName, WsrmMessageInfo info);
        private void TerminateSequence(TimeSpan timeout)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.session.CloseSession();
                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion, reliableMessagingVersion, this.session.OutputID);
                this.OnConnectionSendMessage(message, timeout, MaskingMode.Handled);
            }
            else
            {
                if (reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                {
                    throw Fx.AssertAndThrow("Reliable messaging version not supported.");
                }
                this.CreateTerminateRequestor();
                Message reply = this.terminateRequestor.Request(timeout);
                if (reply != null)
                {
                    this.ProcessCloseOrTerminateReply(false, reply);
                }
            }
        }

        private void ThrowInvalidAddException()
        {
            if (base.State == CommunicationState.Faulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.GetTerminalException());
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateClosedException());
        }

        private void UnblockClose()
        {
            if (this.connection != null)
            {
                this.connection.Fault(this);
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

        protected IReliableChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        protected ReliableOutputConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        protected Exception MaxRetryCountException
        {
            set
            {
                this.maxRetryCountException = value;
            }
        }

        protected ChannelReliableSession ReliableSession
        {
            get
            {
                return this.session;
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.binder.RemoteAddress;
            }
        }

        protected abstract bool RequestAcks { get; }

        public IOutputSession Session
        {
            get
            {
                return this.session;
            }
        }

        protected IReliableFactorySettings Settings
        {
            get
            {
                return this.settings;
            }
        }

        public override Uri Via
        {
            get
            {
                return this.binder.Via;
            }
        }
    }
}

