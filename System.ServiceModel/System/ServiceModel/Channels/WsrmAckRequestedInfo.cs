namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class WsrmAckRequestedInfo : WsrmHeaderInfo
    {
        private UniqueId sequenceID;

        public WsrmAckRequestedInfo(UniqueId sequenceID, MessageHeaderInfo header) : base(header)
        {
            this.sequenceID = sequenceID;
        }

        public static WsrmAckRequestedInfo ReadHeader(ReliableMessagingVersion reliableMessagingVersion, XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            reader.ReadStartElement();
            reader.ReadStartElement(dictionary.Identifier, namespaceUri);
            UniqueId sequenceID = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            if ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && reader.IsStartElement(dictionary.MessageNumber, namespaceUri))
            {
                reader.ReadStartElement();
                WsrmUtilities.ReadSequenceNumber(reader, true);
                reader.ReadEndElement();
            }
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();
            return new WsrmAckRequestedInfo(sequenceID, header);
        }

        public UniqueId SequenceID
        {
            get
            {
                return this.sequenceID;
            }
        }
    }
}

