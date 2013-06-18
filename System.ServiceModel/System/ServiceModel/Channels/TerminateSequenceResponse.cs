namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class TerminateSequenceResponse : BodyWriter
    {
        private UniqueId identifier;

        public TerminateSequenceResponse() : base(true)
        {
        }

        public TerminateSequenceResponse(UniqueId identifier) : base(true)
        {
            this.identifier = identifier;
        }

        public static TerminateSequenceResponseInfo Create(XmlDictionaryReader reader)
        {
            TerminateSequenceResponseInfo info = new TerminateSequenceResponseInfo();
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);
            reader.ReadStartElement(DXD.Wsrm11Dictionary.TerminateSequenceResponse, namespaceUri);
            reader.ReadStartElement(XD.WsrmFeb2005Dictionary.Identifier, namespaceUri);
            info.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();
            return info;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);
            writer.WriteStartElement(DXD.Wsrm11Dictionary.TerminateSequenceResponse, namespaceUri);
            writer.WriteStartElement(XD.WsrmFeb2005Dictionary.Identifier, namespaceUri);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();
            writer.WriteEndElement();
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
    }
}

