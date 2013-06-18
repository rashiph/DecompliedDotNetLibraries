namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal class ServerReliableSession : ChannelReliableSession, IInputSession, ISession
    {
        public ServerReliableSession(ChannelBase channel, IReliableFactorySettings listener, IServerReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID, UniqueId outputID) : base(channel, listener, binder, faultHelper)
        {
            base.InputID = inputID;
            base.OutputID = outputID;
        }

        public override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        public override void EndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
            base.StartInactivityTimer();
        }

        public override void OnLocalActivity()
        {
        }

        public override void Open(TimeSpan timeout)
        {
            this.StartInactivityTimer();
        }

        protected override WsrmFault VerifyDuplexProtocolElements(WsrmMessageInfo info)
        {
            WsrmFault fault = base.VerifyDuplexProtocolElements(info);
            if (fault != null)
            {
                return fault;
            }
            if ((info.CreateSequenceInfo != null) && (info.CreateSequenceInfo.OfferIdentifier != base.OutputID))
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCSOfferId"), System.ServiceModel.SR.GetString("UnexpectedCSOfferId"));
            }
            if (info.CreateSequenceResponseInfo != null)
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.OutputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCSR"), System.ServiceModel.SR.GetString("UnexpectedCSR"));
            }
            return null;
        }

        protected override WsrmFault VerifySimplexProtocolElements(WsrmMessageInfo info)
        {
            if (info.AcknowledgementInfo != null)
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedAcknowledgement"), System.ServiceModel.SR.GetString("UnexpectedAcknowledgement"));
            }
            if ((info.AckRequestedInfo != null) && (info.AckRequestedInfo.SequenceID != base.InputID))
            {
                return new UnknownSequenceFault(info.AckRequestedInfo.SequenceID);
            }
            if (info.CreateSequenceResponseInfo != null)
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCSR"), System.ServiceModel.SR.GetString("UnexpectedCSR"));
            }
            if ((info.SequencedMessageInfo != null) && (info.SequencedMessageInfo.SequenceID != base.InputID))
            {
                return new UnknownSequenceFault(info.SequencedMessageInfo.SequenceID);
            }
            if ((info.TerminateSequenceInfo != null) && (info.TerminateSequenceInfo.Identifier != base.InputID))
            {
                return new UnknownSequenceFault(info.TerminateSequenceInfo.Identifier);
            }
            if (info.TerminateSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(base.Settings.ReliableMessagingVersion);
                if (info.TerminateSequenceResponseInfo.Identifier == base.InputID)
                {
                    return SequenceTerminatedFault.CreateProtocolFault(base.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedTerminateSequenceResponse"), System.ServiceModel.SR.GetString("UnexpectedTerminateSequenceResponse"));
                }
                return new UnknownSequenceFault(info.TerminateSequenceResponseInfo.Identifier);
            }
            if (info.CloseSequenceInfo != null)
            {
                WsrmUtilities.AssertWsrm11(base.Settings.ReliableMessagingVersion);
                if (info.CloseSequenceInfo.Identifier == base.InputID)
                {
                    return null;
                }
                return new UnknownSequenceFault(info.CloseSequenceInfo.Identifier);
            }
            if (info.CloseSequenceResponseInfo == null)
            {
                return null;
            }
            WsrmUtilities.AssertWsrm11(base.Settings.ReliableMessagingVersion);
            if (info.CloseSequenceResponseInfo.Identifier == base.InputID)
            {
                return SequenceTerminatedFault.CreateProtocolFault(base.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedUnexpectedCloseSequenceResponse"), System.ServiceModel.SR.GetString("UnexpectedCloseSequenceResponse"));
            }
            return new UnknownSequenceFault(info.CloseSequenceResponseInfo.Identifier);
        }

        public override UniqueId SequenceID
        {
            get
            {
                return base.InputID;
            }
        }
    }
}

