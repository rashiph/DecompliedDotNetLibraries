namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class StreamedMessage : ReceivedMessage
    {
        private XmlAttributeHolder[] bodyAttributes;
        private string bodyPrefix;
        private XmlAttributeHolder[] envelopeAttributes;
        private string envelopePrefix;
        private XmlAttributeHolder[] headerAttributes;
        private string headerPrefix;
        private MessageHeaders headers;
        private MessageProperties properties = new MessageProperties();
        private XmlDictionaryReaderQuotas quotas;
        private XmlDictionaryReader reader;

        public StreamedMessage(XmlDictionaryReader reader, int maxSizeOfHeaders, MessageVersion desiredVersion)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            if (desiredVersion.Envelope == EnvelopeVersion.None)
            {
                this.reader = reader;
                this.headerAttributes = XmlAttributeHolder.emptyArray;
                this.headers = new MessageHeaders(desiredVersion);
            }
            else
            {
                this.envelopeAttributes = XmlAttributeHolder.ReadAttributes(reader, ref maxSizeOfHeaders);
                this.envelopePrefix = reader.Prefix;
                EnvelopeVersion envelopeVersion = ReceivedMessage.ReadStartEnvelope(reader);
                if (desiredVersion.Envelope != envelopeVersion)
                {
                    Exception innerException = new ArgumentException(System.ServiceModel.SR.GetString("EncoderEnvelopeVersionMismatch", new object[] { envelopeVersion, desiredVersion.Envelope }), "reader");
                    throw TraceUtility.ThrowHelperError(new CommunicationException(innerException.Message, innerException), this);
                }
                if (ReceivedMessage.HasHeaderElement(reader, envelopeVersion))
                {
                    this.headerPrefix = reader.Prefix;
                    this.headerAttributes = XmlAttributeHolder.ReadAttributes(reader, ref maxSizeOfHeaders);
                    this.headers = new MessageHeaders(desiredVersion, reader, this.envelopeAttributes, this.headerAttributes, ref maxSizeOfHeaders);
                }
                else
                {
                    this.headerAttributes = XmlAttributeHolder.emptyArray;
                    this.headers = new MessageHeaders(desiredVersion);
                }
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.MoveToContent();
                }
                this.bodyPrefix = reader.Prefix;
                ReceivedMessage.VerifyStartBody(reader, envelopeVersion);
                this.bodyAttributes = XmlAttributeHolder.ReadAttributes(reader, ref maxSizeOfHeaders);
                if (base.ReadStartBody(reader))
                {
                    this.reader = reader;
                }
                else
                {
                    this.quotas = new XmlDictionaryReaderQuotas();
                    reader.Quotas.CopyTo(this.quotas);
                    reader.Close();
                }
            }
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            writer.WriteString(System.ServiceModel.SR.GetString("MessageBodyIsStream"));
        }

        protected override void OnClose()
        {
            Exception exception = null;
            try
            {
                base.OnClose();
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                exception = exception2;
            }
            try
            {
                this.properties.Dispose();
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (exception == null)
                {
                    exception = exception3;
                }
            }
            try
            {
                if (this.reader != null)
                {
                    this.reader.Close();
                }
            }
            catch (Exception exception4)
            {
                if (Fx.IsFatal(exception4))
                {
                    throw;
                }
                if (exception == null)
                {
                    exception = exception4;
                }
            }
            if (exception != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            if (this.reader != null)
            {
                return base.OnCreateBufferedCopy(maxBufferSize, this.reader.Quotas);
            }
            return base.OnCreateBufferedCopy(maxBufferSize, this.quotas);
        }

        protected override string OnGetBodyAttribute(string localName, string ns)
        {
            return XmlAttributeHolder.GetAttribute(this.bodyAttributes, localName, ns);
        }

        protected override XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            XmlDictionaryReader reader = this.reader;
            this.reader = null;
            return reader;
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(this.bodyPrefix, "Body", this.Version.Envelope.Namespace);
            XmlAttributeHolder.WriteAttributes(this.bodyAttributes, writer);
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelope = this.Version.Envelope;
            writer.WriteStartElement(this.envelopePrefix, "Envelope", envelope.Namespace);
            XmlAttributeHolder.WriteAttributes(this.envelopeAttributes, writer);
        }

        protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelope = this.Version.Envelope;
            writer.WriteStartElement(this.headerPrefix, "Header", envelope.Namespace);
            XmlAttributeHolder.WriteAttributes(this.headerAttributes, writer);
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                return this.headers;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                return this.properties;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                return this.headers.MessageVersion;
            }
        }
    }
}

