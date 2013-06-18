namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class WsrmAcknowledgmentHeader : WsrmMessageHeader
    {
        private int bufferRemaining;
        private bool final;
        private SequenceRangeCollection ranges;
        private UniqueId sequenceID;

        public WsrmAcknowledgmentHeader(ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceID, SequenceRangeCollection ranges, bool final, int bufferRemaining) : base(reliableMessagingVersion)
        {
            this.sequenceID = sequenceID;
            this.ranges = ranges;
            this.final = final;
            this.bufferRemaining = bufferRemaining;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString dictionaryNamespace = this.DictionaryNamespace;
            WriteAckRanges(writer, base.ReliableMessagingVersion, this.sequenceID, this.ranges);
            if ((base.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && this.final)
            {
                writer.WriteStartElement(DXD.Wsrm11Dictionary.Final, dictionaryNamespace);
                writer.WriteEndElement();
            }
            if (this.bufferRemaining != -1)
            {
                writer.WriteStartElement("netrm", dictionary.BufferRemaining, XD.WsrmFeb2005Dictionary.NETNamespace);
                writer.WriteValue(this.bufferRemaining);
                writer.WriteEndElement();
            }
        }

        internal static void WriteAckRanges(XmlDictionaryWriter writer, ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceId, SequenceRangeCollection ranges)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            writer.WriteStartElement(dictionary.Identifier, namespaceUri);
            writer.WriteValue(sequenceId);
            writer.WriteEndElement();
            if (ranges.Count == 0)
            {
                if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    ranges = ranges.MergeWith((long) 0L);
                }
                else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                {
                    writer.WriteStartElement(DXD.Wsrm11Dictionary.None, namespaceUri);
                    writer.WriteEndElement();
                }
            }
            for (int i = 0; i < ranges.Count; i++)
            {
                writer.WriteStartElement(dictionary.AcknowledgementRange, namespaceUri);
                writer.WriteStartAttribute(dictionary.Lower, null);
                SequenceRange range = ranges[i];
                writer.WriteValue(range.Lower);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute(dictionary.Upper, null);
                SequenceRange range2 = ranges[i];
                writer.WriteValue(range2.Upper);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.WsrmFeb2005Dictionary.SequenceAcknowledgement;
            }
        }
    }
}

