namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Xml;

    internal abstract class ChannelReliableSession : ISession
    {
        private IReliableChannelBinder binder;
        private bool canSendFault = true;
        private ChannelBase channel;
        private SessionFaultState faulted;
        private System.ServiceModel.Channels.FaultHelper faultHelper;
        private SequenceRangeCollection finalRanges;
        private System.ServiceModel.Channels.Guard guard = new System.ServiceModel.Channels.Guard(0x7fffffff);
        private InterruptibleTimer inactivityTimer;
        private TimeSpan initiationTime;
        private UniqueId inputID;
        private bool isSessionClosed;
        private UniqueId outputID;
        private RequestContext replyFaultContext;
        private IReliableFactorySettings settings;
        private Message terminatingFault;
        private object thisLock = new object();
        private UnblockChannelCloseHandler unblockChannelCloseCallback;

        protected ChannelReliableSession(ChannelBase channel, IReliableFactorySettings settings, IReliableChannelBinder binder, System.ServiceModel.Channels.FaultHelper faultHelper)
        {
            this.channel = channel;
            this.settings = settings;
            this.binder = binder;
            this.faultHelper = faultHelper;
            this.inactivityTimer = new InterruptibleTimer(this.settings.InactivityTimeout, new WaitCallback(this.OnInactivityElapsed), null);
            this.initiationTime = ReliableMessagingConstants.UnknownInitiationTime;
        }

        public virtual void Abort()
        {
            bool flag;
            this.guard.Abort();
            this.inactivityTimer.Abort();
            lock (this.ThisLock)
            {
                if (this.faulted == SessionFaultState.CleanedUp)
                {
                    return;
                }
                flag = this.canSendFault && (this.faulted != SessionFaultState.RemotelyFaulted);
                this.faulted = SessionFaultState.CleanedUp;
            }
            if (((flag && (this.binder.State == CommunicationState.Opened)) && this.binder.Connected) && (this.binder.CanSendAsynchronously || (this.replyFaultContext != null)))
            {
                if (this.terminatingFault == null)
                {
                    UniqueId sequenceID = this.InputID ?? this.OutputID;
                    if (sequenceID != null)
                    {
                        this.terminatingFault = SequenceTerminatedFault.CreateCommunicationFault(sequenceID, System.ServiceModel.SR.GetString("SequenceTerminatedOnAbort"), null).CreateMessage(this.settings.MessageVersion, this.settings.ReliableMessagingVersion);
                    }
                }
                if (this.terminatingFault != null)
                {
                    this.AddFinalRanges();
                    this.faultHelper.SendFaultAsync(this.binder, this.replyFaultContext, this.terminatingFault);
                    return;
                }
            }
            if (this.terminatingFault != null)
            {
                this.terminatingFault.Close();
            }
            if (this.replyFaultContext != null)
            {
                this.replyFaultContext.Abort();
            }
            this.binder.Abort();
        }

        private void AddFinalRanges()
        {
            if (this.finalRanges != null)
            {
                WsrmUtilities.AddAcknowledgementHeader(this.settings.ReliableMessagingVersion, this.terminatingFault, this.InputID, this.finalRanges, true);
            }
        }

        public virtual IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.guard.BeginClose(timeout, callback, state);
        }

        public abstract IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        public virtual void Close(TimeSpan timeout)
        {
            this.guard.Close(timeout);
            this.inactivityTimer.Abort();
        }

        public void CloseSession()
        {
            this.isSessionClosed = true;
        }

        public virtual void EndClose(IAsyncResult result)
        {
            this.guard.EndClose(result);
            this.inactivityTimer.Abort();
        }

        public abstract void EndOpen(IAsyncResult result);
        protected virtual void FaultCore()
        {
            this.inactivityTimer.Abort();
        }

        public virtual void OnFaulted()
        {
            bool flag;
            this.FaultCore();
            lock (this.ThisLock)
            {
                if ((this.faulted == SessionFaultState.NotFaulted) || (this.faulted == SessionFaultState.CleanedUp))
                {
                    return;
                }
                flag = this.canSendFault && (this.faulted != SessionFaultState.RemotelyFaulted);
                this.faulted = SessionFaultState.CleanedUp;
            }
            if ((((!flag || (this.binder.State != CommunicationState.Opened)) || !this.binder.Connected) || (!this.binder.CanSendAsynchronously && (this.replyFaultContext == null))) || (this.terminatingFault == null))
            {
                if (this.terminatingFault != null)
                {
                    this.terminatingFault.Close();
                }
                if (this.replyFaultContext != null)
                {
                    this.replyFaultContext.Abort();
                }
                this.binder.Abort();
            }
            else
            {
                this.AddFinalRanges();
                this.faultHelper.SendFaultAsync(this.binder, this.replyFaultContext, this.terminatingFault);
            }
        }

        private void OnInactivityElapsed(object state)
        {
            WsrmFault fault;
            Exception exception;
            string exceptionMessage = System.ServiceModel.SR.GetString("SequenceTerminatedInactivityTimeoutExceeded", new object[] { this.settings.InactivityTimeout });
            if (this.SequenceID != null)
            {
                string faultReason = System.ServiceModel.SR.GetString("SequenceTerminatedInactivityTimeoutExceeded", new object[] { this.settings.InactivityTimeout });
                fault = SequenceTerminatedFault.CreateCommunicationFault(this.SequenceID, faultReason, exceptionMessage);
                exception = fault.CreateException();
            }
            else
            {
                fault = null;
                exception = new CommunicationException(exceptionMessage);
            }
            this.OnLocalFault(exception, fault, null);
        }

        public abstract void OnLocalActivity();
        public void OnLocalFault(Exception e, Message faultMessage, RequestContext context)
        {
            if ((this.channel.Aborted || (this.channel.State == CommunicationState.Faulted)) || (this.channel.State == CommunicationState.Closed))
            {
                if (faultMessage != null)
                {
                    faultMessage.Close();
                }
                if (context != null)
                {
                    context.Abort();
                }
            }
            else
            {
                lock (this.ThisLock)
                {
                    if (this.faulted != SessionFaultState.NotFaulted)
                    {
                        return;
                    }
                    this.faulted = SessionFaultState.LocallyFaulted;
                    this.terminatingFault = faultMessage;
                    this.replyFaultContext = context;
                }
                this.FaultCore();
                this.channel.Fault(e);
                this.UnblockChannelIfNecessary();
            }
        }

        public void OnLocalFault(Exception e, WsrmFault fault, RequestContext context)
        {
            Message faultMessage = (fault == null) ? null : fault.CreateMessage(this.settings.MessageVersion, this.settings.ReliableMessagingVersion);
            this.OnLocalFault(e, faultMessage, context);
        }

        public virtual void OnRemoteActivity(bool fastPolling)
        {
            this.inactivityTimer.Set();
        }

        public void OnRemoteFault(Exception e)
        {
            if ((!this.channel.Aborted && (this.channel.State != CommunicationState.Faulted)) && (this.channel.State != CommunicationState.Closed))
            {
                lock (this.ThisLock)
                {
                    if (this.faulted != SessionFaultState.NotFaulted)
                    {
                        return;
                    }
                    this.faulted = SessionFaultState.RemotelyFaulted;
                }
                this.FaultCore();
                this.channel.Fault(e);
                this.UnblockChannelIfNecessary();
            }
        }

        public void OnRemoteFault(WsrmFault fault)
        {
            this.OnRemoteFault(WsrmFault.CreateException(fault));
        }

        public void OnUnknownException(Exception e)
        {
            this.canSendFault = false;
            this.OnLocalFault(e, (Message) null, null);
        }

        public abstract void Open(TimeSpan timeout);
        public bool ProcessInfo(WsrmMessageInfo info, RequestContext context)
        {
            return this.ProcessInfo(info, context, false);
        }

        public bool ProcessInfo(WsrmMessageInfo info, RequestContext context, bool throwException)
        {
            Exception faultException;
            if (info.ParsingException != null)
            {
                WsrmFault fault;
                if (this.SequenceID != null)
                {
                    string faultReason = System.ServiceModel.SR.GetString("CouldNotParseWithAction", new object[] { info.Action });
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.SequenceID, faultReason, null);
                }
                else
                {
                    fault = null;
                }
                faultException = new ProtocolException(System.ServiceModel.SR.GetString("MessageExceptionOccurred"), info.ParsingException);
                this.OnLocalFault(throwException ? null : faultException, fault, context);
            }
            else if (info.FaultReply != null)
            {
                faultException = info.FaultException;
                this.OnLocalFault(throwException ? null : faultException, info.FaultReply, context);
            }
            else if (((info.WsrmHeaderFault != null) && (info.WsrmHeaderFault.SequenceID != this.InputID)) && (info.WsrmHeaderFault.SequenceID != this.OutputID))
            {
                faultException = new ProtocolException(System.ServiceModel.SR.GetString("WrongIdentifierFault", new object[] { FaultException.GetSafeReasonText(info.WsrmHeaderFault.Reason) }));
                this.OnLocalFault(throwException ? null : faultException, (Message) null, context);
            }
            else
            {
                if (info.FaultInfo == null)
                {
                    return true;
                }
                if (this.isSessionClosed)
                {
                    UnknownSequenceFault faultInfo = info.FaultInfo as UnknownSequenceFault;
                    if (faultInfo != null)
                    {
                        UniqueId sequenceID = faultInfo.SequenceID;
                        if (((this.OutputID != null) && (this.OutputID == sequenceID)) || ((this.InputID != null) && (this.InputID == sequenceID)))
                        {
                            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                            {
                                info.Message.Close();
                                return false;
                            }
                            if (this.settings.ReliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
                            {
                                throw Fx.AssertAndThrow("Unknown version.");
                            }
                            return true;
                        }
                    }
                }
                faultException = info.FaultException;
                if (context != null)
                {
                    context.Close();
                }
                this.OnRemoteFault(throwException ? null : faultException);
            }
            info.Message.Close();
            if (throwException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(faultException);
            }
            return false;
        }

        public void SetFinalAck(SequenceRangeCollection finalRanges)
        {
            this.finalRanges = finalRanges;
        }

        public virtual void StartInactivityTimer()
        {
            this.inactivityTimer.Set();
        }

        private void UnblockChannelIfNecessary()
        {
            lock (this.ThisLock)
            {
                if (this.faulted == SessionFaultState.NotFaulted)
                {
                    throw Fx.AssertAndThrow("This method must be called from a fault thread.");
                }
                if (this.faulted == SessionFaultState.CleanedUp)
                {
                    return;
                }
            }
            this.OnFaulted();
            this.unblockChannelCloseCallback();
        }

        protected virtual WsrmFault VerifyDuplexProtocolElements(WsrmMessageInfo info)
        {
            if ((info.AcknowledgementInfo != null) && (info.AcknowledgementInfo.SequenceID != this.OutputID))
            {
                return new UnknownSequenceFault(info.AcknowledgementInfo.SequenceID);
            }
            if ((info.AckRequestedInfo != null) && (info.AckRequestedInfo.SequenceID != this.InputID))
            {
                return new UnknownSequenceFault(info.AckRequestedInfo.SequenceID);
            }
            if ((info.SequencedMessageInfo != null) && (info.SequencedMessageInfo.SequenceID != this.InputID))
            {
                return new UnknownSequenceFault(info.SequencedMessageInfo.SequenceID);
            }
            if ((info.TerminateSequenceInfo != null) && (info.TerminateSequenceInfo.Identifier != this.InputID))
            {
                if (this.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedTerminateSequence"), System.ServiceModel.SR.GetString("UnexpectedTerminateSequence"));
                }
                if (info.TerminateSequenceInfo.Identifier == this.OutputID)
                {
                    return null;
                }
                return new UnknownSequenceFault(info.TerminateSequenceInfo.Identifier);
            }
            if (info.TerminateSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.settings.ReliableMessagingVersion);
                if (info.TerminateSequenceResponseInfo.Identifier == this.OutputID)
                {
                    return null;
                }
                return new UnknownSequenceFault(info.TerminateSequenceResponseInfo.Identifier);
            }
            if (info.CloseSequenceInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.settings.ReliableMessagingVersion);
                if (info.CloseSequenceInfo.Identifier == this.InputID)
                {
                    return null;
                }
                if (info.CloseSequenceInfo.Identifier == this.OutputID)
                {
                    return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnsupportedClose"), System.ServiceModel.SR.GetString("UnsupportedCloseExceptionString"));
                }
                return new UnknownSequenceFault(info.CloseSequenceInfo.Identifier);
            }
            if (info.CloseSequenceResponseInfo == null)
            {
                return null;
            }
            WsrmUtilities.AssertWsrm11(this.settings.ReliableMessagingVersion);
            if (info.CloseSequenceResponseInfo.Identifier == this.OutputID)
            {
                return null;
            }
            if (info.CloseSequenceResponseInfo.Identifier == this.InputID)
            {
                return SequenceTerminatedFault.CreateProtocolFault(this.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCloseSequenceResponse"), System.ServiceModel.SR.GetString("UnexpectedCloseSequenceResponse"));
            }
            return new UnknownSequenceFault(info.CloseSequenceResponseInfo.Identifier);
        }

        public bool VerifyDuplexProtocolElements(WsrmMessageInfo info, RequestContext context)
        {
            return this.VerifyDuplexProtocolElements(info, context, false);
        }

        public bool VerifyDuplexProtocolElements(WsrmMessageInfo info, RequestContext context, bool throwException)
        {
            WsrmFault fault = this.VerifyDuplexProtocolElements(info);
            if (fault == null)
            {
                return true;
            }
            if (throwException)
            {
                Exception exception = fault.CreateException();
                this.OnLocalFault(null, fault, context);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            this.OnLocalFault(fault.CreateException(), fault, context);
            return false;
        }

        protected abstract WsrmFault VerifySimplexProtocolElements(WsrmMessageInfo info);
        public bool VerifySimplexProtocolElements(WsrmMessageInfo info, RequestContext context)
        {
            return this.VerifySimplexProtocolElements(info, context, false);
        }

        public bool VerifySimplexProtocolElements(WsrmMessageInfo info, RequestContext context, bool throwException)
        {
            WsrmFault fault = this.VerifySimplexProtocolElements(info);
            if (fault == null)
            {
                return true;
            }
            info.Message.Close();
            if (throwException)
            {
                Exception exception = fault.CreateException();
                this.OnLocalFault(null, fault, context);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            this.OnLocalFault(fault.CreateException(), fault, context);
            return false;
        }

        protected ChannelBase Channel
        {
            get
            {
                return this.channel;
            }
        }

        protected System.ServiceModel.Channels.FaultHelper FaultHelper
        {
            get
            {
                return this.faultHelper;
            }
        }

        protected System.ServiceModel.Channels.Guard Guard
        {
            get
            {
                return this.guard;
            }
        }

        public string Id
        {
            get
            {
                UniqueId sequenceID = this.SequenceID;
                if (sequenceID == null)
                {
                    return null;
                }
                return sequenceID.ToString();
            }
        }

        public TimeSpan InitiationTime
        {
            get
            {
                return this.initiationTime;
            }
            protected set
            {
                this.initiationTime = value;
            }
        }

        public UniqueId InputID
        {
            get
            {
                return this.inputID;
            }
            protected set
            {
                this.inputID = value;
            }
        }

        public UniqueId OutputID
        {
            get
            {
                return this.outputID;
            }
            protected set
            {
                this.outputID = value;
            }
        }

        public abstract UniqueId SequenceID { get; }

        public IReliableFactorySettings Settings
        {
            get
            {
                return this.settings;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public UnblockChannelCloseHandler UnblockChannelCloseCallback
        {
            set
            {
                this.unblockChannelCloseCallback = value;
            }
        }

        private enum SessionFaultState
        {
            NotFaulted,
            LocallyFaulted,
            RemotelyFaulted,
            CleanedUp
        }

        public delegate void UnblockChannelCloseHandler();
    }
}

