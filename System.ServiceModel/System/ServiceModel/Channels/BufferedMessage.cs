namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class BufferedMessage : ReceivedMessage
    {
        private XmlAttributeHolder[] bodyAttributes;
        private MessageHeaders headers;
        private IBufferedMessageData messageData;
        private MessageProperties properties;
        private XmlDictionaryReader reader;
        private System.ServiceModel.Channels.RecycledMessageState recycledMessageState;

        public BufferedMessage(IBufferedMessageData messageData, System.ServiceModel.Channels.RecycledMessageState recycledMessageState) : this(messageData, recycledMessageState, null, false)
        {
        }

        public BufferedMessage(IBufferedMessageData messageData, System.ServiceModel.Channels.RecycledMessageState recycledMessageState, bool[] understoodHeaders, bool understoodHeadersModified)
        {
            bool flag = true;
            try
            {
                this.recycledMessageState = recycledMessageState;
                this.messageData = messageData;
                this.properties = recycledMessageState.TakeProperties();
                if (this.properties == null)
                {
                    this.properties = new MessageProperties();
                }
                XmlDictionaryReader messageReader = messageData.GetMessageReader();
                MessageVersion messageVersion = messageData.MessageEncoder.MessageVersion;
                if (messageVersion.Envelope == EnvelopeVersion.None)
                {
                    this.reader = messageReader;
                    this.headers = new MessageHeaders(messageVersion);
                }
                else
                {
                    EnvelopeVersion envelopeVersion = ReceivedMessage.ReadStartEnvelope(messageReader);
                    if (messageVersion.Envelope != envelopeVersion)
                    {
                        Exception innerException = new ArgumentException(System.ServiceModel.SR.GetString("EncoderEnvelopeVersionMismatch", new object[] { envelopeVersion, messageVersion.Envelope }), "reader");
                        throw TraceUtility.ThrowHelperError(new CommunicationException(innerException.Message, innerException), this);
                    }
                    if (ReceivedMessage.HasHeaderElement(messageReader, envelopeVersion))
                    {
                        this.headers = recycledMessageState.TakeHeaders();
                        if (this.headers == null)
                        {
                            this.headers = new MessageHeaders(messageVersion, messageReader, messageData, recycledMessageState, understoodHeaders, understoodHeadersModified);
                        }
                        else
                        {
                            this.headers.Init(messageVersion, messageReader, messageData, recycledMessageState, understoodHeaders, understoodHeadersModified);
                        }
                    }
                    else
                    {
                        this.headers = new MessageHeaders(messageVersion);
                    }
                    ReceivedMessage.VerifyStartBody(messageReader, envelopeVersion);
                    int maxSizeOfHeaders = 0x7fffffff;
                    this.bodyAttributes = XmlAttributeHolder.ReadAttributes(messageReader, ref maxSizeOfHeaders);
                    if (maxSizeOfHeaders < 0x7fffefff)
                    {
                        this.bodyAttributes = null;
                    }
                    if (base.ReadStartBody(messageReader))
                    {
                        this.reader = messageReader;
                    }
                    else
                    {
                        messageReader.Close();
                    }
                }
                flag = false;
            }
            finally
            {
                if (flag && MessageLogger.LoggingEnabled)
                {
                    MessageLogger.LogMessage(messageData.Buffer, MessageLoggingSource.Malformed);
                }
            }
        }

        public XmlDictionaryReader GetBufferedReaderAtBody()
        {
            XmlDictionaryReader messageReader = this.messageData.GetMessageReader();
            if (messageReader.NodeType != XmlNodeType.Element)
            {
                messageReader.MoveToContent();
            }
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                messageReader.Read();
                if (ReceivedMessage.HasHeaderElement(messageReader, this.headers.MessageVersion.Envelope))
                {
                    messageReader.Skip();
                }
                if (messageReader.NodeType != XmlNodeType.Element)
                {
                    messageReader.MoveToContent();
                }
            }
            return messageReader;
        }

        public XmlDictionaryReader GetMessageReader()
        {
            return this.messageData.GetMessageReader();
        }

        internal override XmlDictionaryReader GetReaderAtHeader()
        {
            if (this.headers.ContainsOnlyBufferedMessageHeaders)
            {
                XmlDictionaryReader messageReader = this.messageData.GetMessageReader();
                if (messageReader.NodeType != XmlNodeType.Element)
                {
                    messageReader.MoveToContent();
                }
                messageReader.Read();
                if (ReceivedMessage.HasHeaderElement(messageReader, this.headers.MessageVersion.Envelope))
                {
                    return messageReader;
                }
            }
            return base.GetReaderAtHeader();
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = this.GetBufferedReaderAtBody())
            {
                if (this.Version == MessageVersion.None)
                {
                    writer.WriteNode(reader, false);
                }
                else if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        writer.WriteNode(reader, false);
                    }
                }
            }
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
            try
            {
                this.recycledMessageState.ReturnHeaders(this.headers);
                this.recycledMessageState.ReturnProperties(this.properties);
                this.messageData.ReturnMessageState(this.recycledMessageState);
                this.recycledMessageState = null;
                this.messageData.Close();
                this.messageData = null;
            }
            catch (Exception exception5)
            {
                if (Fx.IsFatal(exception5))
                {
                    throw;
                }
                if (exception == null)
                {
                    exception = exception5;
                }
            }
            if (exception != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            if (this.headers.ContainsOnlyBufferedMessageHeaders)
            {
                KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[this.Properties.Count];
                ((ICollection<KeyValuePair<string, object>>) this.Properties).CopyTo(array, 0);
                this.messageData.EnableMultipleUsers();
                bool[] understoodHeaders = null;
                if (this.headers.HasMustUnderstandBeenModified)
                {
                    understoodHeaders = new bool[this.headers.Count];
                    for (int i = 0; i < this.headers.Count; i++)
                    {
                        understoodHeaders[i] = this.headers.IsUnderstood(i);
                    }
                }
                return new BufferedMessageBuffer(this.messageData, array, understoodHeaders, this.headers.HasMustUnderstandBeenModified);
            }
            if (this.reader != null)
            {
                return base.OnCreateBufferedCopy(maxBufferSize, this.reader.Quotas);
            }
            return base.OnCreateBufferedCopy(maxBufferSize, XmlDictionaryReaderQuotas.Max);
        }

        protected override string OnGetBodyAttribute(string localName, string ns)
        {
            if (this.bodyAttributes != null)
            {
                return XmlAttributeHolder.GetAttribute(this.bodyAttributes, localName, ns);
            }
            using (XmlDictionaryReader reader = this.GetBufferedReaderAtBody())
            {
                return reader.GetAttribute(localName, ns);
            }
        }

        protected override XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            XmlDictionaryReader reader = this.reader;
            this.reader = null;
            return reader;
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = this.GetBufferedReaderAtBody())
            {
                writer.WriteStartElement(reader.Prefix, "Body", this.Version.Envelope.Namespace);
                writer.WriteAttributes(reader, false);
            }
        }

        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = this.GetMessageReader())
            {
                reader.MoveToContent();
                EnvelopeVersion envelope = this.Version.Envelope;
                writer.WriteStartElement(reader.Prefix, "Envelope", envelope.Namespace);
                writer.WriteAttributes(reader, false);
            }
        }

        protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            using (XmlDictionaryReader reader = this.GetMessageReader())
            {
                reader.MoveToContent();
                EnvelopeVersion envelope = this.Version.Envelope;
                reader.Read();
                if (ReceivedMessage.HasHeaderElement(reader, envelope))
                {
                    writer.WriteStartElement(reader.Prefix, "Header", envelope.Namespace);
                    writer.WriteAttributes(reader, false);
                }
                else
                {
                    writer.WriteStartElement("s", "Header", envelope.Namespace);
                }
            }
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

        internal IBufferedMessageData MessageData
        {
            get
            {
                return this.messageData;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                return this.properties;
            }
        }

        internal override System.ServiceModel.Channels.RecycledMessageState RecycledMessageState
        {
            get
            {
                return this.recycledMessageState;
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

