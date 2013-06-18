namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class TerminateSequenceInfo : WsrmRequestInfo
    {
        private UniqueId identifier;
        private long lastMsgNumber;

        public static TerminateSequenceInfo ReadMessage(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, Message message, MessageHeaders headers)
        {
            TerminateSequenceInfo info;
            if (message.IsEmpty)
            {
                string str = System.ServiceModel.SR.GetString("NonEmptyWsrmMessageIsEmpty", new object[] { WsrmIndex.GetTerminateSequenceActionString(reliableMessagingVersion) });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(str));
            }
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = TerminateSequence.Create(reliableMessagingVersion, reader);
                message.ReadFromBodyContentsToEnd(reader);
            }
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                info.SetMessageId(messageVersion, headers);
                info.SetReplyTo(messageVersion, headers);
            }
            return info;
        }

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public long LastMsgNumber
        {
            get
            {
                return this.lastMsgNumber;
            }
            set
            {
                this.lastMsgNumber = value;
            }
        }

        public override string RequestName
        {
            get
            {
                return "TerminateSequence";
            }
        }
    }
}

