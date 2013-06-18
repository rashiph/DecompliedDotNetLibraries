namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;

    internal class MtomMessageEncoder : MessageEncoder
    {
        private SynchronizedPool<MtomBufferedMessageData> bufferedReaderPool;
        private SynchronizedPool<MtomBufferedMessageWriter> bufferedWriterPool;
        internal TextMessageEncoderFactory.ContentEncoding[] contentEncodingMap;
        private int maxBufferSize;
        private const int maxPooledXmlReadersPerMessage = 2;
        private int maxReadPoolSize;
        private int maxWritePoolSize;
        private static UriGenerator mimeBoundaryGenerator;
        private const string mtomContentType = "multipart/related; type=\"application/xop+xml\"";
        private const string mtomMediaType = "multipart/related";
        private const string mtomStartUri = "http://tempuri.org/0";
        private OnXmlDictionaryReaderClose onStreamedReaderClose;
        private XmlDictionaryReaderQuotas readerQuotas;
        private SynchronizedPool<RecycledMessageState> recycledStatePool;
        private SynchronizedPool<XmlDictionaryReader> streamedReaderPool;
        private SynchronizedPool<XmlDictionaryWriter> streamedWriterPool;
        private object thisLock;
        private System.ServiceModel.Channels.MessageVersion version;
        private Encoding writeEncoding;

        public MtomMessageEncoder(System.ServiceModel.Channels.MessageVersion version, Encoding writeEncoding, int maxReadPoolSize, int maxWritePoolSize, int maxBufferSize, XmlDictionaryReaderQuotas quotas)
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
            this.maxReadPoolSize = maxReadPoolSize;
            this.maxWritePoolSize = maxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            quotas.CopyTo(this.readerQuotas);
            this.maxBufferSize = maxBufferSize;
            this.onStreamedReaderClose = new OnXmlDictionaryReaderClose(this.ReturnStreamedReader);
            this.thisLock = new object();
            if (version.Envelope == EnvelopeVersion.Soap12)
            {
                this.contentEncodingMap = TextMessageEncoderFactory.Soap12Content;
            }
            else
            {
                if (version.Envelope != EnvelopeVersion.Soap11)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid MessageVersion", new object[0])));
                }
                this.contentEncodingMap = TextMessageEncoderFactory.Soap11Content;
            }
            this.version = version;
        }

        internal string FormatContentType(string boundary, string startInfo)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};start=\"<{1}>\";boundary=\"{2}\";start-info=\"{3}\"", new object[] { "multipart/related; type=\"application/xop+xml\"", "http://tempuri.org/0", boundary, startInfo });
        }

        private string GenerateStartInfoString()
        {
            if (this.version.Envelope != EnvelopeVersion.Soap12)
            {
                return "text/xml";
            }
            return "application/soap+xml";
        }

        internal string GetContentType(out string boundary)
        {
            string startInfo = this.GenerateStartInfoString();
            boundary = MimeBoundaryGenerator.Next();
            return this.FormatContentType(boundary, startInfo);
        }

        internal override bool IsCharSetSupported(string charSet)
        {
            if ((charSet != null) && (charSet.Length != 0))
            {
                Encoding encoding;
                return TextEncoderDefaults.TryGetEncoding(charSet, out encoding);
            }
            return true;
        }

        public override bool IsContentTypeSupported(string contentType)
        {
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contentType"));
            }
            if (!this.IsMTOMContentType(contentType))
            {
                return this.IsTextContentType(contentType);
            }
            return true;
        }

        internal bool IsMTOMContentType(string contentType)
        {
            return base.IsContentTypeSupported(contentType, this.ContentType, this.MediaType);
        }

        internal bool IsTextContentType(string contentType)
        {
            string mediaType = TextMessageEncoderFactory.GetMediaType(this.version);
            string supportedContentType = TextMessageEncoderFactory.GetContentType(mediaType, this.writeEncoding);
            return base.IsContentTypeSupported(contentType, supportedContentType, mediaType);
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            if (bufferManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bufferManager");
            }
            if (contentType == this.ContentType)
            {
                contentType = null;
            }
            MtomBufferedMessageData messageData = this.TakeBufferedReader();
            messageData.ContentType = contentType;
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
            if (contentType == this.ContentType)
            {
                contentType = null;
            }
            Message message = Message.CreateMessage(this.TakeStreamedReader(stream, contentType), maxSizeOfHeaders, this.version);
            message.Properties.Encoder = this;
            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
            }
            return message;
        }

        private void ReturnBufferedData(MtomBufferedMessageData messageData)
        {
            this.bufferedReaderPool.Return(messageData);
        }

        private void ReturnMessageWriter(MtomBufferedMessageWriter messageWriter)
        {
            this.bufferedWriterPool.Return(messageWriter);
        }

        private void ReturnStreamedReader(XmlDictionaryReader xmlReader)
        {
            this.streamedReaderPool.Return(xmlReader);
        }

        private void ReturnStreamedWriter(XmlDictionaryWriter xmlWriter)
        {
            xmlWriter.Close();
            this.streamedWriterPool.Return(xmlWriter);
        }

        private MtomBufferedMessageData TakeBufferedReader()
        {
            if (this.bufferedReaderPool == null)
            {
                lock (this.thisLock)
                {
                    if (this.bufferedReaderPool == null)
                    {
                        this.bufferedReaderPool = new SynchronizedPool<MtomBufferedMessageData>(this.maxReadPoolSize);
                    }
                }
            }
            MtomBufferedMessageData data = this.bufferedReaderPool.Take();
            if (data == null)
            {
                data = new MtomBufferedMessageData(this, 2);
            }
            return data;
        }

        private MtomBufferedMessageWriter TakeBufferedWriter()
        {
            if (this.bufferedWriterPool == null)
            {
                lock (this.thisLock)
                {
                    if (this.bufferedWriterPool == null)
                    {
                        this.bufferedWriterPool = new SynchronizedPool<MtomBufferedMessageWriter>(this.maxWritePoolSize);
                    }
                }
            }
            MtomBufferedMessageWriter writer = this.bufferedWriterPool.Take();
            if (writer == null)
            {
                writer = new MtomBufferedMessageWriter(this);
            }
            return writer;
        }

        private XmlReader TakeStreamedReader(Stream stream, string contentType)
        {
            if (this.streamedReaderPool == null)
            {
                lock (this.thisLock)
                {
                    if (this.streamedReaderPool == null)
                    {
                        this.streamedReaderPool = new SynchronizedPool<XmlDictionaryReader>(this.maxReadPoolSize);
                    }
                }
            }
            XmlDictionaryReader reader = this.streamedReaderPool.Take();
            try
            {
                if ((contentType == null) || this.IsMTOMContentType(contentType))
                {
                    if ((reader != null) && (reader is IXmlMtomReaderInitializer))
                    {
                        ((IXmlMtomReaderInitializer) reader).SetInput(stream, MtomMessageEncoderFactory.GetSupportedEncodings(), contentType, this.readerQuotas, this.maxBufferSize, this.onStreamedReaderClose);
                        return reader;
                    }
                    return XmlDictionaryReader.CreateMtomReader(stream, MtomMessageEncoderFactory.GetSupportedEncodings(), contentType, this.readerQuotas, this.maxBufferSize, this.onStreamedReaderClose);
                }
                if ((reader != null) && (reader is IXmlTextReaderInitializer))
                {
                    ((IXmlTextReaderInitializer) reader).SetInput(stream, TextMessageEncoderFactory.GetEncodingFromContentType(contentType, this.contentEncodingMap), this.readerQuotas, this.onStreamedReaderClose);
                    return reader;
                }
                reader = XmlDictionaryReader.CreateTextReader(stream, TextMessageEncoderFactory.GetEncodingFromContentType(contentType, this.contentEncodingMap), this.readerQuotas, this.onStreamedReaderClose);
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorCreatingMtomReader"), exception));
            }
            catch (XmlException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorCreatingMtomReader"), exception2));
            }
            return reader;
        }

        private XmlDictionaryWriter TakeStreamedWriter(Stream stream, string startInfo, string boundary, string startUri, bool writeMessageHeaders)
        {
            if (this.streamedWriterPool == null)
            {
                lock (this.thisLock)
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
                return XmlDictionaryWriter.CreateMtomWriter(stream, this.writeEncoding, 0x7fffffff, startInfo, boundary, startUri, writeMessageHeaders, false);
            }
            ((IXmlMtomWriterInitializer) writer).SetOutput(stream, this.writeEncoding, 0x7fffffff, startInfo, boundary, startUri, writeMessageHeaders, false);
            return writer;
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            this.WriteMessage(message, stream, this.GenerateStartInfoString(), null, null, true);
        }

        internal void WriteMessage(Message message, Stream stream, string boundary)
        {
            this.WriteMessage(message, stream, this.GenerateStartInfoString(), boundary, "http://tempuri.org/0", false);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            return this.WriteMessage(message, maxMessageSize, bufferManager, messageOffset, this.GenerateStartInfoString(), null, null, true);
        }

        internal ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset, string boundary)
        {
            return this.WriteMessage(message, maxMessageSize, bufferManager, messageOffset, this.GenerateStartInfoString(), boundary, "http://tempuri.org/0", false);
        }

        private void WriteMessage(Message message, Stream stream, string startInfo, string boundary, string startUri, bool writeMessageHeaders)
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
            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
            }
            XmlDictionaryWriter writer = this.TakeStreamedWriter(stream, startInfo, boundary, startUri, writeMessageHeaders);
            if (this.writeEncoding.WebName == "utf-8")
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
        }

        private ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset, string startInfo, string boundary, string startUri, bool writeMessageHeaders)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (bufferManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bufferManager");
            }
            if (maxMessageSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxMessageSize", maxMessageSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if ((messageOffset < 0) || (messageOffset > maxMessageSize))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageOffset", messageOffset, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, maxMessageSize })));
            }
            base.ThrowIfMismatchedMessageVersion(message);
            message.Properties.Encoder = this;
            MtomBufferedMessageWriter messageWriter = this.TakeBufferedWriter();
            messageWriter.StartInfo = startInfo;
            messageWriter.Boundary = boundary;
            messageWriter.StartUri = startUri;
            messageWriter.WriteMessageHeaders = writeMessageHeaders;
            messageWriter.MaxSizeInBytes = maxMessageSize;
            ArraySegment<byte> segment = messageWriter.WriteMessage(message, bufferManager, messageOffset, maxMessageSize);
            this.ReturnMessageWriter(messageWriter);
            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                string contentType = null;
                if (boundary != null)
                {
                    contentType = this.FormatContentType(boundary, startInfo ?? this.GenerateStartInfoString());
                }
                XmlDictionaryReader reader = XmlDictionaryReader.CreateMtomReader(segment.Array, segment.Offset, segment.Count, MtomMessageEncoderFactory.GetSupportedEncodings(), contentType, XmlDictionaryReaderQuotas.Max, 0x7fffffff, null);
                MessageLogger.LogMessage(ref message, reader, MessageLoggingSource.TransportSend);
            }
            return segment;
        }

        public override string ContentType
        {
            get
            {
                return "multipart/related; type=\"application/xop+xml\"";
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
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
                return "multipart/related";
            }
        }

        public override System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.version;
            }
        }

        private static UriGenerator MimeBoundaryGenerator
        {
            get
            {
                if (mimeBoundaryGenerator == null)
                {
                    mimeBoundaryGenerator = new UriGenerator("uuid", "+");
                }
                return mimeBoundaryGenerator;
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
                    lock (this.thisLock)
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

        private class MtomBufferedMessageData : BufferedMessageData
        {
            internal string ContentType;
            private MtomMessageEncoder messageEncoder;
            private OnXmlDictionaryReaderClose onClose;
            private Pool<XmlDictionaryReader> readerPool;

            public MtomBufferedMessageData(MtomMessageEncoder messageEncoder, int maxReaderPoolSize) : base(messageEncoder.RecycledStatePool)
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
                XmlDictionaryReader reader2;
                try
                {
                    ArraySegment<byte> buffer = base.Buffer;
                    XmlDictionaryReader reader = this.readerPool.Take();
                    if ((this.ContentType == null) || this.messageEncoder.IsMTOMContentType(this.ContentType))
                    {
                        if ((reader != null) && (reader is IXmlMtomReaderInitializer))
                        {
                            ((IXmlMtomReaderInitializer) reader).SetInput(buffer.Array, buffer.Offset, buffer.Count, MtomMessageEncoderFactory.GetSupportedEncodings(), this.ContentType, this.messageEncoder.ReaderQuotas, this.messageEncoder.MaxBufferSize, this.onClose);
                        }
                        else
                        {
                            reader = XmlDictionaryReader.CreateMtomReader(buffer.Array, buffer.Offset, buffer.Count, MtomMessageEncoderFactory.GetSupportedEncodings(), this.ContentType, this.messageEncoder.ReaderQuotas, this.messageEncoder.MaxBufferSize, this.onClose);
                        }
                    }
                    else if ((reader != null) && (reader is IXmlTextReaderInitializer))
                    {
                        ((IXmlTextReaderInitializer) reader).SetInput(buffer.Array, buffer.Offset, buffer.Count, TextMessageEncoderFactory.GetEncodingFromContentType(this.ContentType, this.messageEncoder.contentEncodingMap), this.messageEncoder.ReaderQuotas, this.onClose);
                    }
                    else
                    {
                        reader = XmlDictionaryReader.CreateTextReader(buffer.Array, buffer.Offset, buffer.Count, TextMessageEncoderFactory.GetEncodingFromContentType(this.ContentType, this.messageEncoder.contentEncodingMap), this.messageEncoder.ReaderQuotas, this.onClose);
                    }
                    reader2 = reader;
                }
                catch (FormatException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorCreatingMtomReader"), exception));
                }
                catch (XmlException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorCreatingMtomReader"), exception2));
                }
                return reader2;
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
                    return this.messageEncoder.ReaderQuotas;
                }
            }
        }

        private class MtomBufferedMessageWriter : BufferedMessageWriter
        {
            internal string Boundary;
            internal int MaxSizeInBytes = 0x7fffffff;
            private MtomMessageEncoder messageEncoder;
            internal string StartInfo;
            internal string StartUri;
            internal bool WriteMessageHeaders;
            private XmlDictionaryWriter writer;

            public MtomBufferedMessageWriter(MtomMessageEncoder messageEncoder)
            {
                this.messageEncoder = messageEncoder;
            }

            protected override void ReturnXmlWriter(XmlDictionaryWriter writer)
            {
                writer.Close();
                if (this.writer == null)
                {
                    this.writer = writer;
                }
            }

            protected override XmlDictionaryWriter TakeXmlWriter(Stream stream)
            {
                XmlDictionaryWriter writer = this.writer;
                if (writer == null)
                {
                    writer = XmlDictionaryWriter.CreateMtomWriter(stream, this.messageEncoder.writeEncoding, this.MaxSizeInBytes, this.StartInfo, this.Boundary, this.StartUri, this.WriteMessageHeaders, false);
                }
                else
                {
                    this.writer = null;
                    ((IXmlMtomWriterInitializer) writer).SetOutput(stream, this.messageEncoder.writeEncoding, this.MaxSizeInBytes, this.StartInfo, this.Boundary, this.StartUri, this.WriteMessageHeaders, false);
                }
                if (this.messageEncoder.writeEncoding.WebName != "utf-8")
                {
                    writer.WriteStartDocument();
                }
                return writer;
            }
        }
    }
}

