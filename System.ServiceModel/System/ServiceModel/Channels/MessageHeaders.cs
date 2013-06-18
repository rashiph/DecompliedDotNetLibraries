namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Xml;

    public sealed class MessageHeaders : IEnumerable<MessageHeaderInfo>, IEnumerable
    {
        private int attrCount;
        private IBufferedMessageData bufferedMessageData;
        private int collectionVersion;
        private int headerCount;
        private Header[] headers;
        private const int InitialHeaderCount = 4;
        private static XmlDictionaryString[] localNames;
        private const int MaxBufferedHeaderAttributes = 0x800;
        private const int MaxBufferedHeaderNodes = 0x1000;
        private const int MaxRecycledArrayLength = 8;
        private int nodeCount;
        private System.ServiceModel.Channels.UnderstoodHeaders understoodHeaders;
        private bool understoodHeadersModified;
        private System.ServiceModel.Channels.MessageVersion version;
        internal const string WildcardAction = "*";

        public MessageHeaders(MessageHeaders collection)
        {
            if (collection == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("collection");
            }
            this.Init(collection.version, collection.headers.Length);
            this.CopyHeadersFrom(collection);
            this.collectionVersion = 0;
        }

        public MessageHeaders(System.ServiceModel.Channels.MessageVersion version) : this(version, 4)
        {
        }

        public MessageHeaders(System.ServiceModel.Channels.MessageVersion version, int initialSize)
        {
            this.Init(version, initialSize);
        }

        internal MessageHeaders(System.ServiceModel.Channels.MessageVersion version, MessageHeaders headers, IBufferedMessageData bufferedMessageData)
        {
            this.version = version;
            this.bufferedMessageData = bufferedMessageData;
            this.headerCount = headers.headerCount;
            this.headers = new Header[this.headerCount];
            Array.Copy(headers.headers, this.headers, this.headerCount);
            this.collectionVersion = 0;
        }

        internal MessageHeaders(System.ServiceModel.Channels.MessageVersion version, XmlDictionaryReader reader, XmlAttributeHolder[] envelopeAttributes, XmlAttributeHolder[] headerAttributes, ref int maxSizeOfHeaders) : this(version)
        {
            if (maxSizeOfHeaders < 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxSizeOfHeaders", (int) maxSizeOfHeaders, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("version"));
            }
            if (reader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                XmlBuffer buffer = null;
                EnvelopeVersion envelope = version.Envelope;
                reader.ReadStartElement(XD.MessageDictionary.Header, envelope.DictionaryNamespace);
                while (reader.IsStartElement())
                {
                    if (buffer == null)
                    {
                        buffer = new XmlBuffer(maxSizeOfHeaders);
                    }
                    BufferedHeader headerInfo = new BufferedHeader(version, buffer, reader, envelopeAttributes, headerAttributes);
                    HeaderProcessing processing = headerInfo.MustUnderstand ? HeaderProcessing.MustUnderstand : ((HeaderProcessing) 0);
                    HeaderKind headerKind = this.GetHeaderKind(headerInfo);
                    if (headerKind != HeaderKind.Unknown)
                    {
                        processing = (HeaderProcessing) ((byte) (processing | HeaderProcessing.Understood));
                        TraceUnderstood(headerInfo);
                    }
                    Header header = new Header(headerKind, (ReadableMessageHeader) headerInfo, processing);
                    this.AddHeader(header);
                }
                if (buffer != null)
                {
                    buffer.Close();
                    maxSizeOfHeaders -= buffer.BufferSize;
                }
                reader.ReadEndElement();
                this.collectionVersion = 0;
            }
        }

        internal MessageHeaders(System.ServiceModel.Channels.MessageVersion version, XmlDictionaryReader reader, IBufferedMessageData bufferedMessageData, RecycledMessageState recycledMessageState, bool[] understoodHeaders, bool understoodHeadersModified)
        {
            this.headers = new Header[4];
            this.Init(version, reader, bufferedMessageData, recycledMessageState, understoodHeaders, understoodHeadersModified);
        }

        public void Add(MessageHeader header)
        {
            this.Insert(this.headerCount, header);
        }

        private void Add(MessageHeader header, HeaderKind kind)
        {
            this.Insert(this.headerCount, header, kind);
        }

        internal void AddActionHeader(ActionHeader actionHeader)
        {
            this.Insert(this.headerCount, actionHeader, HeaderKind.Action);
        }

        private void AddHeader(Header header)
        {
            this.InsertHeader(this.headerCount, header);
        }

        internal void AddMessageIDHeader(MessageIDHeader messageIDHeader)
        {
            this.Insert(this.headerCount, messageIDHeader, HeaderKind.MessageId);
        }

        internal void AddRelatesToHeader(RelatesToHeader relatesToHeader)
        {
            this.Insert(this.headerCount, relatesToHeader, HeaderKind.RelatesTo);
        }

        internal void AddReplyToHeader(ReplyToHeader replyToHeader)
        {
            this.Insert(this.headerCount, replyToHeader, HeaderKind.ReplyTo);
        }

        internal void AddToHeader(ToHeader toHeader)
        {
            this.Insert(this.headerCount, toHeader, HeaderKind.To);
        }

        internal void AddUnderstood(int i)
        {
            this.headers[i].HeaderProcessing = (HeaderProcessing) ((byte) (this.headers[i].HeaderProcessing | HeaderProcessing.Understood));
            TraceUnderstood(this.headers[i].HeaderInfo);
        }

        internal void AddUnderstood(MessageHeaderInfo headerInfo)
        {
            if (headerInfo == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("headerInfo"));
            }
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderInfo == headerInfo)
                {
                    if (((byte) (this.headers[i].HeaderProcessing & HeaderProcessing.Understood)) != 0)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("HeaderAlreadyUnderstood", new object[] { headerInfo.Name, headerInfo.Namespace }), "headerInfo"));
                    }
                    this.AddUnderstood(i);
                }
            }
        }

        private BufferedHeader CaptureBufferedHeader(XmlDictionaryReader reader, MessageHeaderInfo headerInfo)
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            buffer.OpenSection(this.bufferedMessageData.Quotas).WriteNode(reader, false);
            buffer.CloseSection();
            buffer.Close();
            return new BufferedHeader(this.version, buffer, 0, headerInfo);
        }

        private BufferedHeader CaptureBufferedHeader(IBufferedMessageData bufferedMessageData, MessageHeaderInfo headerInfo, int bufferedMessageHeaderIndex)
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(bufferedMessageData.Quotas);
            this.WriteBufferedMessageHeader(bufferedMessageData, bufferedMessageHeaderIndex, writer);
            buffer.CloseSection();
            buffer.Close();
            return new BufferedHeader(this.version, buffer, 0, headerInfo);
        }

        private void CaptureBufferedHeaders()
        {
            this.CaptureBufferedHeaders(-1);
        }

        private void CaptureBufferedHeaders(int exceptIndex)
        {
            using (XmlDictionaryReader reader = GetBufferedMessageHeaderReaderAtHeaderContents(this.bufferedMessageData))
            {
                for (int i = 0; i < this.headerCount; i++)
                {
                    if ((reader.NodeType != XmlNodeType.Element) && (reader.MoveToContent() != XmlNodeType.Element))
                    {
                        goto Label_0095;
                    }
                    Header header = this.headers[i];
                    if ((i == exceptIndex) || (header.HeaderType != HeaderType.BufferedMessageHeader))
                    {
                        reader.Skip();
                    }
                    else
                    {
                        BufferedHeader readableHeader = this.CaptureBufferedHeader(reader, header.HeaderInfo);
                        this.headers[i] = new Header(header.HeaderKind, readableHeader, header.HeaderProcessing);
                    }
                }
            }
        Label_0095:
            this.bufferedMessageData = null;
        }

        private BufferedHeader CaptureWriteableHeader(MessageHeader writeableHeader)
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            writeableHeader.WriteHeader(writer, this.version);
            buffer.CloseSection();
            buffer.Close();
            return new BufferedHeader(this.version, buffer, 0, writeableHeader);
        }

        public void Clear()
        {
            for (int i = 0; i < this.headerCount; i++)
            {
                this.headers[i] = new Header();
            }
            this.headerCount = 0;
            this.collectionVersion++;
            this.bufferedMessageData = null;
        }

        public void CopyHeaderFrom(Message message, int headerIndex)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            }
            this.CopyHeaderFrom(message.Headers, headerIndex);
        }

        public void CopyHeaderFrom(MessageHeaders collection, int headerIndex)
        {
            if (collection == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("collection");
            }
            if (collection.version != this.version)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessageHeaderVersionMismatch", new object[] { collection.version.ToString(), this.version.ToString() }), "collection"));
            }
            if ((headerIndex < 0) || (headerIndex >= collection.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", headerIndex, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, collection.headerCount })));
            }
            Header header = collection.headers[headerIndex];
            HeaderProcessing processing = header.HeaderInfo.MustUnderstand ? HeaderProcessing.MustUnderstand : ((HeaderProcessing) 0);
            if ((((byte) (header.HeaderProcessing & HeaderProcessing.Understood)) != 0) || (header.HeaderKind != HeaderKind.Unknown))
            {
                processing = (HeaderProcessing) ((byte) (processing | HeaderProcessing.Understood));
            }
            switch (header.HeaderType)
            {
                case HeaderType.ReadableHeader:
                    this.AddHeader(new Header(header.HeaderKind, header.ReadableHeader, processing));
                    return;

                case HeaderType.BufferedMessageHeader:
                    this.AddHeader(new Header(header.HeaderKind, this.CaptureBufferedHeader(collection.bufferedMessageData, header.HeaderInfo, headerIndex), processing));
                    return;

                case HeaderType.WriteableHeader:
                    this.AddHeader(new Header(header.HeaderKind, header.MessageHeader, processing));
                    return;
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidEnumValue", new object[] { header.HeaderType })));
        }

        public void CopyHeadersFrom(Message message)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            }
            this.CopyHeadersFrom(message.Headers);
        }

        public void CopyHeadersFrom(MessageHeaders collection)
        {
            if (collection == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("collection"));
            }
            for (int i = 0; i < collection.headerCount; i++)
            {
                this.CopyHeaderFrom(collection, i);
            }
        }

        public void CopyTo(MessageHeaderInfo[] array, int index)
        {
            if (array == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("array");
            }
            if ((index < 0) || ((index + this.headerCount) > array.Length))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, array.Length - this.headerCount })));
            }
            for (int i = 0; i < this.headerCount; i++)
            {
                array[i + index] = this.headers[i].HeaderInfo;
            }
        }

        private Exception CreateDuplicateHeaderException(HeaderKind kind)
        {
            string str;
            switch (kind)
            {
                case HeaderKind.Action:
                    str = "Action";
                    break;

                case HeaderKind.FaultTo:
                    str = "FaultTo";
                    break;

                case HeaderKind.From:
                    str = "From";
                    break;

                case HeaderKind.MessageId:
                    str = "MessageID";
                    break;

                case HeaderKind.ReplyTo:
                    str = "ReplyTo";
                    break;

                case HeaderKind.To:
                    str = "To";
                    break;

                default:
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidEnumValue", new object[] { kind })));
            }
            return new MessageHeaderException(System.ServiceModel.SR.GetString("MultipleMessageHeaders", new object[] { str, this.version.Addressing.Namespace }), str, this.version.Addressing.Namespace, true);
        }

        private int FindAddressingHeader(string name, string ns)
        {
            int num = -1;
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderKind != HeaderKind.Unknown)
                {
                    MessageHeaderInfo headerInfo = this.headers[i].HeaderInfo;
                    if ((headerInfo.Name == name) && (headerInfo.Namespace == ns))
                    {
                        if (num >= 0)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MultipleMessageHeaders", new object[] { name, ns }), name, ns, true));
                        }
                        num = i;
                    }
                }
            }
            return num;
        }

        public int FindHeader(string name, string ns)
        {
            if (name == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (ns == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            }
            if (ns == this.version.Addressing.Namespace)
            {
                return this.FindAddressingHeader(name, ns);
            }
            return this.FindNonAddressingHeader(name, ns, this.version.Envelope.UltimateDestinationActorValues);
        }

        public int FindHeader(string name, string ns, params string[] actors)
        {
            if (name == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (ns == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            }
            if (actors == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("actors"));
            }
            int num = -1;
            for (int i = 0; i < this.headerCount; i++)
            {
                MessageHeaderInfo headerInfo = this.headers[i].HeaderInfo;
                if ((headerInfo.Name == name) && (headerInfo.Namespace == ns))
                {
                    for (int j = 0; j < actors.Length; j++)
                    {
                        if (actors[j] == headerInfo.Actor)
                        {
                            if (num >= 0)
                            {
                                if (actors.Length == 1)
                                {
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MultipleMessageHeadersWithActor", new object[] { name, ns, actors[0] }), name, ns, true));
                                }
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MultipleMessageHeaders", new object[] { name, ns }), name, ns, true));
                            }
                            num = i;
                        }
                    }
                }
            }
            return num;
        }

        private int FindHeaderProperty(HeaderKind kind)
        {
            int num = -1;
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderKind == kind)
                {
                    if (num >= 0)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateDuplicateHeaderException(kind));
                    }
                    num = i;
                }
            }
            return num;
        }

        private int FindNonAddressingHeader(string name, string ns, string[] actors)
        {
            int num = -1;
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderKind == HeaderKind.Unknown)
                {
                    MessageHeaderInfo headerInfo = this.headers[i].HeaderInfo;
                    if ((headerInfo.Name == name) && (headerInfo.Namespace == ns))
                    {
                        for (int j = 0; j < actors.Length; j++)
                        {
                            if (actors[j] == headerInfo.Actor)
                            {
                                if (num >= 0)
                                {
                                    if (actors.Length == 1)
                                    {
                                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MultipleMessageHeadersWithActor", new object[] { name, ns, actors[0] }), name, ns, true));
                                    }
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MultipleMessageHeaders", new object[] { name, ns }), name, ns, true));
                                }
                                num = i;
                            }
                        }
                    }
                }
            }
            return num;
        }

        private int FindRelatesTo(Uri relationshipType, out UniqueId messageId)
        {
            UniqueId id = null;
            int num = -1;
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderKind == HeaderKind.RelatesTo)
                {
                    Uri uri;
                    UniqueId id2;
                    this.GetRelatesToValues(i, out uri, out id2);
                    if (relationshipType == uri)
                    {
                        if (id != null)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MultipleRelatesToHeaders", new object[] { relationshipType.AbsoluteUri }), "RelatesTo", this.version.Addressing.Namespace, true));
                        }
                        id = id2;
                        num = i;
                    }
                }
            }
            messageId = id;
            return num;
        }

        private XmlDictionaryReader GetBufferedMessageHeaderReader(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex)
        {
            if ((this.nodeCount > 0x1000) || (this.attrCount > 0x800))
            {
                this.CaptureBufferedHeaders();
                return this.headers[bufferedMessageHeaderIndex].ReadableHeader.GetHeaderReader();
            }
            XmlDictionaryReader bufferedMessageHeaderReaderAtHeaderContents = GetBufferedMessageHeaderReaderAtHeaderContents(bufferedMessageData);
        Label_003E:
            if (bufferedMessageHeaderReaderAtHeaderContents.NodeType != XmlNodeType.Element)
            {
                bufferedMessageHeaderReaderAtHeaderContents.MoveToContent();
            }
            if (bufferedMessageHeaderIndex != 0)
            {
                this.Skip(bufferedMessageHeaderReaderAtHeaderContents);
                bufferedMessageHeaderIndex--;
                goto Label_003E;
            }
            return bufferedMessageHeaderReaderAtHeaderContents;
        }

        private static XmlDictionaryReader GetBufferedMessageHeaderReaderAtHeaderContents(IBufferedMessageData bufferedMessageData)
        {
            XmlDictionaryReader messageReader = bufferedMessageData.GetMessageReader();
            if (messageReader.NodeType == XmlNodeType.Element)
            {
                messageReader.Read();
            }
            else
            {
                messageReader.ReadStartElement();
            }
            if (messageReader.NodeType == XmlNodeType.Element)
            {
                messageReader.Read();
                return messageReader;
            }
            messageReader.ReadStartElement();
            return messageReader;
        }

        public IEnumerator<MessageHeaderInfo> GetEnumerator()
        {
            MessageHeaderInfo[] array = new MessageHeaderInfo[this.headerCount];
            this.CopyTo(array, 0);
            return this.GetEnumerator(array);
        }

        private IEnumerator<MessageHeaderInfo> GetEnumerator(MessageHeaderInfo[] headers)
        {
            return Array.AsReadOnly<MessageHeaderInfo>(headers).GetEnumerator();
        }

        public T GetHeader<T>(int index)
        {
            if ((index < 0) || (index >= this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            MessageHeaderInfo headerInfo = this.headers[index].HeaderInfo;
            return this.GetHeader<T>(index, DataContractSerializerDefaults.CreateSerializer(typeof(T), headerInfo.Name, headerInfo.Namespace, 0x7fffffff));
        }

        public T GetHeader<T>(int index, XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
            {
                return (T) serializer.ReadObject(reader);
            }
        }

        public T GetHeader<T>(string name, string ns)
        {
            return this.GetHeader<T>(name, ns, DataContractSerializerDefaults.CreateSerializer(typeof(T), name, ns, 0x7fffffff));
        }

        public T GetHeader<T>(string name, string ns, params string[] actors)
        {
            int index = this.FindHeader(name, ns, actors);
            if (index < 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("HeaderNotFound", new object[] { name, ns }), name, ns));
            }
            return this.GetHeader<T>(index);
        }

        public T GetHeader<T>(string name, string ns, XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            int index = this.FindHeader(name, ns);
            if (index < 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("HeaderNotFound", new object[] { name, ns }), name, ns));
            }
            return this.GetHeader<T>(index, serializer);
        }

        internal string[] GetHeaderAttributes(string localName, string ns)
        {
            string[] strArray = null;
            if (!this.ContainsOnlyBufferedMessageHeaders)
            {
                for (int j = 0; j < this.headerCount; j++)
                {
                    if (this.headers[j].HeaderType != HeaderType.WriteableHeader)
                    {
                        using (XmlDictionaryReader reader2 = this.GetReaderAtHeader(j))
                        {
                            string attribute = reader2.GetAttribute(localName, ns);
                            if (attribute != null)
                            {
                                if (strArray == null)
                                {
                                    strArray = new string[this.headerCount];
                                }
                                strArray[j] = attribute;
                            }
                        }
                    }
                }
                return strArray;
            }
            XmlDictionaryReader messageReader = this.bufferedMessageData.GetMessageReader();
            messageReader.ReadStartElement();
            messageReader.ReadStartElement();
            for (int i = 0; messageReader.IsStartElement(); i++)
            {
                string str = messageReader.GetAttribute(localName, ns);
                if (str != null)
                {
                    if (strArray == null)
                    {
                        strArray = new string[this.headerCount];
                    }
                    strArray[i] = str;
                }
                if (i == (this.headerCount - 1))
                {
                    break;
                }
                messageReader.Skip();
            }
            messageReader.Close();
            return strArray;
        }

        private HeaderKind GetHeaderKind(MessageHeaderInfo headerInfo)
        {
            HeaderKind unknown = HeaderKind.Unknown;
            if ((headerInfo.Namespace == this.version.Addressing.Namespace) && this.version.Envelope.IsUltimateDestinationActor(headerInfo.Actor))
            {
                string name = headerInfo.Name;
                if (name.Length > 0)
                {
                    switch (name[0])
                    {
                        case 'R':
                            if (!(name == "ReplyTo"))
                            {
                                if (name == "RelatesTo")
                                {
                                    unknown = HeaderKind.RelatesTo;
                                }
                                break;
                            }
                            unknown = HeaderKind.ReplyTo;
                            break;

                        case 'T':
                            if (name == "To")
                            {
                                unknown = HeaderKind.To;
                            }
                            break;

                        case 'M':
                            if (name == "MessageID")
                            {
                                unknown = HeaderKind.MessageId;
                            }
                            break;

                        case 'A':
                            if (name == "Action")
                            {
                                unknown = HeaderKind.Action;
                            }
                            break;

                        case 'F':
                            if (name == "From")
                            {
                                unknown = HeaderKind.From;
                            }
                            else if (name == "FaultTo")
                            {
                                unknown = HeaderKind.FaultTo;
                            }
                            break;
                    }
                }
            }
            this.ValidateHeaderKind(unknown);
            return unknown;
        }

        internal Collection<MessageHeaderInfo> GetHeadersNotUnderstood()
        {
            Collection<MessageHeaderInfo> collection = null;
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderProcessing == HeaderProcessing.MustUnderstand)
                {
                    if (collection == null)
                    {
                        collection = new Collection<MessageHeaderInfo>();
                    }
                    MessageHeaderInfo headerInfo = this.headers[i].HeaderInfo;
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x8000e, System.ServiceModel.SR.GetString("TraceCodeDidNotUnderstandMessageHeader"), new MessageHeaderInfoTraceRecord(headerInfo), null, null);
                    }
                    collection.Add(headerInfo);
                }
            }
            return collection;
        }

        internal MessageHeader GetMessageHeader(int index)
        {
            if ((index < 0) || (index >= this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            switch (this.headers[index].HeaderType)
            {
                case HeaderType.ReadableHeader:
                case HeaderType.WriteableHeader:
                    return this.headers[index].MessageHeader;

                case HeaderType.BufferedMessageHeader:
                {
                    MessageHeader header = this.CaptureBufferedHeader(this.bufferedMessageData, this.headers[index].HeaderInfo, index);
                    this.headers[index] = new Header(this.headers[index].HeaderKind, header, this.headers[index].HeaderProcessing);
                    this.collectionVersion++;
                    return header;
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidEnumValue", new object[] { this.headers[index].HeaderType })));
        }

        public XmlDictionaryReader GetReaderAtHeader(int headerIndex)
        {
            if ((headerIndex < 0) || (headerIndex >= this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", headerIndex, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            switch (this.headers[headerIndex].HeaderType)
            {
                case HeaderType.ReadableHeader:
                    return this.headers[headerIndex].ReadableHeader.GetHeaderReader();

                case HeaderType.BufferedMessageHeader:
                    return this.GetBufferedMessageHeaderReader(this.bufferedMessageData, headerIndex);

                case HeaderType.WriteableHeader:
                {
                    MessageHeader messageHeader = this.headers[headerIndex].MessageHeader;
                    BufferedHeader readableHeader = this.CaptureWriteableHeader(messageHeader);
                    this.headers[headerIndex] = new Header(this.headers[headerIndex].HeaderKind, readableHeader, this.headers[headerIndex].HeaderProcessing);
                    this.collectionVersion++;
                    return readableHeader.GetHeaderReader();
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidEnumValue", new object[] { this.headers[headerIndex].HeaderType })));
        }

        internal UniqueId GetRelatesTo(Uri relationshipType)
        {
            UniqueId id;
            if (relationshipType == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("relationshipType"));
            }
            this.FindRelatesTo(relationshipType, out id);
            return id;
        }

        private void GetRelatesToValues(int index, out Uri relationshipType, out UniqueId messageId)
        {
            RelatesToHeader headerInfo = this.headers[index].HeaderInfo as RelatesToHeader;
            if (headerInfo != null)
            {
                relationshipType = headerInfo.RelationshipType;
                messageId = headerInfo.UniqueId;
            }
            else
            {
                using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
                {
                    RelatesToHeader.ReadHeaderValue(reader, this.version.Addressing, out relationshipType, out messageId);
                }
            }
        }

        internal IEnumerator<MessageHeaderInfo> GetUnderstoodEnumerator()
        {
            List<MessageHeaderInfo> list = new List<MessageHeaderInfo>();
            for (int i = 0; i < this.headerCount; i++)
            {
                if (((byte) (this.headers[i].HeaderProcessing & HeaderProcessing.Understood)) != 0)
                {
                    list.Add(this.headers[i].HeaderInfo);
                }
            }
            return list.GetEnumerator();
        }

        public bool HaveMandatoryHeadersBeenUnderstood()
        {
            return this.HaveMandatoryHeadersBeenUnderstood(this.version.Envelope.MustUnderstandActorValues);
        }

        public bool HaveMandatoryHeadersBeenUnderstood(params string[] actors)
        {
            if (actors == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("actors"));
            }
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderProcessing == HeaderProcessing.MustUnderstand)
                {
                    for (int j = 0; j < actors.Length; j++)
                    {
                        if (this.headers[i].HeaderInfo.Actor == actors[j])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal void Init(System.ServiceModel.Channels.MessageVersion version)
        {
            this.nodeCount = 0;
            this.attrCount = 0;
            this.version = version;
            this.collectionVersion = 0;
        }

        internal void Init(System.ServiceModel.Channels.MessageVersion version, int initialSize)
        {
            this.nodeCount = 0;
            this.attrCount = 0;
            if (initialSize < 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("initialSize", initialSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            this.version = version;
            this.headers = new Header[initialSize];
        }

        internal void Init(System.ServiceModel.Channels.MessageVersion version, XmlDictionaryReader reader, IBufferedMessageData bufferedMessageData, RecycledMessageState recycledMessageState, bool[] understoodHeaders, bool understoodHeadersModified)
        {
            this.nodeCount = 0;
            this.attrCount = 0;
            this.version = version;
            this.bufferedMessageData = bufferedMessageData;
            if (version.Envelope != EnvelopeVersion.None)
            {
                this.understoodHeadersModified = (understoodHeaders != null) && understoodHeadersModified;
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                    return;
                }
                EnvelopeVersion envelope = version.Envelope;
                reader.ReadStartElement();
                AddressingDictionary addressingDictionary = XD.AddressingDictionary;
                if (localNames == null)
                {
                    XmlDictionaryString[] strArray = new XmlDictionaryString[7];
                    strArray[6] = addressingDictionary.To;
                    strArray[0] = addressingDictionary.Action;
                    strArray[3] = addressingDictionary.MessageId;
                    strArray[5] = addressingDictionary.RelatesTo;
                    strArray[4] = addressingDictionary.ReplyTo;
                    strArray[2] = addressingDictionary.From;
                    strArray[1] = addressingDictionary.FaultTo;
                    Thread.MemoryBarrier();
                    localNames = strArray;
                }
                int num = 0;
                while (reader.IsStartElement())
                {
                    this.ReadBufferedHeader(reader, recycledMessageState, localNames, (understoodHeaders != null) && understoodHeaders[num++]);
                }
                reader.ReadEndElement();
            }
            this.collectionVersion = 0;
        }

        public void Insert(int headerIndex, MessageHeader header)
        {
            if (header == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("header"));
            }
            if (!header.IsMessageVersionSupported(this.version))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessageHeaderVersionNotSupported", new object[] { header.GetType().FullName, this.version.Envelope.ToString() }), "header"));
            }
            this.Insert(headerIndex, header, this.GetHeaderKind(header));
        }

        private void Insert(int headerIndex, MessageHeader header, HeaderKind kind)
        {
            ReadableMessageHeader readableHeader = header as ReadableMessageHeader;
            HeaderProcessing processing = header.MustUnderstand ? HeaderProcessing.MustUnderstand : ((HeaderProcessing) 0);
            if (kind != HeaderKind.Unknown)
            {
                processing = (HeaderProcessing) ((byte) (processing | HeaderProcessing.Understood));
            }
            if (readableHeader != null)
            {
                this.InsertHeader(headerIndex, new Header(kind, readableHeader, processing));
            }
            else
            {
                this.InsertHeader(headerIndex, new Header(kind, header, processing));
            }
        }

        private void InsertHeader(int headerIndex, Header header)
        {
            this.ValidateHeaderKind(header.HeaderKind);
            if ((headerIndex < 0) || (headerIndex > this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", headerIndex, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            if (this.headerCount == this.headers.Length)
            {
                if (this.headers.Length == 0)
                {
                    this.headers = new Header[1];
                }
                else
                {
                    Header[] array = new Header[this.headers.Length * 2];
                    this.headers.CopyTo(array, 0);
                    this.headers = array;
                }
            }
            if (headerIndex < this.headerCount)
            {
                if (this.bufferedMessageData != null)
                {
                    for (int i = headerIndex; i < this.headerCount; i++)
                    {
                        if (this.headers[i].HeaderType == HeaderType.BufferedMessageHeader)
                        {
                            this.CaptureBufferedHeaders();
                            break;
                        }
                    }
                }
                Array.Copy(this.headers, headerIndex, this.headers, headerIndex + 1, this.headerCount - headerIndex);
            }
            this.headers[headerIndex] = header;
            this.headerCount++;
            this.collectionVersion++;
        }

        internal bool IsUnderstood(int i)
        {
            return (((byte) (this.headers[i].HeaderProcessing & HeaderProcessing.Understood)) != 0);
        }

        internal bool IsUnderstood(MessageHeaderInfo headerInfo)
        {
            if (headerInfo == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("headerInfo"));
            }
            for (int i = 0; i < this.headerCount; i++)
            {
                if ((this.headers[i].HeaderInfo == headerInfo) && this.IsUnderstood(i))
                {
                    return true;
                }
            }
            return false;
        }

        private void ReadBufferedHeader(XmlDictionaryReader reader, RecycledMessageState recycledMessageState, XmlDictionaryString[] localNames, bool understood)
        {
            string str;
            bool flag;
            bool flag2;
            bool flag3;
            if ((this.version.Addressing == AddressingVersion.None) && (reader.NamespaceURI == AddressingVersion.None.Namespace))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AddressingHeadersCannotBeAddedToAddressingVersion", new object[] { this.version.Addressing })));
            }
            MessageHeader.GetHeaderAttributes(reader, this.version, out str, out flag, out flag2, out flag3);
            HeaderKind unknown = HeaderKind.Unknown;
            MessageHeaderInfo info = null;
            if (this.version.Envelope.IsUltimateDestinationActor(str))
            {
                switch (((HeaderKind) ((byte) reader.IndexOfLocalName(localNames, this.version.Addressing.DictionaryNamespace))))
                {
                    case HeaderKind.Action:
                        info = ActionHeader.ReadHeader(reader, this.version.Addressing, str, flag, flag2);
                        goto Label_0186;

                    case HeaderKind.FaultTo:
                        info = FaultToHeader.ReadHeader(reader, this.version.Addressing, str, flag, flag2);
                        goto Label_0186;

                    case HeaderKind.From:
                        info = FromHeader.ReadHeader(reader, this.version.Addressing, str, flag, flag2);
                        goto Label_0186;

                    case HeaderKind.MessageId:
                        info = MessageIDHeader.ReadHeader(reader, this.version.Addressing, str, flag, flag2);
                        goto Label_0186;

                    case HeaderKind.ReplyTo:
                        info = ReplyToHeader.ReadHeader(reader, this.version.Addressing, str, flag, flag2);
                        goto Label_0186;

                    case HeaderKind.RelatesTo:
                        info = RelatesToHeader.ReadHeader(reader, this.version.Addressing, str, flag, flag2);
                        goto Label_0186;

                    case HeaderKind.To:
                        info = ToHeader.ReadHeader(reader, this.version.Addressing, recycledMessageState.UriCache, str, flag, flag2);
                        goto Label_0186;
                }
                unknown = HeaderKind.Unknown;
            }
        Label_0186:
            if (info == null)
            {
                info = recycledMessageState.HeaderInfoCache.TakeHeaderInfo(reader, str, flag, flag2, flag3);
                reader.Skip();
            }
            HeaderProcessing processing = flag ? HeaderProcessing.MustUnderstand : ((HeaderProcessing) 0);
            if ((unknown != HeaderKind.Unknown) || understood)
            {
                processing = (HeaderProcessing) ((byte) (processing | HeaderProcessing.Understood));
                TraceUnderstood(info);
            }
            this.AddHeader(new Header(unknown, info, processing));
        }

        internal void Recycle(HeaderInfoCache headerInfoCache)
        {
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderKind == HeaderKind.Unknown)
                {
                    headerInfoCache.ReturnHeaderInfo(this.headers[i].HeaderInfo);
                }
            }
            this.Clear();
            this.collectionVersion = 0;
            if (this.understoodHeaders != null)
            {
                this.understoodHeaders.Modified = false;
            }
        }

        public void RemoveAll(string name, string ns)
        {
            if (name == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (ns == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            }
            for (int i = this.headerCount - 1; i >= 0; i--)
            {
                MessageHeaderInfo headerInfo = this.headers[i].HeaderInfo;
                if ((headerInfo.Name == name) && (headerInfo.Namespace == ns))
                {
                    this.RemoveAt(i);
                }
            }
        }

        public void RemoveAt(int headerIndex)
        {
            if ((headerIndex < 0) || (headerIndex >= this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", headerIndex, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            if ((this.bufferedMessageData != null) && (this.headers[headerIndex].HeaderType == HeaderType.BufferedMessageHeader))
            {
                this.CaptureBufferedHeaders(headerIndex);
            }
            Array.Copy(this.headers, headerIndex + 1, this.headers, headerIndex, (this.headerCount - headerIndex) - 1);
            this.headers[--this.headerCount] = new Header();
            this.collectionVersion++;
        }

        internal void RemoveUnderstood(MessageHeaderInfo headerInfo)
        {
            if (headerInfo == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("headerInfo"));
            }
            for (int i = 0; i < this.headerCount; i++)
            {
                if (this.headers[i].HeaderInfo == headerInfo)
                {
                    if (((byte) (this.headers[i].HeaderProcessing & HeaderProcessing.Understood)) == 0)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("HeaderAlreadyNotUnderstood", new object[] { headerInfo.Name, headerInfo.Namespace }), "headerInfo"));
                    }
                    this.headers[i].HeaderProcessing = (HeaderProcessing) ((byte) (((int) this.headers[i].HeaderProcessing) & 0xfd));
                }
            }
        }

        internal void ReplaceAt(int headerIndex, MessageHeader header)
        {
            if ((headerIndex < 0) || (headerIndex >= this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", headerIndex, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            if (header == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("header");
            }
            this.ReplaceAt(headerIndex, header, this.GetHeaderKind(header));
        }

        private void ReplaceAt(int headerIndex, MessageHeader header, HeaderKind kind)
        {
            HeaderProcessing processing = header.MustUnderstand ? HeaderProcessing.MustUnderstand : ((HeaderProcessing) 0);
            if (kind != HeaderKind.Unknown)
            {
                processing = (HeaderProcessing) ((byte) (processing | HeaderProcessing.Understood));
            }
            ReadableMessageHeader readableHeader = header as ReadableMessageHeader;
            if (readableHeader != null)
            {
                this.headers[headerIndex] = new Header(kind, readableHeader, processing);
            }
            else
            {
                this.headers[headerIndex] = new Header(kind, header, processing);
            }
            this.collectionVersion++;
        }

        public void SetAction(XmlDictionaryString action)
        {
            if (action == null)
            {
                this.SetHeaderProperty(HeaderKind.Action, null);
            }
            else
            {
                this.SetActionHeader(ActionHeader.Create(action, this.version.Addressing));
            }
        }

        internal void SetActionHeader(ActionHeader actionHeader)
        {
            this.SetHeaderProperty(HeaderKind.Action, actionHeader);
        }

        internal void SetFaultToHeader(FaultToHeader faultToHeader)
        {
            this.SetHeaderProperty(HeaderKind.FaultTo, faultToHeader);
        }

        internal void SetFromHeader(FromHeader fromHeader)
        {
            this.SetHeaderProperty(HeaderKind.From, fromHeader);
        }

        private void SetHeaderProperty(HeaderKind kind, MessageHeader header)
        {
            int headerIndex = this.FindHeaderProperty(kind);
            if (headerIndex >= 0)
            {
                if (header == null)
                {
                    this.RemoveAt(headerIndex);
                }
                else
                {
                    this.ReplaceAt(headerIndex, header, kind);
                }
            }
            else if (header != null)
            {
                this.Add(header, kind);
            }
        }

        internal void SetMessageIDHeader(MessageIDHeader messageIDHeader)
        {
            this.SetHeaderProperty(HeaderKind.MessageId, messageIDHeader);
        }

        private void SetRelatesTo(Uri relationshipType, RelatesToHeader relatesToHeader)
        {
            UniqueId id;
            int headerIndex = this.FindRelatesTo(relationshipType, out id);
            if (headerIndex >= 0)
            {
                if (relatesToHeader == null)
                {
                    this.RemoveAt(headerIndex);
                }
                else
                {
                    this.ReplaceAt(headerIndex, relatesToHeader, HeaderKind.RelatesTo);
                }
            }
            else if (relatesToHeader != null)
            {
                this.Add(relatesToHeader, HeaderKind.RelatesTo);
            }
        }

        internal void SetRelatesTo(Uri relationshipType, UniqueId messageId)
        {
            RelatesToHeader header;
            if (relationshipType == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("relationshipType");
            }
            if (!object.ReferenceEquals(messageId, null))
            {
                header = RelatesToHeader.Create(messageId, this.version.Addressing, relationshipType);
            }
            else
            {
                header = null;
            }
            this.SetRelatesTo(RelatesToHeader.ReplyRelationshipType, header);
        }

        internal void SetReplyToHeader(ReplyToHeader replyToHeader)
        {
            this.SetHeaderProperty(HeaderKind.ReplyTo, replyToHeader);
        }

        internal void SetToHeader(ToHeader toHeader)
        {
            this.SetHeaderProperty(HeaderKind.To, toHeader);
        }

        private void Skip(XmlDictionaryReader reader)
        {
            if ((reader.MoveToContent() == XmlNodeType.Element) && !reader.IsEmptyElement)
            {
                int depth = reader.Depth;
                do
                {
                    this.attrCount += reader.AttributeCount;
                    this.nodeCount++;
                }
                while (reader.Read() && (depth < reader.Depth));
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    this.nodeCount++;
                    reader.Read();
                }
            }
            else
            {
                this.attrCount += reader.AttributeCount;
                this.nodeCount++;
                reader.Read();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static void TraceUnderstood(MessageHeaderInfo info)
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x8000f, System.ServiceModel.SR.GetString("TraceCodeUnderstoodMessageHeader"), new MessageHeaderInfoTraceRecord(info), null, null);
            }
        }

        private void ValidateHeaderKind(HeaderKind headerKind)
        {
            if (((this.version.Envelope == EnvelopeVersion.None) && (headerKind != HeaderKind.Action)) && (headerKind != HeaderKind.To))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("HeadersCannotBeAddedToEnvelopeVersion", new object[] { this.version.Envelope })));
            }
            if (((this.version.Addressing == AddressingVersion.None) && (headerKind != HeaderKind.Unknown)) && ((headerKind != HeaderKind.Action) && (headerKind != HeaderKind.To)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AddressingHeadersCannotBeAddedToAddressingVersion", new object[] { this.version.Addressing })));
            }
        }

        private void WriteBufferedMessageHeader(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex, XmlWriter writer)
        {
            using (XmlReader reader = this.GetBufferedMessageHeaderReader(bufferedMessageData, bufferedMessageHeaderIndex))
            {
                writer.WriteNode(reader, false);
            }
        }

        private void WriteBufferedMessageHeaderContents(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex, XmlWriter writer)
        {
            using (XmlReader reader = this.GetBufferedMessageHeaderReader(bufferedMessageData, bufferedMessageHeaderIndex))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        writer.WriteNode(reader, false);
                    }
                    reader.ReadEndElement();
                }
            }
        }

        public void WriteHeader(int headerIndex, XmlDictionaryWriter writer)
        {
            this.WriteStartHeader(headerIndex, writer);
            this.WriteHeaderContents(headerIndex, writer);
            writer.WriteEndElement();
        }

        public void WriteHeader(int headerIndex, XmlWriter writer)
        {
            this.WriteHeader(headerIndex, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public void WriteHeaderContents(int headerIndex, XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if ((headerIndex < 0) || (headerIndex >= this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", headerIndex, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            switch (this.headers[headerIndex].HeaderType)
            {
                case HeaderType.ReadableHeader:
                case HeaderType.WriteableHeader:
                    this.headers[headerIndex].MessageHeader.WriteHeaderContents(writer, this.version);
                    return;

                case HeaderType.BufferedMessageHeader:
                    this.WriteBufferedMessageHeaderContents(this.bufferedMessageData, headerIndex, writer);
                    return;
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidEnumValue", new object[] { this.headers[headerIndex].HeaderType })));
        }

        public void WriteHeaderContents(int headerIndex, XmlWriter writer)
        {
            this.WriteHeaderContents(headerIndex, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        private void WriteStartBufferedMessageHeader(IBufferedMessageData bufferedMessageData, int bufferedMessageHeaderIndex, XmlWriter writer)
        {
            using (XmlReader reader = this.GetBufferedMessageHeaderReader(bufferedMessageData, bufferedMessageHeaderIndex))
            {
                writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                writer.WriteAttributes(reader, false);
            }
        }

        public void WriteStartHeader(int headerIndex, XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if ((headerIndex < 0) || (headerIndex >= this.headerCount))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("headerIndex", headerIndex, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
            }
            switch (this.headers[headerIndex].HeaderType)
            {
                case HeaderType.ReadableHeader:
                case HeaderType.WriteableHeader:
                    this.headers[headerIndex].MessageHeader.WriteStartHeader(writer, this.version);
                    return;

                case HeaderType.BufferedMessageHeader:
                    this.WriteStartBufferedMessageHeader(this.bufferedMessageData, headerIndex, writer);
                    return;
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidEnumValue", new object[] { this.headers[headerIndex].HeaderType })));
        }

        public void WriteStartHeader(int headerIndex, XmlWriter writer)
        {
            this.WriteStartHeader(headerIndex, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public string Action
        {
            get
            {
                int index = this.FindHeaderProperty(HeaderKind.Action);
                if (index < 0)
                {
                    return null;
                }
                ActionHeader headerInfo = this.headers[index].HeaderInfo as ActionHeader;
                if (headerInfo != null)
                {
                    return headerInfo.Action;
                }
                using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
                {
                    return ActionHeader.ReadHeaderValue(reader, this.version.Addressing);
                }
            }
            set
            {
                if (value != null)
                {
                    this.SetActionHeader(ActionHeader.Create(value, this.version.Addressing));
                }
                else
                {
                    this.SetHeaderProperty(HeaderKind.Action, null);
                }
            }
        }

        internal bool CanRecycle
        {
            get
            {
                return (this.headers.Length <= 8);
            }
        }

        internal int CollectionVersion
        {
            get
            {
                return this.collectionVersion;
            }
        }

        internal bool ContainsOnlyBufferedMessageHeaders
        {
            get
            {
                return ((this.bufferedMessageData != null) && (this.collectionVersion == 0));
            }
        }

        public int Count
        {
            get
            {
                return this.headerCount;
            }
        }

        public EndpointAddress FaultTo
        {
            get
            {
                int index = this.FindHeaderProperty(HeaderKind.FaultTo);
                if (index < 0)
                {
                    return null;
                }
                FaultToHeader headerInfo = this.headers[index].HeaderInfo as FaultToHeader;
                if (headerInfo != null)
                {
                    return headerInfo.FaultTo;
                }
                using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
                {
                    return FaultToHeader.ReadHeaderValue(reader, this.version.Addressing);
                }
            }
            set
            {
                if (value != null)
                {
                    this.SetFaultToHeader(FaultToHeader.Create(value, this.version.Addressing));
                }
                else
                {
                    this.SetHeaderProperty(HeaderKind.FaultTo, null);
                }
            }
        }

        public EndpointAddress From
        {
            get
            {
                int index = this.FindHeaderProperty(HeaderKind.From);
                if (index < 0)
                {
                    return null;
                }
                FromHeader headerInfo = this.headers[index].HeaderInfo as FromHeader;
                if (headerInfo != null)
                {
                    return headerInfo.From;
                }
                using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
                {
                    return FromHeader.ReadHeaderValue(reader, this.version.Addressing);
                }
            }
            set
            {
                if (value != null)
                {
                    this.SetFromHeader(FromHeader.Create(value, this.version.Addressing));
                }
                else
                {
                    this.SetHeaderProperty(HeaderKind.From, null);
                }
            }
        }

        internal bool HasMustUnderstandBeenModified
        {
            get
            {
                if (this.understoodHeaders != null)
                {
                    return this.understoodHeaders.Modified;
                }
                return this.understoodHeadersModified;
            }
        }

        public MessageHeaderInfo this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.headerCount))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.headerCount })));
                }
                return this.headers[index].HeaderInfo;
            }
        }

        public UniqueId MessageId
        {
            get
            {
                int index = this.FindHeaderProperty(HeaderKind.MessageId);
                if (index < 0)
                {
                    return null;
                }
                MessageIDHeader headerInfo = this.headers[index].HeaderInfo as MessageIDHeader;
                if (headerInfo != null)
                {
                    return headerInfo.MessageId;
                }
                using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
                {
                    return MessageIDHeader.ReadHeaderValue(reader, this.version.Addressing);
                }
            }
            set
            {
                if (value != null)
                {
                    this.SetMessageIDHeader(MessageIDHeader.Create(value, this.version.Addressing));
                }
                else
                {
                    this.SetHeaderProperty(HeaderKind.MessageId, null);
                }
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.version;
            }
        }

        public UniqueId RelatesTo
        {
            get
            {
                return this.GetRelatesTo(RelatesToHeader.ReplyRelationshipType);
            }
            set
            {
                this.SetRelatesTo(RelatesToHeader.ReplyRelationshipType, value);
            }
        }

        public EndpointAddress ReplyTo
        {
            get
            {
                int index = this.FindHeaderProperty(HeaderKind.ReplyTo);
                if (index < 0)
                {
                    return null;
                }
                ReplyToHeader headerInfo = this.headers[index].HeaderInfo as ReplyToHeader;
                if (headerInfo != null)
                {
                    return headerInfo.ReplyTo;
                }
                using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
                {
                    return ReplyToHeader.ReadHeaderValue(reader, this.version.Addressing);
                }
            }
            set
            {
                if (value != null)
                {
                    this.SetReplyToHeader(ReplyToHeader.Create(value, this.version.Addressing));
                }
                else
                {
                    this.SetHeaderProperty(HeaderKind.ReplyTo, null);
                }
            }
        }

        public Uri To
        {
            get
            {
                int index = this.FindHeaderProperty(HeaderKind.To);
                if (index < 0)
                {
                    return null;
                }
                ToHeader headerInfo = this.headers[index].HeaderInfo as ToHeader;
                if (headerInfo != null)
                {
                    return headerInfo.To;
                }
                using (XmlDictionaryReader reader = this.GetReaderAtHeader(index))
                {
                    return ToHeader.ReadHeaderValue(reader, this.version.Addressing);
                }
            }
            set
            {
                if (value != null)
                {
                    this.SetToHeader(ToHeader.Create(value, this.version.Addressing));
                }
                else
                {
                    this.SetHeaderProperty(HeaderKind.To, null);
                }
            }
        }

        public System.ServiceModel.Channels.UnderstoodHeaders UnderstoodHeaders
        {
            get
            {
                if (this.understoodHeaders == null)
                {
                    this.understoodHeaders = new System.ServiceModel.Channels.UnderstoodHeaders(this, this.understoodHeadersModified);
                }
                return this.understoodHeaders;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Header
        {
            private System.ServiceModel.Channels.MessageHeaders.HeaderType type;
            private System.ServiceModel.Channels.MessageHeaders.HeaderKind kind;
            private System.ServiceModel.Channels.MessageHeaders.HeaderProcessing processing;
            private MessageHeaderInfo info;
            public Header(System.ServiceModel.Channels.MessageHeaders.HeaderKind kind, MessageHeaderInfo info, System.ServiceModel.Channels.MessageHeaders.HeaderProcessing processing)
            {
                this.kind = kind;
                this.type = System.ServiceModel.Channels.MessageHeaders.HeaderType.BufferedMessageHeader;
                this.info = info;
                this.processing = processing;
            }

            public Header(System.ServiceModel.Channels.MessageHeaders.HeaderKind kind, ReadableMessageHeader readableHeader, System.ServiceModel.Channels.MessageHeaders.HeaderProcessing processing)
            {
                this.kind = kind;
                this.type = System.ServiceModel.Channels.MessageHeaders.HeaderType.ReadableHeader;
                this.info = readableHeader;
                this.processing = processing;
            }

            public Header(System.ServiceModel.Channels.MessageHeaders.HeaderKind kind, System.ServiceModel.Channels.MessageHeader header, System.ServiceModel.Channels.MessageHeaders.HeaderProcessing processing)
            {
                this.kind = kind;
                this.type = System.ServiceModel.Channels.MessageHeaders.HeaderType.WriteableHeader;
                this.info = header;
                this.processing = processing;
            }

            public System.ServiceModel.Channels.MessageHeaders.HeaderType HeaderType
            {
                get
                {
                    return this.type;
                }
            }
            public System.ServiceModel.Channels.MessageHeaders.HeaderKind HeaderKind
            {
                get
                {
                    return this.kind;
                }
            }
            public MessageHeaderInfo HeaderInfo
            {
                get
                {
                    return this.info;
                }
            }
            public System.ServiceModel.Channels.MessageHeader MessageHeader
            {
                get
                {
                    return (System.ServiceModel.Channels.MessageHeader) this.info;
                }
            }
            public System.ServiceModel.Channels.MessageHeaders.HeaderProcessing HeaderProcessing
            {
                get
                {
                    return this.processing;
                }
                set
                {
                    this.processing = value;
                }
            }
            public ReadableMessageHeader ReadableHeader
            {
                get
                {
                    return (ReadableMessageHeader) this.info;
                }
            }
        }

        private enum HeaderKind : byte
        {
            Action = 0,
            FaultTo = 1,
            From = 2,
            MessageId = 3,
            RelatesTo = 5,
            ReplyTo = 4,
            To = 6,
            Unknown = 7
        }

        [Flags]
        private enum HeaderProcessing : byte
        {
            MustUnderstand = 1,
            Understood = 2
        }

        private enum HeaderType : byte
        {
            BufferedMessageHeader = 2,
            Invalid = 0,
            ReadableHeader = 1,
            WriteableHeader = 3
        }
    }
}

