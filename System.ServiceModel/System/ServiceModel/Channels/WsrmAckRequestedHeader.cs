namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class WsrmAckRequestedHeader : WsrmMessageHeader
    {
        private UniqueId sequenceID;

        public WsrmAckRequestedHeader(ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceID) : base(reliableMessagingVersion)
        {
            this.sequenceID = sequenceID;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString dictionaryNamespace = this.DictionaryNamespace;
            writer.WriteStartElement(dictionary.Identifier, dictionaryNamespace);
            writer.WriteValue(this.sequenceID);
            writer.WriteEndElement();
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.WsrmFeb2005Dictionary.AckRequested;
            }
        }
    }
}

