namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net.Mime;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;

    internal class TextMessageEncoderFactory : MessageEncoderFactory
    {
        private TextMessageEncoder messageEncoder;
        internal static ContentEncoding[] Soap11Content = GetContentEncodingMap(System.ServiceModel.Channels.MessageVersion.Soap11WSAddressing10);
        internal const string Soap11MediaType = "text/xml";
        internal static ContentEncoding[] Soap12Content = GetContentEncodingMap(System.ServiceModel.Channels.MessageVersion.Soap12WSAddressing10);
        internal const string Soap12MediaType = "application/soap+xml";
        internal static ContentEncoding[] SoapNoneContent = GetContentEncodingMap(System.ServiceModel.Channels.MessageVersion.None);
        private const string XmlMediaType = "application/xml";

        public TextMessageEncoderFactory(System.ServiceModel.Channels.MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas)
        {
            this.messageEncoder = new TextMessageEncoder(version, writeEncoding, maxReadPoolSize, maxWritePoolSize, quotas);
        }

        private static ContentEncoding[] GetContentEncodingMap(System.ServiceModel.Channels.MessageVersion version)
        {
            Encoding[] supportedEncodings = GetSupportedEncodings();
            string mediaType = GetMediaType(version);
            ContentEncoding[] encodingArray2 = new ContentEncoding[supportedEncodings.Length];
            for (int i = 0; i < supportedEncodings.Length; i++)
            {
                encodingArray2[i] = new ContentEncoding { contentType = GetContentType(mediaType, supportedEncodings[i]), encoding = supportedEncodings[i] };
            }
            return encodingArray2;
        }

        internal static string GetContentType(string mediaType, Encoding encoding)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}; charset={1}", new object[] { mediaType, TextEncoderDefaults.EncodingToCharSet(encoding) });
        }

        internal static Encoding GetEncodingFromContentType(string contentType, ContentEncoding[] contentMap)
        {
            string charSet;
            Encoding encoding;
            if (contentType == null)
            {
                return null;
            }
            for (int i = 0; i < contentMap.Length; i++)
            {
                if (contentMap[i].contentType == contentType)
                {
                    return contentMap[i].encoding;
                }
            }
            int index = contentType.IndexOf(';');
            if (index == -1)
            {
                return null;
            }
            int startIndex = -1;
            if (((contentType.Length > (index + 11)) && (contentType[index + 2] == 'c')) && (string.Compare("charset=", 0, contentType, index + 2, 8, StringComparison.OrdinalIgnoreCase) == 0))
            {
                startIndex = index + 10;
            }
            else
            {
                int num4 = contentType.IndexOf("charset=", index + 1, StringComparison.OrdinalIgnoreCase);
                if (num4 != -1)
                {
                    for (int j = num4 - 1; j >= index; j--)
                    {
                        if (contentType[j] == ';')
                        {
                            startIndex = num4 + 8;
                            break;
                        }
                        if (contentType[j] == '\n')
                        {
                            if ((j == index) || (contentType[j - 1] != '\r'))
                            {
                                break;
                            }
                            j--;
                        }
                        else if ((contentType[j] != ' ') && (contentType[j] != '\t'))
                        {
                            break;
                        }
                    }
                }
            }
            if (startIndex != -1)
            {
                index = contentType.IndexOf(';', startIndex);
                if (index == -1)
                {
                    charSet = contentType.Substring(startIndex);
                }
                else
                {
                    charSet = contentType.Substring(startIndex, index - startIndex);
                }
                if (((charSet.Length > 2) && (charSet[0] == '"')) && (charSet[charSet.Length - 1] == '"'))
                {
                    charSet = charSet.Substring(1, charSet.Length - 2);
                }
                if (TryGetEncodingFromCharSet(charSet, out encoding))
                {
                    return encoding;
                }
            }
            try
            {
                ContentType type = new ContentType(contentType);
                charSet = type.CharSet;
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("EncoderBadContentType"), exception));
            }
            if (!TryGetEncodingFromCharSet(charSet, out encoding))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("EncoderUnrecognizedCharSet", new object[] { charSet })));
            }
            return encoding;
        }

        internal static string GetMediaType(System.ServiceModel.Channels.MessageVersion version)
        {
            if (version.Envelope == EnvelopeVersion.Soap12)
            {
                return "application/soap+xml";
            }
            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                return "text/xml";
            }
            if (version.Envelope != EnvelopeVersion.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EnvelopeVersionNotSupported", new object[] { version.Envelope })));
            }
            return "application/xml";
        }

        public static Encoding[] GetSupportedEncodings()
        {
            Encoding[] supportedEncodings = TextEncoderDefaults.SupportedEncodings;
            Encoding[] destinationArray = new Encoding[supportedEncodings.Length];
            Array.Copy(supportedEncodings, destinationArray, supportedEncodings.Length);
            return destinationArray;
        }

        internal static bool TryGetEncodingFromCharSet(string charSet, out Encoding encoding)
        {
            encoding = null;
            if ((charSet != null) && (charSet.Length != 0))
            {
                return TextEncoderDefaults.TryGetEncoding(charSet, out encoding);
            }
            return true;
        }

        public override MessageEncoder Encoder
        {
            get
            {
                return this.messageEncoder;
            }
        }

        public int MaxReadPoolSize
        {
            get
            {
                return this.messageEncoder.MaxReadPoolSize;
            }
        }

        public int MaxWritePoolSize
        {
            get
            {
                return this.messageEncoder.MaxWritePoolSize;
            }
        }

        public override System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageEncoder.MessageVersion;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.messageEncoder.ReaderQuotas;
            }
        }

        internal class ContentEncoding
        {
            internal string contentType;
            internal Encoding encoding;
        }

        private class TextMessageEncoder : MessageEncoder
        {
            private SynchronizedPool<UTF8BufferedMessageData> bufferedReaderPool;
            private SynchronizedPool<TextBufferedMessageWriter> bufferedWriterPool;
            private TextMessageEncoderFactory.ContentEncoding[] contentEncodingMap;
            private string contentType;
            private static readonly byte[] encodingText = new byte[] { 0x65, 110, 0x63, 0x6f, 100, 0x69, 110, 0x67, 0x3d };
            private const int maxPooledXmlReadersPerMessage = 2;
            private int maxReadPoolSize;
            private int maxWritePoolSize;
            private string mediaType;
            private OnXmlDictionaryReaderClose onStreamedReaderClose;
            private bool optimizeWriteForUTF8;
            private XmlDictionaryReaderQuotas readerQuotas;
            private SynchronizedPool<RecycledMessageState> recycledStatePool;
            private SynchronizedPool<XmlDictionaryReader> streamedReaderPool;
            private SynchronizedPool<XmlDictionaryWriter> streamedWriterPool;
            private object thisLock;
            private System.ServiceModel.Channels.MessageVersion version;
            private static readonly byte[] version10Text = new byte[] { 0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 110, 0x3d, 0x22, 0x31, 0x2e, 0x30, 0x22 };
            private Encoding writeEncoding;
            private static readonly byte[] xmlDeclarationStartText = new byte[] { 60, 0x3f, 120, 0x6d, 0x6c };

            public TextMessageEncoder(System.ServiceModel.Channels.MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, XmlDictionaryReaderQuotas quotas)
            {
                if (version == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
                }
                if (writeEncoding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");
                }
                TextEncoderDefaults.ValidateEncoding(writeEncoding);
                this.writeEncoding = writeEncoding;
                this.optimizeWriteForUTF8 = IsUTF8Encoding(writeEncoding);
                this.thisLock = new object();
                this.version = version;
                this.maxReadPoolSize = maxReadPoolSize;
                this.maxWritePoolSize = maxWritePoolSize;
                this.readerQuotas = new XmlDictionaryReaderQuotas();
                quotas.CopyTo(this.readerQuotas);
                this.onStreamedReaderClose = new OnXmlDictionaryReaderClose(this.ReturnStreamedReader);
                this.mediaType = TextMessageEncoderFactory.GetMediaType(version);
                this.contentType = TextMessageEncoderFactory.GetContentType(this.mediaType, writeEncoding);
                if (version.Envelope == EnvelopeVersion.Soap12)
                {
                    this.contentEncodingMap = TextMessageEncoderFactory.Soap12Content;
                }
                else if (version.Envelope == EnvelopeVersion.Soap11)
                {
                    this.contentEncodingMap = TextMessageEncoderFactory.Soap11Content;
                }
                else
                {
                    if (version.Envelope != EnvelopeVersion.None)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EnvelopeVersionNotSupported", new object[] { version.Envelope })));
                    }
                    this.contentEncodingMap = TextMessageEncoderFactory.SoapNoneContent;
                }
            }

            private XmlDictionaryWriter CreateWriter(Stream stream)
            {
                return XmlDictionaryWriter.CreateTextWriter(stream, this.writeEncoding, false);
            }

            internal override bool IsCharSetSupported(string charSet)
            {
                Encoding encoding;
                return TextEncoderDefaults.TryGetEncoding(charSet, out encoding);
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                if (contentType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contentType");
                }
                if (base.IsContentTypeSupported(contentType))
                {
                    return true;
                }
                if (this.MessageVersion == System.ServiceModel.Channels.MessageVersion.None)
                {
                    if (base.IsContentTypeSupported(contentType, "text/xml", "text/xml"))
                    {
                        return true;
                    }
                    if (base.IsContentTypeSupported(contentType, "application/rss+xml", "application/rss+xml"))
                    {
                        return true;
                    }
                    if (base.IsContentTypeSupported(contentType, "text/html", "application/atom+xml"))
                    {
                        return true;
                    }
                    if (base.IsContentTypeSupported(contentType, "application/atom+xml", "application/atom+xml"))
                    {
                        return true;
                    }
                }
                return false;
            }

            private static bool IsUTF8Encoding(Encoding encoding)
            {
                return (encoding.WebName == "utf-8");
            }

            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                if (bufferManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bufferManager"));
                }
                UTF8BufferedMessageData messageData = this.TakeBufferedReader();
                messageData.Encoding = TextMessageEncoderFactory.GetEncodingFromContentType(contentType, this.contentEncodingMap);
                messageData.Open(buffer, bufferManager);
                RecycledMessageState recycledMessageState = messageData.TakeMessageState();
                if (recycledMessageState == null)
                {
                    recycledMessageState = new RecycledMessageState();
                }
                Message message = new BufferedMessage(messageData, recycledMessageState) {
                    Properties = { Encoder = this }
                };
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
                }
                return message;
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                if (stream == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
                }
                Message message = Message.CreateMessage(this.TakeStreamedReader(stream, TextMessageEncoderFactory.GetEncodingFromContentType(contentType, this.contentEncodingMap)), maxSizeOfHeaders, this.version);
                message.Properties.Encoder = this;
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
                }
                return message;
            }

            private void ReturnBufferedData(UTF8BufferedMessageData messageData)
            {
                this.bufferedReaderPool.Return(messageData);
            }

            private void ReturnMessageWriter(TextBufferedMessageWriter messageWriter)
            {
                this.bufferedWriterPool.Return(messageWriter);
            }

            private void ReturnStreamedReader(XmlDictionaryReader xmlReader)
            {
                this.streamedReaderPool.Return(xmlReader);
            }

            private void ReturnStreamedWriter(XmlWriter xmlWriter)
            {
                xmlWriter.Close();
                this.streamedWriterPool.Return((XmlDictionaryWriter) xmlWriter);
            }

            private UTF8BufferedMessageData TakeBufferedReader()
            {
                if (this.bufferedReaderPool == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.bufferedReaderPool == null)
                        {
                            this.bufferedReaderPool = new SynchronizedPool<UTF8BufferedMessageData>(this.maxReadPoolSize);
                        }
                    }
                }
                UTF8BufferedMessageData data = this.bufferedReaderPool.Take();
                if (data == null)
                {
                    data = new UTF8BufferedMessageData(this, 2);
                }
                return data;
            }

            private TextBufferedMessageWriter TakeBufferedWriter()
            {
                if (this.bufferedWriterPool == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.bufferedWriterPool == null)
                        {
                            this.bufferedWriterPool = new SynchronizedPool<TextBufferedMessageWriter>(this.maxWritePoolSize);
                        }
                    }
                }
                TextBufferedMessageWriter writer = this.bufferedWriterPool.Take();
                if (writer == null)
                {
                    writer = new TextBufferedMessageWriter(this);
                }
                return writer;
            }

            private XmlReader TakeStreamedReader(Stream stream, Encoding enc)
            {
                if (this.streamedReaderPool == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.streamedReaderPool == null)
                        {
                            this.streamedReaderPool = new SynchronizedPool<XmlDictionaryReader>(this.maxReadPoolSize);
                        }
                    }
                }
                XmlDictionaryReader reader = this.streamedReaderPool.Take();
                if (reader == null)
                {
                    return XmlDictionaryReader.CreateTextReader(stream, enc, this.readerQuotas, null);
                }
                ((IXmlTextReaderInitializer) reader).SetInput(stream, enc, this.readerQuotas, this.onStreamedReaderClose);
                return reader;
            }

            private XmlDictionaryWriter TakeStreamedWriter(Stream stream)
            {
                if (this.streamedWriterPool == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.streamedWriterPool == null)
                        {
                            this.streamedWriterPool = new SynchronizedPool<XmlDictionaryWriter>(this.maxWritePoolSize);
                        }
                    }
                }
                XmlDictionaryWriter writer = this.streamedWriterPool.Take();
                if (writer == null)
                {
                    return XmlDictionaryWriter.CreateTextWriter(stream, this.writeEncoding, false);
                }
                ((IXmlTextWriterInitializer) writer).SetOutput(stream, this.writeEncoding, false);
                return writer;
            }

            public override void WriteMessage(Message message, Stream stream)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (stream == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("stream"), message);
                }
                base.ThrowIfMismatchedMessageVersion(message);
                message.Properties.Encoder = this;
                XmlDictionaryWriter writer = this.TakeStreamedWriter(stream);
                if (this.optimizeWriteForUTF8)
                {
                    message.WriteMessage(writer);
                }
                else
                {
                    writer.WriteStartDocument();
                    message.WriteMessage(writer);
                    writer.WriteEndDocument();
                }
                writer.Flush();
                this.ReturnStreamedWriter(writer);
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
                }
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (bufferManager == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("bufferManager"), message);
                }
                if (maxMessageSize < 0)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxMessageSize", maxMessageSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")), message);
                }
                if ((messageOffset < 0) || (messageOffset > maxMessageSize))
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageOffset", messageOffset, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, maxMessageSize })), message);
                }
                base.ThrowIfMismatchedMessageVersion(message);
                message.Properties.Encoder = this;
                TextBufferedMessageWriter messageWriter = this.TakeBufferedWriter();
                ArraySegment<byte> segment = messageWriter.WriteMessage(message, bufferManager, messageOffset, maxMessageSize);
                this.ReturnMessageWriter(messageWriter);
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(segment.Array, segment.Offset, segment.Count, null, XmlDictionaryReaderQuotas.Max, null);
                    MessageLogger.LogMessage(ref message, reader, MessageLoggingSource.TransportSend);
                }
                return segment;
            }

            public override string ContentType
            {
                get
                {
                    return this.contentType;
                }
            }

            public int MaxReadPoolSize
            {
                get
                {
                    return this.maxReadPoolSize;
                }
            }

            public int MaxWritePoolSize
            {
                get
                {
                    return this.maxWritePoolSize;
                }
            }

            public override string MediaType
            {
                get
                {
                    return this.mediaType;
                }
            }

            public override System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.version;
                }
            }

            public XmlDictionaryReaderQuotas ReaderQuotas
            {
                get
                {
                    return this.readerQuotas;
                }
            }

            private SynchronizedPool<RecycledMessageState> RecycledStatePool
            {
                get
                {
                    if (this.recycledStatePool == null)
                    {
                        lock (this.ThisLock)
                        {
                            if (this.recycledStatePool == null)
                            {
                                this.recycledStatePool = new SynchronizedPool<RecycledMessageState>(this.maxReadPoolSize);
                            }
                        }
                    }
                    return this.recycledStatePool;
                }
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            private class TextBufferedMessageWriter : BufferedMessageWriter
            {
                private TextMessageEncoderFactory.TextMessageEncoder messageEncoder;
                private XmlDictionaryWriter writer;

                public TextBufferedMessageWriter(TextMessageEncoderFactory.TextMessageEncoder messageEncoder)
                {
                    this.messageEncoder = messageEncoder;
                }

                protected override void OnWriteEndMessage(XmlDictionaryWriter writer)
                {
                    if (!this.messageEncoder.optimizeWriteForUTF8)
                    {
                        writer.WriteEndDocument();
                    }
                }

                protected override void OnWriteStartMessage(XmlDictionaryWriter writer)
                {
                    if (!this.messageEncoder.optimizeWriteForUTF8)
                    {
                        writer.WriteStartDocument();
                    }
                }

                protected override void ReturnXmlWriter(XmlDictionaryWriter writer)
                {
                    writer.Close();
                    if (this.messageEncoder.optimizeWriteForUTF8 && (this.writer == null))
                    {
                        this.writer = writer;
                    }
                }

                protected override XmlDictionaryWriter TakeXmlWriter(Stream stream)
                {
                    if (!this.messageEncoder.optimizeWriteForUTF8)
                    {
                        return this.messageEncoder.CreateWriter(stream);
                    }
                    XmlDictionaryWriter writer = this.writer;
                    if (writer == null)
                    {
                        return XmlDictionaryWriter.CreateTextWriter(stream, this.messageEncoder.writeEncoding, false);
                    }
                    this.writer = null;
                    ((IXmlTextWriterInitializer) writer).SetOutput(stream, this.messageEncoder.writeEncoding, false);
                    return writer;
                }
            }

            private class UTF8BufferedMessageData : BufferedMessageData
            {
                private const int additionalNodeSpace = 0x400;
                private System.Text.Encoding encoding;
                private TextMessageEncoderFactory.TextMessageEncoder messageEncoder;
                private OnXmlDictionaryReaderClose onClose;
                private Pool<XmlDictionaryReader> readerPool;

                public UTF8BufferedMessageData(TextMessageEncoderFactory.TextMessageEncoder messageEncoder, int maxReaderPoolSize) : base(messageEncoder.RecycledStatePool)
                {
                    this.messageEncoder = messageEncoder;
                    this.readerPool = new Pool<XmlDictionaryReader>(maxReaderPoolSize);
                    this.onClose = new OnXmlDictionaryReaderClose(this.OnXmlReaderClosed);
                }

                protected override void OnClosed()
                {
                    this.messageEncoder.ReturnBufferedData(this);
                }

                protected override void ReturnXmlReader(XmlDictionaryReader xmlReader)
                {
                    if (xmlReader != null)
                    {
                        this.readerPool.Return(xmlReader);
                    }
                }

                protected override XmlDictionaryReader TakeXmlReader()
                {
                    ArraySegment<byte> buffer = base.Buffer;
                    XmlDictionaryReader reader = this.readerPool.Take();
                    if (reader == null)
                    {
                        return XmlDictionaryReader.CreateTextReader(buffer.Array, buffer.Offset, buffer.Count, this.encoding, this.messageEncoder.readerQuotas, this.onClose);
                    }
                    ((IXmlTextReaderInitializer) reader).SetInput(buffer.Array, buffer.Offset, buffer.Count, this.encoding, this.messageEncoder.readerQuotas, this.onClose);
                    return reader;
                }

                internal System.Text.Encoding Encoding
                {
                    set
                    {
                        this.encoding = value;
                    }
                }

                public override System.ServiceModel.Channels.MessageEncoder MessageEncoder
                {
                    get
                    {
                        return this.messageEncoder;
                    }
                }

                public override XmlDictionaryReaderQuotas Quotas
                {
                    get
                    {
                        return this.messageEncoder.readerQuotas;
                    }
                }
            }
        }
    }
}

