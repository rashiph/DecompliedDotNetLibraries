namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class InvalidAcknowledgementFault : WsrmHeaderFault
    {
        private SequenceRangeCollection ranges;

        public InvalidAcknowledgementFault(UniqueId sequenceID, SequenceRangeCollection ranges) : base(true, "InvalidAcknowledgement", System.ServiceModel.SR.GetString("InvalidAcknowledgementFaultReason"), System.ServiceModel.SR.GetString("InvalidAcknowledgementReceived"), sequenceID, true, false)
        {
            this.ranges = ranges;
        }

        public InvalidAcknowledgementFault(FaultCode code, FaultReason reason, XmlDictionaryReader detailReader, ReliableMessagingVersion reliableMessagingVersion) : base(code, "InvalidAcknowledgement", reason, true, false)
        {
            UniqueId id;
            bool flag;
            WsrmAcknowledgmentInfo.ReadAck(reliableMessagingVersion, detailReader, out id, out this.ranges, out flag);
            base.SequenceID = id;
            while (detailReader.IsStartElement())
            {
                detailReader.Skip();
            }
            detailReader.ReadEndElement();
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            ReliableMessagingVersion reliableMessagingVersion = base.GetReliableMessagingVersion();
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            writer.WriteStartElement(dictionary.SequenceAcknowledgement, namespaceUri);
            WsrmAcknowledgmentHeader.WriteAckRanges(writer, reliableMessagingVersion, base.SequenceID, this.ranges);
            writer.WriteEndElement();
        }
    }
}

