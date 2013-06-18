namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class CloseSequenceInfo : WsrmRequestInfo
    {
        private UniqueId identifier;
        private long lastMsgNumber;

        public static CloseSequenceInfo ReadMessage(MessageVersion messageVersion, Message message, MessageHeaders headers)
        {
            CloseSequenceInfo info;
            if (message.IsEmpty)
            {
                string str = System.ServiceModel.SR.GetString("NonEmptyWsrmMessageIsEmpty", new object[] { "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequence" });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(str));
            }
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CloseSequence.Create(reader);
                message.ReadFromBodyContentsToEnd(reader);
            }
            info.SetMessageId(messageVersion, headers);
            info.SetReplyTo(messageVersion, headers);
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
                return "CloseSequence";
            }
        }
    }
}

