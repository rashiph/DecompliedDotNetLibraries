namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    internal class ClientReliableSession : ChannelReliableSession, IOutputSession, ISession
    {
        private IClientReliableChannelBinder binder;
        private PollingMode oldPollingMode;
        private PollingHandler pollingHandler;
        private PollingMode pollingMode;
        private InterruptibleTimer pollingTimer;
        private ReliableRequestor requestor;

        public ClientReliableSession(ChannelBase channel, IReliableFactorySettings factory, IClientReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID) : base(channel, factory, binder, faultHelper)
        {
            this.binder = binder;
            base.InputID = inputID;
            this.pollingTimer = new InterruptibleTimer(this.GetPollingInterval(), new WaitCallback(this.OnPollingTimerElapsed), null);
            if (this.binder.Channel is IRequestChannel)
            {
                this.requestor = new RequestReliableRequestor();
            }
            else if (this.binder.Channel is IDuplexChannel)
            {
                SendReceiveReliableRequestor requestor = new SendReceiveReliableRequestor {
                    TimeoutIsSafe = !this.ChannelSupportsOneCreateSequenceAttempt()
                };
                this.requestor = requestor;
            }
            MessageVersion messageVersion = base.Settings.MessageVersion;
            ReliableMessagingVersion reliableMessagingVersion = base.Settings.ReliableMessagingVersion;
            this.requestor.MessageVersion = messageVersion;
            this.requestor.Binder = this.binder;
            this.requestor.IsCreateSequence = true;
            this.requestor.TimeoutString1Index = "TimeoutOnOpen";
            this.requestor.MessageAction = WsrmIndex.GetCreateSequenceActionHeader(messageVersion.Addressing, reliableMessagingVersion);
            if ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (this.binder.GetInnerSession() is ISecureConversationSession))
            {
                this.requestor.MessageHeader = new WsrmUsesSequenceSTRHeader();
            }
            this.requestor.MessageBody = new CreateSequence(base.Settings.MessageVersion.Addressing, reliableMessagingVersion, base.Settings.Ordered, this.binder, base.InputID);
            this.requestor.SetRequestResponsePattern();
        }

        public override void Abort()
        {
            ReliableRequestor requestor = this.requestor;
            if (requestor != null)
            {
                requestor.Abort(base.Channel);
            }
            this.pollingTimer.Abort();
            base.Abort();
        }

        public override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.pollingHandler == null)
            {
                throw Fx.AssertAndThrow("The client reliable channel must set the polling handler prior to opening the client reliable session.");
            }
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        private bool ChannelSupportsOneCreateSequenceAttempt()
        {
            IDuplexSessionChannel channel = this.binder.Channel as IDuplexSessionChannel;
            if (channel == null)
            {
                return false;
            }
            return ((channel.Session is ISecuritySession) && !(channel.Session is ISecureConversationSession));
        }

        public override void Close(TimeSpan timeout)
        {
            base.Close(timeout);
            this.pollingTimer.Abort();
        }

        public override void EndClose(IAsyncResult result)
        {
            base.EndClose(result);
            this.pollingTimer.Abort();
        }

        public override void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
            this.requestor = null;
        }

        protected override void FaultCore()
        {
            this.pollingTimer.Abort();
            base.FaultCore();
        }

        private TimeSpan GetPollingInterval()
        {
            switch (this.pollingMode)
            {
                case PollingMode.Idle:
                    return Ticks.ToTimeSpan(Ticks.FromTimeSpan(base.Settings.InactivityTimeout) / 2L);

                case PollingMode.KeepAlive:
                    return WsrmUtilities.CalculateKeepAliveInterval(base.Settings.InactivityTimeout, base.Settings.MaxRetryCount);

                case PollingMode.FastPolling:
                {
                    TimeSpan span = WsrmUtilities.CalculateKeepAliveInterval(base.Settings.InactivityTimeout, base.Settings.MaxRetryCount);
                    TimeSpan span2 = Ticks.ToTimeSpan(Ticks.FromTimeSpan(this.binder.DefaultSendTimeout) / 2L);
                    if (span2 >= span)
                    {
                        return span;
                    }
                    return span2;
                }
                case PollingMode.NotPolling:
                    return TimeSpan.MaxValue;
            }
            throw Fx.AssertAndThrow("Unknown polling mode.");
        }

        public override void OnFaulted()
        {
            base.OnFaulted();
            if (this.requestor != null)
            {
                this.requestor.Fault(base.Channel);
            }
        }

        public override void OnLocalActivity()
        {
            lock (base.ThisLock)
            {
                if (this.pollingMode != PollingMode.NotPolling)
                {
                    this.pollingTimer.Set(this.GetPollingInterval());
                }
            }
        }

        private void OnPollingTimerElapsed(object state)
        {
            if (base.Guard.Enter())
            {
                try
                {
                    lock (base.ThisLock)
                    {
                        if (this.pollingMode == PollingMode.NotPolling)
                        {
                            return;
                        }
                        if (this.pollingMode == PollingMode.Idle)
                        {
                            this.pollingMode = PollingMode.KeepAlive;
                        }
                    }
                    this.pollingHandler();
                    this.pollingTimer.Set(this.GetPollingInterval());
                }
                finally
                {
                    base.Guard.Exit();
                }
            }
        }

        public override void OnRemoteActivity(bool fastPolling)
        {
            base.OnRemoteActivity(fastPolling);
            lock (base.ThisLock)
            {
                if (this.pollingMode != PollingMode.NotPolling)
                {
                    if (fastPolling)
                    {
                        this.pollingMode = PollingMode.FastPolling;
                    }
                    else
                    {
                        this.pollingMode = PollingMode.Idle;
                    }
                    this.pollingTimer.Set(this.GetPollingInterval());
                }
            }
        }

        public override void Open(TimeSpan timeout)
        {
            if (this.pollingHandler == null)
            {
                throw Fx.AssertAndThrow("The client reliable channel must set the polling handler prior to opening the client reliable session.");
            }
            DateTime utcNow = DateTime.UtcNow;
            Message response = this.requestor.Request(timeout);
            this.ProcessCreateSequenceResponse(response, utcNow);
            this.requestor = null;
        }

        private void ProcessCreateSequenceResponse(Message response, DateTime start)
        {
            CreateSequenceResponseInfo createSequenceResponseInfo = null;
            using (response)
            {
                if (response.IsFault)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmUtilities.CreateCSFaultException(base.Settings.MessageVersion, base.Settings.ReliableMessagingVersion, response, this.binder.Channel));
                }
                WsrmMessageInfo info2 = WsrmMessageInfo.Get(base.Settings.MessageVersion, base.Settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), response, true);
                if (info2.ParsingException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("UnparsableCSResponse"), info2.ParsingException));
                }
                base.ProcessInfo(info2, null, true);
                createSequenceResponseInfo = info2.CreateSequenceResponseInfo;
                string message = null;
                string faultReason = null;
                if (createSequenceResponseInfo == null)
                {
                    message = System.ServiceModel.SR.GetString("InvalidWsrmResponseChannelNotOpened", new object[] { "CreateSequence", info2.Action, WsrmIndex.GetCreateSequenceResponseActionString(base.Settings.ReliableMessagingVersion) });
                }
                else if (!object.Equals(createSequenceResponseInfo.RelatesTo, this.requestor.MessageId))
                {
                    message = System.ServiceModel.SR.GetString("WsrmMessageWithWrongRelatesToExceptionString", new object[] { "CreateSequence" });
                    faultReason = System.ServiceModel.SR.GetString("WsrmMessageWithWrongRelatesToFaultString", new object[] { "CreateSequence" });
                }
                else if ((createSequenceResponseInfo.AcceptAcksTo == null) && (base.InputID != null))
                {
                    if (base.Settings.ReliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                    {
                        if (base.Settings.ReliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                        {
                            throw Fx.AssertAndThrow("Reliable messaging version not supported.");
                        }
                        message = System.ServiceModel.SR.GetString("CSResponseOfferRejected");
                        faultReason = System.ServiceModel.SR.GetString("CSResponseOfferRejectedReason");
                    }
                    else
                    {
                        message = System.ServiceModel.SR.GetString("CSResponseWithoutOffer");
                        faultReason = System.ServiceModel.SR.GetString("CSResponseWithoutOfferReason");
                    }
                }
                else if ((createSequenceResponseInfo.AcceptAcksTo != null) && (base.InputID == null))
                {
                    message = System.ServiceModel.SR.GetString("CSResponseWithOffer");
                    faultReason = System.ServiceModel.SR.GetString("CSResponseWithOfferReason");
                }
                else if ((createSequenceResponseInfo.AcceptAcksTo != null) && (createSequenceResponseInfo.AcceptAcksTo.Uri != this.binder.RemoteAddress.Uri))
                {
                    message = System.ServiceModel.SR.GetString("AcksToMustBeSameAsRemoteAddress");
                    faultReason = System.ServiceModel.SR.GetString("AcksToMustBeSameAsRemoteAddressReason");
                }
                if ((faultReason != null) && (createSequenceResponseInfo != null))
                {
                    WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(createSequenceResponseInfo.Identifier, faultReason, null);
                    base.OnLocalFault(null, fault, null);
                }
                if (message != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(message));
                }
            }
            base.InitiationTime = (TimeSpan) (DateTime.UtcNow - start);
            base.OutputID = createSequenceResponseInfo.Identifier;
            this.pollingTimer.Set(this.GetPollingInterval());
            base.StartInactivityTimer();
        }

        public void ResumePolling(bool fastPolling)
        {
            lock (base.ThisLock)
            {
                if (this.pollingMode != PollingMode.NotPolling)
                {
                    throw Fx.AssertAndThrow("Can't resume polling if pollingMode != PollingMode.NotPolling");
                }
                if (fastPolling)
                {
                    this.pollingMode = PollingMode.FastPolling;
                }
                else if (this.oldPollingMode == PollingMode.FastPolling)
                {
                    this.pollingMode = PollingMode.Idle;
                }
                else
                {
                    this.pollingMode = this.oldPollingMode;
                }
                base.Guard.Exit();
                this.pollingTimer.Set(this.GetPollingInterval());
            }
        }

        public bool StopPolling()
        {
            lock (base.ThisLock)
            {
                if (this.pollingMode == PollingMode.NotPolling)
                {
                    return false;
                }
                this.oldPollingMode = this.pollingMode;
                this.pollingMode = PollingMode.NotPolling;
                this.pollingTimer.Cancel();
                return base.Guard.Enter();
            }
        }

        protected override WsrmFault VerifyDuplexProtocolElements(WsrmMessageInfo info)
        {
            WsrmFault fault = base.VerifyDuplexProtocolElements(info);
            if (fault != null)
            {
                return fault;
            }
            if (info.CreateSequenceInfo != null)
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCS"), System.ServiceModel.SR.GetString("UnexpectedCS"));
            }
            if ((info.CreateSequenceResponseInfo != null) && (info.CreateSequenceResponseInfo.Identifier != base.OutputID))
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCSROfferId"), System.ServiceModel.SR.GetString("UnexpectedCSROfferId"));
            }
            return null;
        }

        protected override WsrmFault VerifySimplexProtocolElements(WsrmMessageInfo info)
        {
            if ((info.AcknowledgementInfo != null) && (info.AcknowledgementInfo.SequenceID != base.OutputID))
            {
                return new UnknownSequenceFault(info.AcknowledgementInfo.SequenceID);
            }
            if (info.AckRequestedInfo != null)
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedAckRequested"), System.ServiceModel.SR.GetString("UnexpectedAckRequested"));
            }
            if (info.CreateSequenceInfo != null)
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCS"), System.ServiceModel.SR.GetString("UnexpectedCS"));
            }
            if (info.SequencedMessageInfo != null)
            {
                return new UnknownSequenceFault(info.SequencedMessageInfo.SequenceID);
            }
            if (info.TerminateSequenceInfo != null)
            {
                if (base.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedTerminateSequence"), System.ServiceModel.SR.GetString("UnexpectedTerminateSequence"));
                }
                if (info.TerminateSequenceInfo.Identifier == base.OutputID)
                {
                    return null;
                }
                return new UnknownSequenceFault(info.TerminateSequenceInfo.Identifier);
            }
            if (info.TerminateSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(base.Settings.ReliableMessagingVersion);
                if (info.TerminateSequenceResponseInfo.Identifier == base.OutputID)
                {
                    return null;
                }
                return new UnknownSequenceFault(info.TerminateSequenceResponseInfo.Identifier);
            }
            if (info.CloseSequenceInfo != null)
            {
                WsrmUtilities.AssertWsrm11(base.Settings.ReliableMessagingVersion);
                if (info.CloseSequenceInfo.Identifier == base.OutputID)
                {
                    return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedClose"), System.ServiceModel.SR.GetString("UnsupportedCloseExceptionString"));
                }
                return new UnknownSequenceFault(info.CloseSequenceInfo.Identifier);
            }
            if (info.CloseSequenceResponseInfo == null)
            {
                return null;
            }
            WsrmUtilities.AssertWsrm11(base.Settings.ReliableMessagingVersion);
            if (info.CloseSequenceResponseInfo.Identifier == base.OutputID)
            {
                return null;
            }
            return new UnknownSequenceFault(info.CloseSequenceResponseInfo.Identifier);
        }

        public PollingHandler PollingCallback
        {
            set
            {
                this.pollingHandler = value;
            }
        }

        public override UniqueId SequenceID
        {
            get
            {
                return base.OutputID;
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private static AsyncCallback onRequestComplete = Fx.ThunkCallback(new AsyncCallback(ClientReliableSession.OpenAsyncResult.OnRequestCompleteStatic));
            private ClientReliableSession session;
            private DateTime start;

            public OpenAsyncResult(ClientReliableSession session, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.session = session;
                this.start = DateTime.UtcNow;
                IAsyncResult result = this.session.requestor.BeginRequest(timeout, onRequestComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.CompleteRequest(result);
                    base.Complete(true);
                }
            }

            private void CompleteRequest(IAsyncResult result)
            {
                Message response = this.session.requestor.EndRequest(result);
                this.session.ProcessCreateSequenceResponse(response, this.start);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ClientReliableSession.OpenAsyncResult>(result);
            }

            private static void OnRequestCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ClientReliableSession.OpenAsyncResult asyncState = (ClientReliableSession.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.CompleteRequest(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }

        public delegate void PollingHandler();

        private enum PollingMode
        {
            Idle,
            KeepAlive,
            FastPolling,
            NotPolling
        }
    }
}

