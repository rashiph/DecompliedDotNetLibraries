namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal sealed class CreateSequenceInfo : WsrmRequestInfo
    {
        private EndpointAddress acksTo = EndpointAddress.AnonymousAddress;
        private TimeSpan? expires;
        private TimeSpan? offerExpires;
        private UniqueId offerIdentifier;
        private Uri to;

        public static CreateSequenceInfo ReadMessage(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, ISecureConversationSession securitySession, Message message, MessageHeaders headers)
        {
            CreateSequenceInfo info;
            if (message.IsEmpty)
            {
                string reason = System.ServiceModel.SR.GetString("NonEmptyWsrmMessageIsEmpty", new object[] { WsrmIndex.GetCreateSequenceActionString(reliableMessagingVersion) });
                Message faultReply = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, reason);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(faultReply, reason, new ProtocolException(reason)));
            }
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CreateSequence.Create(messageVersion, reliableMessagingVersion, securitySession, reader);
                message.ReadFromBodyContentsToEnd(reader);
            }
            info.SetMessageId(messageVersion, headers);
            info.SetReplyTo(messageVersion, headers);
            if (info.AcksTo.Uri != info.ReplyTo.Uri)
            {
                string str2 = System.ServiceModel.SR.GetString("CSRefusedAcksToMustEqualReplyTo");
                Message message3 = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, str2);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(message3, str2, new ProtocolException(str2)));
            }
            info.to = message.Headers.To;
            if ((info.to == null) && (messageVersion.Addressing == AddressingVersion.WSAddressing10))
            {
                info.to = messageVersion.Addressing.AnonymousUri;
            }
            return info;
        }

        public static void ValidateCreateSequenceHeaders(MessageVersion messageVersion, ISecureConversationSession securitySession, WsrmMessageInfo info)
        {
            string reason = null;
            if (info.UsesSequenceSSLInfo != null)
            {
                reason = System.ServiceModel.SR.GetString("CSRefusedSSLNotSupported");
            }
            else if ((info.UsesSequenceSTRInfo != null) && (securitySession == null))
            {
                reason = System.ServiceModel.SR.GetString("CSRefusedSTRNoWSSecurity");
            }
            else if ((info.UsesSequenceSTRInfo == null) && (securitySession != null))
            {
                reason = System.ServiceModel.SR.GetString("CSRefusedNoSTRWSSecurity");
            }
            if (reason != null)
            {
                Message faultReply = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, ReliableMessagingVersion.WSReliableMessaging11, reason);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(faultReply, reason, new ProtocolException(reason)));
            }
        }

        public EndpointAddress AcksTo
        {
            get
            {
                return this.acksTo;
            }
            set
            {
                this.acksTo = value;
            }
        }

        public TimeSpan? Expires
        {
            get
            {
                return this.expires;
            }
            set
            {
                this.expires = value;
            }
        }

        public TimeSpan? OfferExpires
        {
            get
            {
                return this.offerExpires;
            }
            set
            {
                this.offerExpires = value;
            }
        }

        public UniqueId OfferIdentifier
        {
            get
            {
                return this.offerIdentifier;
            }
            set
            {
                this.offerIdentifier = value;
            }
        }

        public override string RequestName
        {
            get
            {
                return "CreateSequence";
            }
        }

        public Uri To
        {
            get
            {
                return this.to;
            }
        }
    }
}

