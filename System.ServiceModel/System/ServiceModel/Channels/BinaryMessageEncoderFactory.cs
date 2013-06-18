namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;

    internal class BinaryMessageEncoderFactory : MessageEncoderFactory
    {
        private BinaryVersion binaryVersion;
        private SynchronizedPool<BinaryBufferedMessageData> bufferedDataPool;
        private SynchronizedPool<BinaryBufferedMessageWriter> bufferedWriterPool;
        private const int maxPooledXmlReaderPerMessage = 2;
        private int maxReadPoolSize;
        private int maxSessionSize;
        private int maxWritePoolSize;
        private BinaryMessageEncoder messageEncoder;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private OnXmlDictionaryReaderClose onStreamedReaderClose;
        private XmlDictionaryReaderQuotas readerQuotas;
        private SynchronizedPool<RecycledMessageState> recycledStatePool;
        private SynchronizedPool<XmlDictionaryReader> streamedReaderPool;
        private SynchronizedPool<XmlDictionaryWriter> streamedWriterPool;
        private object thisLock;

        public BinaryMessageEncoderFactory(System.ServiceModel.Channels.MessageVersion messageVersion, int maxReadPoolSize, int maxWritePoolSize, int maxSessionSize, XmlDictionaryReaderQuotas readerQuotas, BinaryVersion version)
        {
            this.messageVersion = messageVersion;
            this.messageEncoder = new BinaryMessageEncoder(this, false, 0);
            this.maxReadPoolSize = maxReadPoolSize;
            this.maxWritePoolSize = maxWritePoolSize;
            this.maxSessionSize = maxSessionSize;
            this.thisLock = new object();
            this.onStreamedReaderClose = new OnXmlDictionaryReaderClose(this.ReturnStreamedReader);
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            if (readerQuotas != null)
            {
                readerQuotas.CopyTo(this.readerQuotas);
            }
            this.binaryVersion = version;
        }

        public override MessageEncoder CreateSessionEncoder()
        {
            return new BinaryMessageEncoder(this, true, this.maxSessionSize);
        }

        private void ReturnBufferedData(BinaryBufferedMessageData messageData)
        {
            messageData.SetMessageEncoder(null);
            this.bufferedDataPool.Return(messageData);
        }

        private void ReturnMessageWriter(BinaryBufferedMessageWriter messageWriter)
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

        private BinaryBufferedMessageData TakeBufferedData(BinaryMessageEncoder messageEncoder)
        {
            if (this.bufferedDataPool == null)
            {
                lock (this.ThisLock)
                {
                    if (this.bufferedDataPool == null)
                    {
                        this.bufferedDataPool = new SynchronizedPool<BinaryBufferedMessageData>(this.maxReadPoolSize);
                    }
                }
            }
            BinaryBufferedMessageData data = this.bufferedDataPool.Take();
            if (data == null)
            {
                data = new BinaryBufferedMessageData(this, 2);
            }
            data.SetMessageEncoder(messageEncoder);
            return data;
        }

        private BinaryBufferedMessageWriter TakeBufferedWriter()
        {
            if (this.bufferedWriterPool == null)
            {
                lock (this.ThisLock)
                {
                    if (this.bufferedWriterPool == null)
                    {
                        this.bufferedWriterPool = new SynchronizedPool<BinaryBufferedMessageWriter>(this.maxWritePoolSize);
                    }
                }
            }
            BinaryBufferedMessageWriter writer = this.bufferedWriterPool.Take();
            if (writer == null)
            {
                writer = new BinaryBufferedMessageWriter(this.binaryVersion.Dictionary);
            }
            return writer;
        }

        private XmlDictionaryReader TakeStreamedReader(Stream stream)
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
                return XmlDictionaryReader.CreateBinaryReader(stream, this.binaryVersion.Dictionary, this.readerQuotas, null, this.onStreamedReaderClose);
            }
            ((IXmlBinaryReaderInitializer) reader).SetInput(stream, this.binaryVersion.Dictionary, this.readerQuotas, null, this.onStreamedReaderClose);
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
                return XmlDictionaryWriter.CreateBinaryWriter(stream, this.binaryVersion.Dictionary, null, false);
            }
            ((IXmlBinaryWriterInitializer) writer).SetOutput(stream, this.binaryVersion.Dictionary, null, false);
            return writer;
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
                return this.maxReadPoolSize;
            }
        }

        public int MaxSessionSize
        {
            get
            {
                return this.maxSessionSize;
            }
        }

        public int MaxWritePoolSize
        {
            get
            {
                return this.maxWritePoolSize;
            }
        }

        public override System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
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

        public static IXmlDictionary XmlDictionary
        {
            get
            {
                return XD.Dictionary;
            }
        }

        private class BinaryBufferedMessageData : BufferedMessageData
        {
            private BinaryMessageEncoderFactory factory;
            private BinaryMessageEncoderFactory.BinaryMessageEncoder messageEncoder;
            private OnXmlDictionaryReaderClose onClose;
            private Pool<XmlDictionaryReader> readerPool;

            public BinaryBufferedMessageData(BinaryMessageEncoderFactory factory, int maxPoolSize) : base(factory.RecycledStatePool)
            {
                this.factory = factory;
                this.readerPool = new Pool<XmlDictionaryReader>(maxPoolSize);
                this.onClose = new OnXmlDictionaryReaderClose(this.OnXmlReaderClosed);
            }

            protected override void OnClosed()
            {
                this.factory.ReturnBufferedData(this);
            }

            protected override void ReturnXmlReader(XmlDictionaryReader reader)
            {
                this.readerPool.Return(reader);
            }

            public void SetMessageEncoder(BinaryMessageEncoderFactory.BinaryMessageEncoder messageEncoder)
            {
                this.messageEncoder = messageEncoder;
            }

            protected override XmlDictionaryReader TakeXmlReader()
            {
                ArraySegment<byte> buffer = base.Buffer;
                XmlDictionaryReader reader = this.readerPool.Take();
                if (reader != null)
                {
                    ((IXmlBinaryReaderInitializer) reader).SetInput(buffer.Array, buffer.Offset, buffer.Count, this.factory.binaryVersion.Dictionary, this.factory.readerQuotas, this.messageEncoder.ReaderSession, this.onClose);
                    return reader;
                }
                return XmlDictionaryReader.CreateBinaryReader(buffer.Array, buffer.Offset, buffer.Count, this.factory.binaryVersion.Dictionary, this.factory.readerQuotas, this.messageEncoder.ReaderSession, this.onClose);
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
                    return this.factory.readerQuotas;
                }
            }
        }

        private class BinaryBufferedMessageWriter : BufferedMessageWriter
        {
            private IXmlDictionary dictionary;
            private XmlBinaryWriterSession session;
            private XmlDictionaryWriter writer;

            public BinaryBufferedMessageWriter(IXmlDictionary dictionary)
            {
                this.dictionary = dictionary;
            }

            public BinaryBufferedMessageWriter(IXmlDictionary dictionary, XmlBinaryWriterSession session)
            {
                this.dictionary = dictionary;
                this.session = session;
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
                    return XmlDictionaryWriter.CreateBinaryWriter(stream, this.dictionary, this.session, false);
                }
                this.writer = null;
                ((IXmlBinaryWriterInitializer) writer).SetOutput(stream, this.dictionary, this.session, false);
                return writer;
            }
        }

        private class BinaryMessageEncoder : MessageEncoder
        {
            private BinaryMessageEncoderFactory factory;
            private int idCounter;
            private bool isReaderSessionInvalid;
            private bool isSession;
            private int maxSessionSize;
            private MessagePatterns messagePatterns;
            private XmlBinaryReaderSession readerSession;
            private XmlBinaryReaderSession readerSessionForLogging;
            private bool readerSessionForLoggingIsInvalid;
            private int remainingReaderSessionSize;
            private BinaryMessageEncoderFactory.BinaryBufferedMessageWriter sessionMessageWriter;
            private int writeIdCounter;
            private BinaryMessageEncoderFactory.XmlBinaryWriterSessionWithQuota writerSession;

            public BinaryMessageEncoder(BinaryMessageEncoderFactory factory, bool isSession, int maxSessionSize)
            {
                this.factory = factory;
                this.isSession = isSession;
                this.maxSessionSize = maxSessionSize;
                this.remainingReaderSessionSize = maxSessionSize;
            }

            private ArraySegment<byte> AddSessionInformationToMessage(ArraySegment<byte> messageData, BufferManager bufferManager, int maxMessageSize)
            {
                int num = 0;
                byte[] array = messageData.Array;
                if (this.writerSession.HasNewStrings)
                {
                    IList<XmlDictionaryString> newStrings = this.writerSession.GetNewStrings();
                    for (int i = 0; i < newStrings.Count; i++)
                    {
                        int byteCount = Encoding.UTF8.GetByteCount(newStrings[i].Value);
                        num += IntEncoder.GetEncodedSize(byteCount) + byteCount;
                    }
                    int num4 = messageData.Offset + messageData.Count;
                    int num5 = maxMessageSize - num4;
                    if ((num5 - num) < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(System.ServiceModel.SR.GetString("MaxSentMessageSizeExceeded", new object[] { maxMessageSize })));
                    }
                    int bufferSize = (messageData.Offset + messageData.Count) + num;
                    if (array.Length < bufferSize)
                    {
                        byte[] dst = bufferManager.TakeBuffer(bufferSize);
                        Buffer.BlockCopy(array, messageData.Offset, dst, messageData.Offset, messageData.Count);
                        bufferManager.ReturnBuffer(array);
                        array = dst;
                    }
                    Buffer.BlockCopy(array, messageData.Offset, array, messageData.Offset + num, messageData.Count);
                    int num7 = messageData.Offset;
                    for (int j = 0; j < newStrings.Count; j++)
                    {
                        string s = newStrings[j].Value;
                        int num9 = Encoding.UTF8.GetByteCount(s);
                        num7 += IntEncoder.Encode(num9, array, num7);
                        num7 += Encoding.UTF8.GetBytes(s, 0, s.Length, array, num7);
                    }
                    this.writerSession.ClearNewStrings();
                }
                int encodedSize = IntEncoder.GetEncodedSize(num);
                int offset = messageData.Offset - encodedSize;
                int count = (encodedSize + messageData.Count) + num;
                IntEncoder.Encode(num, array, offset);
                return new ArraySegment<byte>(array, offset, count);
            }

            private ArraySegment<byte> ExtractSessionInformationFromMessage(ArraySegment<byte> messageData)
            {
                int num3;
                int num4;
                if (this.isReaderSessionInvalid)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("BinaryEncoderSessionInvalid")));
                }
                byte[] array = messageData.Array;
                bool flag = true;
                try
                {
                    IntDecoder decoder = new IntDecoder();
                    int num2 = decoder.Decode(array, messageData.Offset, messageData.Count);
                    int num = decoder.Value;
                    if (num > messageData.Count)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("BinaryEncoderSessionMalformed")));
                    }
                    num3 = (messageData.Offset + num2) + num;
                    num4 = (messageData.Count - num2) - num;
                    if (num4 < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("BinaryEncoderSessionMalformed")));
                    }
                    if (num > 0)
                    {
                        if (num > this.remainingReaderSessionSize)
                        {
                            string message = System.ServiceModel.SR.GetString("BinaryEncoderSessionTooLarge", new object[] { this.maxSessionSize });
                            Exception innerException = new QuotaExceededException(message);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(message, innerException));
                        }
                        this.remainingReaderSessionSize -= num;
                        int size = num;
                        int offset = messageData.Offset + num2;
                        while (size > 0)
                        {
                            decoder.Reset();
                            int num7 = decoder.Decode(array, offset, size);
                            int count = decoder.Value;
                            offset += num7;
                            size -= num7;
                            if (count > size)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("BinaryEncoderSessionMalformed")));
                            }
                            string str2 = Encoding.UTF8.GetString(array, offset, count);
                            offset += count;
                            size -= count;
                            this.readerSession.Add(this.idCounter, str2);
                            this.idCounter++;
                        }
                    }
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        this.isReaderSessionInvalid = true;
                    }
                }
                return new ArraySegment<byte>(array, num3, num4);
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                if (stream == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
                }
                if (((contentType != null) && (contentType != this.ContentType)) && !contentType.StartsWith(this.ContentType, StringComparison.OrdinalIgnoreCase))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("EncoderUnrecognizedContentType", new object[] { contentType, this.ContentType })));
                }
                Message message = Message.CreateMessage(this.factory.TakeStreamedReader(stream), maxSizeOfHeaders, this.factory.messageVersion);
                message.Properties.Encoder = this;
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
                }
                return message;
            }

            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                Message message;
                if (bufferManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bufferManager");
                }
                if (((contentType != null) && (contentType != this.ContentType)) && !contentType.StartsWith(this.ContentType, StringComparison.OrdinalIgnoreCase))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("EncoderUnrecognizedContentType", new object[] { contentType, this.ContentType })));
                }
                if (this.isSession)
                {
                    if (this.readerSession == null)
                    {
                        this.readerSession = new XmlBinaryReaderSession();
                        this.messagePatterns = new MessagePatterns(this.factory.binaryVersion.Dictionary, this.readerSession, this.MessageVersion);
                    }
                    try
                    {
                        buffer = this.ExtractSessionInformationFromMessage(buffer);
                    }
                    catch (InvalidDataException)
                    {
                        MessageLogger.LogMessage(buffer, MessageLoggingSource.Malformed);
                        throw;
                    }
                }
                BinaryMessageEncoderFactory.BinaryBufferedMessageData messageData = this.factory.TakeBufferedData(this);
                if (this.messagePatterns != null)
                {
                    message = this.messagePatterns.TryCreateMessage(buffer.Array, buffer.Offset, buffer.Count, bufferManager, messageData);
                }
                else
                {
                    message = null;
                }
                if (message == null)
                {
                    messageData.Open(buffer, bufferManager);
                    RecycledMessageState recycledMessageState = messageData.TakeMessageState();
                    if (recycledMessageState == null)
                    {
                        recycledMessageState = new RecycledMessageState();
                    }
                    message = new BufferedMessage(messageData, recycledMessageState);
                }
                message.Properties.Encoder = this;
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive);
                }
                return message;
            }

            public override void WriteMessage(Message message, Stream stream)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
                }
                if (stream == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
                }
                base.ThrowIfMismatchedMessageVersion(message);
                message.Properties.Encoder = this;
                XmlDictionaryWriter writer = this.factory.TakeStreamedWriter(stream);
                message.WriteMessage(writer);
                writer.Flush();
                this.factory.ReturnStreamedWriter(writer);
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
                }
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                BinaryMessageEncoderFactory.BinaryBufferedMessageWriter sessionMessageWriter;
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
                message.Properties.Encoder = this;
                if (this.isSession)
                {
                    if (this.writerSession == null)
                    {
                        this.writerSession = new BinaryMessageEncoderFactory.XmlBinaryWriterSessionWithQuota(this.maxSessionSize);
                        this.sessionMessageWriter = new BinaryMessageEncoderFactory.BinaryBufferedMessageWriter(this.factory.binaryVersion.Dictionary, this.writerSession);
                    }
                    messageOffset += 5;
                }
                if (messageOffset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageOffset", messageOffset, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if (messageOffset > maxMessageSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(System.ServiceModel.SR.GetString("MaxSentMessageSizeExceeded", new object[] { maxMessageSize })));
                }
                base.ThrowIfMismatchedMessageVersion(message);
                if (this.isSession)
                {
                    sessionMessageWriter = this.sessionMessageWriter;
                }
                else
                {
                    sessionMessageWriter = this.factory.TakeBufferedWriter();
                }
                ArraySegment<byte> messageData = sessionMessageWriter.WriteMessage(message, bufferManager, messageOffset, maxMessageSize);
                if (MessageLogger.LogMessagesAtTransportLevel && !this.readerSessionForLoggingIsInvalid)
                {
                    if (this.isSession)
                    {
                        if (this.readerSessionForLogging == null)
                        {
                            this.readerSessionForLogging = new XmlBinaryReaderSession();
                        }
                        if (this.writerSession.HasNewStrings)
                        {
                            foreach (XmlDictionaryString str in this.writerSession.GetNewStrings())
                            {
                                this.readerSessionForLogging.Add(this.writeIdCounter++, str.Value);
                            }
                        }
                    }
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(messageData.Array, messageData.Offset, messageData.Count, XD.Dictionary, XmlDictionaryReaderQuotas.Max, this.readerSessionForLogging, null);
                    MessageLogger.LogMessage(ref message, reader, MessageLoggingSource.TransportSend);
                }
                else
                {
                    this.readerSessionForLoggingIsInvalid = true;
                }
                if (this.isSession)
                {
                    return this.AddSessionInformationToMessage(messageData, bufferManager, maxMessageSize);
                }
                this.factory.ReturnMessageWriter(sessionMessageWriter);
                return messageData;
            }

            public override string ContentType
            {
                get
                {
                    if (!this.isSession)
                    {
                        return this.factory.binaryVersion.ContentType;
                    }
                    return this.factory.binaryVersion.SessionContentType;
                }
            }

            public override string MediaType
            {
                get
                {
                    if (!this.isSession)
                    {
                        return this.factory.binaryVersion.ContentType;
                    }
                    return this.factory.binaryVersion.SessionContentType;
                }
            }

            public override System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.factory.messageVersion;
                }
            }

            public XmlBinaryReaderSession ReaderSession
            {
                get
                {
                    return this.readerSession;
                }
            }
        }

        private class XmlBinaryWriterSessionWithQuota : XmlBinaryWriterSession
        {
            private int bytesRemaining;
            private List<XmlDictionaryString> newStrings;

            public XmlBinaryWriterSessionWithQuota(int maxSessionSize)
            {
                this.bytesRemaining = maxSessionSize;
            }

            public void ClearNewStrings()
            {
                this.newStrings = null;
            }

            public IList<XmlDictionaryString> GetNewStrings()
            {
                return this.newStrings;
            }

            public override bool TryAdd(XmlDictionaryString s, out int key)
            {
                if (this.bytesRemaining == 0)
                {
                    key = -1;
                    return false;
                }
                int byteCount = Encoding.UTF8.GetByteCount(s.Value);
                byteCount += IntEncoder.GetEncodedSize(byteCount);
                if (byteCount > this.bytesRemaining)
                {
                    key = -1;
                    this.bytesRemaining = 0;
                    return false;
                }
                if (!base.TryAdd(s, out key))
                {
                    return false;
                }
                if (this.newStrings == null)
                {
                    this.newStrings = new List<XmlDictionaryString>();
                }
                this.newStrings.Add(s);
                this.bytesRemaining -= byteCount;
                return true;
            }

            public bool HasNewStrings
            {
                get
                {
                    return (this.newStrings != null);
                }
            }
        }
    }
}

