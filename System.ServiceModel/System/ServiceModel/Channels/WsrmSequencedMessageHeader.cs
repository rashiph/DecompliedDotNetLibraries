namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class WsrmSequencedMessageHeader : WsrmMessageHeader
    {
        private bool lastMessage;
        private UniqueId sequenceID;
        private long sequenceNumber;

        public WsrmSequencedMessageHeader(ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceID, long sequenceNumber, bool lastMessage) : base(reliableMessagingVersion)
        {
            this.sequenceID = sequenceID;
            this.sequenceNumber = sequenceNumber;
            this.lastMessage = lastMessage;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString dictionaryNamespace = this.DictionaryNamespace;
            writer.WriteStartElement(dictionary.Identifier, dictionaryNamespace);
            writer.WriteValue(this.sequenceID);
            writer.WriteEndElement();
            writer.WriteStartElement(dictionary.MessageNumber, dictionaryNamespace);
            writer.WriteValue(this.sequenceNumber);
            writer.WriteEndElement();
            if ((base.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && this.lastMessage)
            {
                writer.WriteStartElement(dictionary.LastMessage, dictionaryNamespace);
                writer.WriteEndElement();
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.WsrmFeb2005Dictionary.Sequence;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return true;
            }
        }
    }
}

