namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class WsrmSequencedMessageInfo : WsrmHeaderInfo
    {
        private bool lastMessage;
        private UniqueId sequenceID;
        private long sequenceNumber;

        private WsrmSequencedMessageInfo(UniqueId sequenceID, long sequenceNumber, bool lastMessage, MessageHeaderInfo header) : base(header)
        {
            this.sequenceID = sequenceID;
            this.sequenceNumber = sequenceNumber;
            this.lastMessage = lastMessage;
        }

        public static WsrmSequencedMessageInfo ReadHeader(ReliableMessagingVersion reliableMessagingVersion, XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            reader.ReadStartElement();
            reader.ReadStartElement(dictionary.Identifier, namespaceUri);
            UniqueId sequenceID = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            reader.ReadStartElement(dictionary.MessageNumber, namespaceUri);
            long sequenceNumber = WsrmUtilities.ReadSequenceNumber(reader);
            reader.ReadEndElement();
            bool lastMessage = false;
            if ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && reader.IsStartElement(dictionary.LastMessage, namespaceUri))
            {
                WsrmUtilities.ReadEmptyElement(reader);
                lastMessage = true;
            }
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();
            return new WsrmSequencedMessageInfo(sequenceID, sequenceNumber, lastMessage, header);
        }

        public bool LastMessage
        {
            get
            {
                return this.lastMessage;
            }
        }

        public UniqueId SequenceID
        {
            get
            {
                return this.sequenceID;
            }
        }

        public long SequenceNumber
        {
            get
            {
                return this.sequenceNumber;
            }
        }
    }
}

