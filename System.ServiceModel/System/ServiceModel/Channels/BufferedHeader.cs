namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class BufferedHeader : ReadableMessageHeader
    {
        private string actor;
        private XmlBuffer buffer;
        private int bufferIndex;
        private bool isRefParam;
        private bool mustUnderstand;
        private string name;
        private string ns;
        private bool relay;
        private bool streamed;
        private MessageVersion version;

        public BufferedHeader(MessageVersion version, XmlBuffer buffer, int bufferIndex, MessageHeaderInfo headerInfo)
        {
            this.version = version;
            this.buffer = buffer;
            this.bufferIndex = bufferIndex;
            this.actor = headerInfo.Actor;
            this.relay = headerInfo.Relay;
            this.name = headerInfo.Name;
            this.ns = headerInfo.Namespace;
            this.isRefParam = headerInfo.IsReferenceParameter;
            this.mustUnderstand = headerInfo.MustUnderstand;
        }

        public BufferedHeader(MessageVersion version, XmlBuffer buffer, XmlDictionaryReader reader, XmlAttributeHolder[] envelopeAttributes, XmlAttributeHolder[] headerAttributes)
        {
            this.streamed = true;
            this.buffer = buffer;
            this.version = version;
            MessageHeader.GetHeaderAttributes(reader, version, out this.actor, out this.mustUnderstand, out this.relay, out this.isRefParam);
            this.name = reader.LocalName;
            this.ns = reader.NamespaceURI;
            this.bufferIndex = buffer.SectionCount;
            XmlDictionaryWriter writer = buffer.OpenSection(reader.Quotas);
            writer.WriteStartElement("Envelope");
            if (envelopeAttributes != null)
            {
                XmlAttributeHolder.WriteAttributes(envelopeAttributes, writer);
            }
            writer.WriteStartElement("Header");
            if (headerAttributes != null)
            {
                XmlAttributeHolder.WriteAttributes(headerAttributes, writer);
            }
            writer.WriteNode(reader, false);
            writer.WriteEndElement();
            writer.WriteEndElement();
            buffer.CloseSection();
        }

        public BufferedHeader(MessageVersion version, XmlBuffer buffer, int bufferIndex, string name, string ns, bool mustUnderstand, string actor, bool relay, bool isRefParam)
        {
            this.version = version;
            this.buffer = buffer;
            this.bufferIndex = bufferIndex;
            this.name = name;
            this.ns = ns;
            this.mustUnderstand = mustUnderstand;
            this.actor = actor;
            this.relay = relay;
            this.isRefParam = isRefParam;
        }

        public override XmlDictionaryReader GetHeaderReader()
        {
            XmlDictionaryReader reader = this.buffer.GetReader(this.bufferIndex);
            if (this.streamed)
            {
                reader.MoveToContent();
                reader.Read();
                reader.Read();
                reader.MoveToContent();
            }
            return reader;
        }

        public override bool IsMessageVersionSupported(MessageVersion messageVersion)
        {
            if (messageVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageVersion"));
            }
            return (messageVersion == this.version);
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public override bool IsReferenceParameter
        {
            get
            {
                return this.isRefParam;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.relay;
            }
        }
    }
}

