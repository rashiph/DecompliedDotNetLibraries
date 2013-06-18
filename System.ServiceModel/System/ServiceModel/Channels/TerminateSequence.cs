namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class TerminateSequence : BodyWriter
    {
        private UniqueId identifier;
        private long lastMsgNumber;
        private ReliableMessagingVersion reliableMessagingVersion;

        public TerminateSequence() : base(true)
        {
        }

        public TerminateSequence(ReliableMessagingVersion reliableMessagingVersion, UniqueId identifier, long last) : base(true)
        {
            this.reliableMessagingVersion = reliableMessagingVersion;
            this.identifier = identifier;
            this.lastMsgNumber = last;
        }

        public static TerminateSequenceInfo Create(ReliableMessagingVersion reliableMessagingVersion, XmlDictionaryReader reader)
        {
            TerminateSequenceInfo info = new TerminateSequenceInfo();
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            reader.ReadStartElement(dictionary.TerminateSequence, namespaceUri);
            reader.ReadStartElement(dictionary.Identifier, namespaceUri);
            info.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            if ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && reader.IsStartElement(DXD.Wsrm11Dictionary.LastMsgNumber, namespaceUri))
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
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(this.reliableMessagingVersion);
            writer.WriteStartElement(dictionary.TerminateSequence, namespaceUri);
            writer.WriteStartElement(dictionary.Identifier, namespaceUri);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();
            if ((this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (this.lastMsgNumber > 0L))
            {
                writer.WriteStartElement(DXD.Wsrm11Dictionary.LastMsgNumber, namespaceUri);
                writer.WriteValue(this.lastMsgNumber);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}

