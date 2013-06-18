namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class CloseSequence : BodyWriter
    {
        private UniqueId identifier;
        private long lastMsgNumber;

        public CloseSequence(UniqueId identifier, long lastMsgNumber) : base(true)
        {
            this.identifier = identifier;
            this.lastMsgNumber = lastMsgNumber;
        }

        public static CloseSequenceInfo Create(XmlDictionaryReader reader)
        {
            CloseSequenceInfo info = new CloseSequenceInfo();
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(ReliableMessagingVersion.WSReliableMessaging11);
            Wsrm11Dictionary dictionary = DXD.Wsrm11Dictionary;
            reader.ReadStartElement(dictionary.CloseSequence, namespaceUri);
            reader.ReadStartElement(XD.WsrmFeb2005Dictionary.Identifier, namespaceUri);
            info.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            if (reader.IsStartElement(dictionary.LastMsgNumber, namespaceUri))
            {
                reader.ReadStartElement();
                info.LastMsgNumber = WsrmUtilities.ReadSequenceNumber(reader, false);
                reader.ReadEndElement();
            }
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
            Wsrm11Dictionary dictionary = DXD.Wsrm11Dictionary;
            writer.WriteStartElement(dictionary.CloseSequence, namespaceUri);
            writer.WriteStartElement(XD.WsrmFeb2005Dictionary.Identifier, namespaceUri);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();
            if (this.lastMsgNumber > 0L)
            {
                writer.WriteStartElement(dictionary.LastMsgNumber, namespaceUri);
                writer.WriteValue(this.lastMsgNumber);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}

