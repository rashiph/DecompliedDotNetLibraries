namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    public abstract class Message : IDisposable
    {
        internal const int InitialBufferSize = 0x400;
        private SeekableMessageNavigator messageNavigator;
        private MessageState state;

        protected Message()
        {
        }

        internal void BodyToString(XmlDictionaryWriter writer)
        {
            this.OnBodyToString(writer);
        }

        public void Close()
        {
            if (this.state != MessageState.Closed)
            {
                this.state = MessageState.Closed;
                this.OnClose();
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80010, System.ServiceModel.SR.GetString("TraceCodeMessageClosed"), this);
                }
            }
            else if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80011, System.ServiceModel.SR.GetString("TraceCodeMessageClosedAgain"), this);
            }
        }

        public MessageBuffer CreateBufferedCopy(int maxBufferSize)
        {
            if (maxBufferSize < 0)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")), this);
            }
            switch (this.state)
            {
                case MessageState.Created:
                    this.state = MessageState.Copied;
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80012, System.ServiceModel.SR.GetString("TraceCodeMessageCopied"), this, this);
                    }
                    return this.OnCreateBufferedCopy(maxBufferSize);

                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenRead")), this);

                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenWritten")), this);

                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenCopied")), this);

                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
            }
            throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidMessageState")), this);
        }

        internal static Message CreateMessage(MessageVersion version, ActionHeader actionHeader)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            return new BodyWriterMessage(version, actionHeader, EmptyBodyWriter.Value);
        }

        public static Message CreateMessage(MessageVersion version, string action)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            return new BodyWriterMessage(version, action, EmptyBodyWriter.Value);
        }

        internal static Message CreateMessage(MessageVersion version, ActionHeader actionHeader, BodyWriter body)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            if (body == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("body"));
            }
            return new BodyWriterMessage(version, actionHeader, body);
        }

        public static Message CreateMessage(MessageVersion version, MessageFault fault, string action)
        {
            if (fault == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("fault"));
            }
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            return new BodyWriterMessage(version, action, new FaultBodyWriter(fault, version.Envelope));
        }

        public static Message CreateMessage(MessageVersion version, string action, object body)
        {
            return CreateMessage(version, action, body, DataContractSerializerDefaults.CreateSerializer(GetObjectType(body), 0x7fffffff));
        }

        public static Message CreateMessage(MessageVersion version, string action, BodyWriter body)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            if (body == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("body"));
            }
            return new BodyWriterMessage(version, action, body);
        }

        public static Message CreateMessage(MessageVersion version, string action, XmlDictionaryReader body)
        {
            if (body == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("body");
            }
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            return CreateMessage(version, action, (BodyWriter) new XmlReaderBodyWriter(body, version.Envelope));
        }

        public static Message CreateMessage(MessageVersion version, string action, XmlReader body)
        {
            return CreateMessage(version, action, XmlDictionaryReader.CreateDictionaryReader(body));
        }

        public static Message CreateMessage(XmlDictionaryReader envelopeReader, int maxSizeOfHeaders, MessageVersion version)
        {
            if (envelopeReader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("envelopeReader"));
            }
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            return new StreamedMessage(envelopeReader, maxSizeOfHeaders, version);
        }

        public static Message CreateMessage(XmlReader envelopeReader, int maxSizeOfHeaders, MessageVersion version)
        {
            return CreateMessage(XmlDictionaryReader.CreateDictionaryReader(envelopeReader), maxSizeOfHeaders, version);
        }

        public static Message CreateMessage(MessageVersion version, FaultCode faultCode, string reason, string action)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            if (faultCode == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("faultCode"));
            }
            if (reason == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reason"));
            }
            return CreateMessage(version, MessageFault.CreateFault(faultCode, reason), action);
        }

        public static Message CreateMessage(MessageVersion version, string action, object body, XmlObjectSerializer serializer)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            return new BodyWriterMessage(version, action, new XmlObjectSerializerBodyWriter(body, serializer));
        }

        public static Message CreateMessage(MessageVersion version, FaultCode faultCode, string reason, object detail, string action)
        {
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            if (faultCode == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("faultCode"));
            }
            if (reason == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reason"));
            }
            return CreateMessage(version, MessageFault.CreateFault(faultCode, new FaultReason(reason), detail), action);
        }

        internal Exception CreateMessageDisposedException()
        {
            return new ObjectDisposedException("", System.ServiceModel.SR.GetString("MessageClosed"));
        }

        public T GetBody<T>()
        {
            return this.GetBody<T>(DataContractSerializerDefaults.CreateSerializer(typeof(T), 0x7fffffff));
        }

        public T GetBody<T>(XmlObjectSerializer serializer)
        {
            T local;
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            using (XmlDictionaryReader reader = this.GetReaderAtBodyContents())
            {
                local = (T) serializer.ReadObject(reader);
                this.ReadFromBodyContentsToEnd(reader);
            }
            return local;
        }

        public string GetBodyAttribute(string localName, string ns)
        {
            if (localName == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("localName"), this);
            }
            if (ns == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("ns"), this);
            }
            switch (this.state)
            {
                case MessageState.Created:
                    return this.OnGetBodyAttribute(localName, ns);

                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenRead")), this);

                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenWritten")), this);

                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenCopied")), this);

                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
            }
            throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidMessageState")), this);
        }

        internal SeekableMessageNavigator GetNavigator(bool navigateBody, int maxNodes)
        {
            if (this.IsDisposed)
            {
                throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
            }
            if (this.messageNavigator == null)
            {
                this.messageNavigator = new SeekableMessageNavigator(this, maxNodes, XmlSpace.Default, navigateBody, false);
            }
            else
            {
                this.messageNavigator.ForkNodeCount(maxNodes);
            }
            return this.messageNavigator;
        }

        private static System.Type GetObjectType(object value)
        {
            if (value != null)
            {
                return value.GetType();
            }
            return typeof(object);
        }

        public XmlDictionaryReader GetReaderAtBodyContents()
        {
            switch (this.state)
            {
                case MessageState.Created:
                    this.state = MessageState.Read;
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80013, System.ServiceModel.SR.GetString("TraceCodeMessageRead"), this);
                    }
                    if (this.IsEmpty)
                    {
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageIsEmpty")), this);
                    }
                    return this.OnGetReaderAtBodyContents();

                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenRead")), this);

                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenWritten")), this);

                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenCopied")), this);

                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
            }
            throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidMessageState")), this);
        }

        internal virtual XmlDictionaryReader GetReaderAtHeader()
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            this.WriteStartEnvelope(writer);
            MessageHeaders headers = this.Headers;
            for (int i = 0; i < headers.Count; i++)
            {
                headers.WriteHeader(i, writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            buffer.CloseSection();
            buffer.Close();
            XmlDictionaryReader reader = buffer.GetReader(0);
            reader.ReadStartElement();
            reader.MoveToStartElement();
            return reader;
        }

        internal void InitializeReply(Message request)
        {
            UniqueId messageId = request.Headers.MessageId;
            if (messageId == null)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("RequestMessageDoesNotHaveAMessageID")), request);
            }
            this.Headers.RelatesTo = messageId;
        }

        internal static bool IsFaultStartElement(XmlDictionaryReader reader, EnvelopeVersion version)
        {
            return reader.IsStartElement(XD.MessageDictionary.Fault, version.DictionaryNamespace);
        }

        protected virtual void OnBodyToString(XmlDictionaryWriter writer)
        {
            writer.WriteString(System.ServiceModel.SR.GetString("MessageBodyIsUnknown"));
        }

        protected virtual void OnClose()
        {
        }

        protected virtual MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            return this.OnCreateBufferedCopy(maxBufferSize, XmlDictionaryReaderQuotas.Max);
        }

        internal MessageBuffer OnCreateBufferedCopy(int maxBufferSize, XmlDictionaryReaderQuotas quotas)
        {
            XmlBuffer msgBuffer = new XmlBuffer(maxBufferSize);
            XmlDictionaryWriter writer = msgBuffer.OpenSection(quotas);
            this.OnWriteMessage(writer);
            msgBuffer.CloseSection();
            msgBuffer.Close();
            return new DefaultMessageBuffer(this, msgBuffer);
        }

        protected virtual string OnGetBodyAttribute(string localName, string ns)
        {
            return null;
        }

        protected virtual XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                this.OnWriteStartEnvelope(writer);
                this.OnWriteStartBody(writer);
            }
            this.OnWriteBodyContents(writer);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            buffer.CloseSection();
            buffer.Close();
            XmlDictionaryReader reader = buffer.GetReader(0);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                reader.ReadStartElement();
                reader.ReadStartElement();
            }
            reader.MoveToContent();
            return reader;
        }

        protected abstract void OnWriteBodyContents(XmlDictionaryWriter writer);
        protected virtual void OnWriteMessage(XmlDictionaryWriter writer)
        {
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                this.OnWriteStartEnvelope(writer);
                MessageHeaders headers = this.Headers;
                int count = headers.Count;
                if (count > 0)
                {
                    this.OnWriteStartHeaders(writer);
                    for (int i = 0; i < count; i++)
                    {
                        headers.WriteHeader(i, writer);
                    }
                    writer.WriteEndElement();
                }
                this.OnWriteStartBody(writer);
            }
            this.OnWriteBodyContents(writer);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        protected virtual void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            MessageDictionary messageDictionary = XD.MessageDictionary;
            writer.WriteStartElement(messageDictionary.Prefix.Value, messageDictionary.Body, this.Version.Envelope.DictionaryNamespace);
        }

        protected virtual void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelope = this.Version.Envelope;
            if (envelope != EnvelopeVersion.None)
            {
                MessageDictionary messageDictionary = XD.MessageDictionary;
                writer.WriteStartElement(messageDictionary.Prefix.Value, messageDictionary.Envelope, envelope.DictionaryNamespace);
                this.WriteSharedHeaderPrefixes(writer);
            }
        }

        protected virtual void OnWriteStartHeaders(XmlDictionaryWriter writer)
        {
            EnvelopeVersion envelope = this.Version.Envelope;
            if (envelope != EnvelopeVersion.None)
            {
                MessageDictionary messageDictionary = XD.MessageDictionary;
                writer.WriteStartElement(messageDictionary.Prefix.Value, messageDictionary.Header, envelope.DictionaryNamespace);
            }
        }

        internal void ReadFromBodyContentsToEnd(XmlDictionaryReader reader)
        {
            ReadFromBodyContentsToEnd(reader, this.Version.Envelope);
        }

        private static void ReadFromBodyContentsToEnd(XmlDictionaryReader reader, EnvelopeVersion envelopeVersion)
        {
            if (envelopeVersion != EnvelopeVersion.None)
            {
                reader.ReadEndElement();
                reader.ReadEndElement();
            }
            reader.MoveToContent();
        }

        internal static bool ReadStartBody(XmlDictionaryReader reader, EnvelopeVersion envelopeVersion, out bool isFault, out bool isEmpty)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                isEmpty = true;
                isFault = false;
                reader.ReadEndElement();
                return false;
            }
            reader.Read();
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            if (reader.NodeType == XmlNodeType.Element)
            {
                isFault = IsFaultStartElement(reader, envelopeVersion);
                isEmpty = false;
            }
            else
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    isEmpty = true;
                    isFault = false;
                    ReadFromBodyContentsToEnd(reader, envelopeVersion);
                    return false;
                }
                isEmpty = false;
                isFault = false;
            }
            return true;
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        public override string ToString()
        {
            if (this.IsDisposed)
            {
                return base.ToString();
            }
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            EncodingFallbackAwareXmlTextWriter writer2 = new EncodingFallbackAwareXmlTextWriter(writer) {
                Formatting = Formatting.Indented
            };
            XmlDictionaryWriter writer3 = XmlDictionaryWriter.CreateDictionaryWriter(writer2);
            try
            {
                this.ToString(writer3);
                writer3.Flush();
                return writer.ToString();
            }
            catch (XmlException exception)
            {
                return System.ServiceModel.SR.GetString("MessageBodyToStringError", new object[] { exception.GetType().ToString(), exception.Message });
            }
        }

        internal void ToString(XmlDictionaryWriter writer)
        {
            if (this.IsDisposed)
            {
                throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
            }
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                this.WriteStartEnvelope(writer);
                this.WriteStartHeaders(writer);
                MessageHeaders headers = this.Headers;
                for (int i = 0; i < headers.Count; i++)
                {
                    headers.WriteHeader(i, writer);
                }
                writer.WriteEndElement();
                MessageDictionary messageDictionary = XD.MessageDictionary;
                this.WriteStartBody(writer);
            }
            this.BodyToString(writer);
            if (this.Version.Envelope != EnvelopeVersion.None)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public void WriteBody(XmlDictionaryWriter writer)
        {
            this.WriteStartBody(writer);
            this.WriteBodyContents(writer);
            writer.WriteEndElement();
        }

        public void WriteBody(XmlWriter writer)
        {
            this.WriteBody(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteBodyContents(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("writer"), this);
            }
            switch (this.state)
            {
                case MessageState.Created:
                    this.state = MessageState.Written;
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80014, System.ServiceModel.SR.GetString("TraceCodeMessageWritten"), this);
                    }
                    this.OnWriteBodyContents(writer);
                    return;

                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenRead")), this);

                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenWritten")), this);

                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenCopied")), this);

                case MessageState.Closed:
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateMessageDisposedException());
            }
            throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidMessageState")), this);
        }

        public void WriteMessage(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("writer"), this);
            }
            switch (this.state)
            {
                case MessageState.Created:
                    this.state = MessageState.Written;
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80014, System.ServiceModel.SR.GetString("TraceCodeMessageWritten"), this);
                    }
                    this.OnWriteMessage(writer);
                    return;

                case MessageState.Read:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenRead")), this);

                case MessageState.Written:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenWritten")), this);

                case MessageState.Copied:
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageHasBeenCopied")), this);

                case MessageState.Closed:
                    throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
            }
            throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidMessageState")), this);
        }

        public void WriteMessage(XmlWriter writer)
        {
            this.WriteMessage(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        private void WriteSharedHeaderPrefixes(XmlDictionaryWriter writer)
        {
            MessageHeaders headers = this.Headers;
            int count = headers.Count;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                if ((this.Version.Addressing != AddressingVersion.None) || (headers[i].Namespace != AddressingVersion.None.Namespace))
                {
                    IMessageHeaderWithSharedNamespace namespace2 = headers[i] as IMessageHeaderWithSharedNamespace;
                    if (namespace2 != null)
                    {
                        string prefix = namespace2.SharedPrefix.Value;
                        if (prefix.Length != 1)
                        {
                            throw TraceUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "IMessageHeaderWithSharedNamespace must use a single lowercase letter prefix.", new object[0])), this);
                        }
                        int num4 = prefix[0] - 'a';
                        if ((num4 < 0) || (num4 >= 0x1a))
                        {
                            throw TraceUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "IMessageHeaderWithSharedNamespace must use a single lowercase letter prefix.", new object[0])), this);
                        }
                        int num5 = ((int) 1) << num4;
                        if ((num2 & num5) == 0)
                        {
                            writer.WriteXmlnsAttribute(prefix, namespace2.SharedNamespace);
                            num2 |= num5;
                        }
                    }
                }
            }
        }

        public void WriteStartBody(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("writer"), this);
            }
            this.OnWriteStartBody(writer);
        }

        public void WriteStartBody(XmlWriter writer)
        {
            this.WriteStartBody(XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteStartEnvelope(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("writer"), this);
            }
            this.OnWriteStartEnvelope(writer);
        }

        internal void WriteStartHeaders(XmlDictionaryWriter writer)
        {
            this.OnWriteStartHeaders(writer);
        }

        public abstract MessageHeaders Headers { get; }

        protected bool IsDisposed
        {
            get
            {
                return (this.state == MessageState.Closed);
            }
        }

        public virtual bool IsEmpty
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
                }
                return false;
            }
        }

        public virtual bool IsFault
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(this.CreateMessageDisposedException(), this);
                }
                return false;
            }
        }

        public abstract MessageProperties Properties { get; }

        internal virtual System.ServiceModel.Channels.RecycledMessageState RecycledMessageState
        {
            get
            {
                return null;
            }
        }

        public MessageState State
        {
            get
            {
                return this.state;
            }
        }

        public abstract MessageVersion Version { get; }
    }
}

