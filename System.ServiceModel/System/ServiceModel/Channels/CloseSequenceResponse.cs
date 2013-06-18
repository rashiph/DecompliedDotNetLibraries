namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class CloseSequenceResponse : BodyWriter
    {
        private UniqueId identifier;

        public CloseSequenceResponse(UniqueId identifier) : base(true)
        {
            this.identifier = identifier;
        }

        public static CloseSequenceResponseInfo Create(XmlDictionaryReader reader)
        {
            CloseSequenceResponseInfo info = new CloseSequenceResponseInfo();
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);
            reader.ReadStartElement(DXD.Wsrm11Dictionary.CloseSequenceResponse, namespaceUri);
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
            writer.WriteStartElement(DXD.Wsrm11Dictionary.CloseSequenceResponse, namespaceUri);
            writer.WriteStartElement(XD.WsrmFeb2005Dictionary.Identifier, namespaceUri);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}

