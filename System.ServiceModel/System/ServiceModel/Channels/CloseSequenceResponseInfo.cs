namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class CloseSequenceResponseInfo
    {
        private UniqueId identifier;
        private UniqueId relatesTo;

        public static CloseSequenceResponseInfo ReadMessage(MessageVersion messageVersion, Message message, MessageHeaders headers)
        {
            CloseSequenceResponseInfo info;
            if (headers.RelatesTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MissingRelatesToOnWsrmResponseReason", new object[] { DXD.Wsrm11Dictionary.CloseSequenceResponse }), messageVersion.Addressing.Namespace, "RelatesTo", false));
            }
            if (message.IsEmpty)
            {
                string str = System.ServiceModel.SR.GetString("NonEmptyWsrmMessageIsEmpty", new object[] { "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse" });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(str));
            }
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CloseSequenceResponse.Create(reader);
                message.ReadFromBodyContentsToEnd(reader);
            }
            info.relatesTo = headers.RelatesTo;
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

