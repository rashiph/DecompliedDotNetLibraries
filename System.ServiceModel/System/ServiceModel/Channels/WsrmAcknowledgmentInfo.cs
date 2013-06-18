namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class WsrmAcknowledgmentInfo : WsrmHeaderInfo
    {
        private int bufferRemaining;
        private bool final;
        private SequenceRangeCollection ranges;
        private UniqueId sequenceID;

        private WsrmAcknowledgmentInfo(UniqueId sequenceID, SequenceRangeCollection ranges, bool final, int bufferRemaining, MessageHeaderInfo header) : base(header)
        {
            this.sequenceID = sequenceID;
            this.ranges = ranges;
            this.final = final;
            this.bufferRemaining = bufferRemaining;
        }

        internal static void ReadAck(ReliableMessagingVersion reliableMessagingVersion, XmlDictionaryReader reader, out UniqueId sequenceId, out SequenceRangeCollection rangeCollection, out bool final)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            reader.ReadStartElement(dictionary.SequenceAcknowledgement, namespaceUri);
            reader.ReadStartElement(dictionary.Identifier, namespaceUri);
            sequenceId = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            bool allowZero = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
            rangeCollection = SequenceRangeCollection.Empty;
            while (reader.IsStartElement(dictionary.AcknowledgementRange, namespaceUri))
            {
                reader.MoveToAttribute("Lower");
                long lower = WsrmUtilities.ReadSequenceNumber(reader, allowZero);
                reader.MoveToAttribute("Upper");
                long upper = WsrmUtilities.ReadSequenceNumber(reader, allowZero);
                if ((((lower < 0L) || (lower > upper)) || (((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && (lower == 0L)) && (upper > 0L))) || ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (lower == 0L)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidSequenceRange", new object[] { lower, upper })));
                }
                rangeCollection = rangeCollection.MergeWith(new SequenceRange(lower, upper));
                reader.MoveToElement();
                WsrmUtilities.ReadEmptyElement(reader);
            }
            bool flag2 = rangeCollection.Count > 0;
            final = false;
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                Wsrm11Dictionary dictionary2 = DXD.Wsrm11Dictionary;
                if (reader.IsStartElement(dictionary2.None, namespaceUri))
                {
                    if (flag2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, dictionary.SequenceAcknowledgement })));
                    }
                    WsrmUtilities.ReadEmptyElement(reader);
                    flag2 = true;
                }
                if (reader.IsStartElement(dictionary2.Final, namespaceUri))
                {
                    if (!flag2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, dictionary.SequenceAcknowledgement })));
                    }
                    WsrmUtilities.ReadEmptyElement(reader);
                    final = true;
                }
            }
            bool flag4 = false;
            while (reader.IsStartElement(dictionary.Nack, namespaceUri))
            {
                if (flag2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, "Body" })));
                }
                reader.ReadStartElement();
                WsrmUtilities.ReadSequenceNumber(reader, true);
                reader.ReadEndElement();
                flag4 = true;
            }
            if (!flag2 && !flag4)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, "Body" })));
            }
        }

        public static WsrmAcknowledgmentInfo ReadHeader(ReliableMessagingVersion reliableMessagingVersion, XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            UniqueId id;
            SequenceRangeCollection ranges;
            bool flag;
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            ReadAck(reliableMessagingVersion, reader, out id, out ranges, out flag);
            int bufferRemaining = -1;
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(dictionary.BufferRemaining, XD.WsrmFeb2005Dictionary.NETNamespace))
                {
                    if (bufferRemaining != -1)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, "Body" })));
                    }
                    reader.ReadStartElement();
                    bufferRemaining = reader.ReadContentAsInt();
                    reader.ReadEndElement();
                    if (bufferRemaining < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidBufferRemaining", new object[] { bufferRemaining })));
                    }
                }
                else
                {
                    if (reader.IsStartElement(dictionary.AcknowledgementRange, namespaceUri))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, "Body" })));
                    }
                    if (reader.IsStartElement(dictionary.Nack, namespaceUri))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, "Body" })));
                    }
                    if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                    {
                        Wsrm11Dictionary dictionary2 = DXD.Wsrm11Dictionary;
                        if (reader.IsStartElement(dictionary2.None, namespaceUri) || reader.IsStartElement(dictionary2.Final, namespaceUri))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { reader.Name, reader.NodeType, dictionary.SequenceAcknowledgement })));
                        }
                    }
                    reader.Skip();
                }
            }
            reader.ReadEndElement();
            return new WsrmAcknowledgmentInfo(id, ranges, flag, bufferRemaining, header);
        }

        public int BufferRemaining
        {
            get
            {
                return this.bufferRemaining;
            }
        }

        public bool Final
        {
            get
            {
                return this.final;
            }
        }

        public SequenceRangeCollection Ranges
        {
            get
            {
                return this.ranges;
            }
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

