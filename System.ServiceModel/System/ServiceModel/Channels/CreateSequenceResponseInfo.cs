namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class CreateSequenceResponseInfo
    {
        private EndpointAddress acceptAcksTo;
        private UniqueId identifier;
        private UniqueId relatesTo;

        public static CreateSequenceResponseInfo ReadMessage(MessageVersion messageVersion, ReliableMessagingVersion reliableMessagingVersion, Message message, MessageHeaders headers)
        {
            CreateSequenceResponseInfo info;
            if (message.IsEmpty)
            {
                string str = System.ServiceModel.SR.GetString("NonEmptyWsrmMessageIsEmpty", new object[] { WsrmIndex.GetCreateSequenceResponseActionString(reliableMessagingVersion) });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(str));
            }
            if (headers.RelatesTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MissingRelatesToOnWsrmResponseReason", new object[] { XD.WsrmFeb2005Dictionary.CreateSequenceResponse }), messageVersion.Addressing.Namespace, "RelatesTo", false));
            }
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CreateSequenceResponse.Create(messageVersion.Addressing, reliableMessagingVersion, reader);
                message.ReadFromBodyContentsToEnd(reader);
            }
            info.RelatesTo = headers.RelatesTo;
            return info;
        }

        public EndpointAddress AcceptAcksTo
        {
            get
            {
                return this.acceptAcksTo;
            }
            set
            {
                this.acceptAcksTo = value;
            }
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

        public UniqueId RelatesTo
        {
            get
            {
                return this.relatesTo;
            }
            set
            {
                this.relatesTo = value;
            }
        }
    }
}

