namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    internal class MessagePatterns
    {
        private static readonly byte[] bodyFragment;
        private static readonly byte[] commonFragment;
        private IXmlDictionary dictionary;
        private MessageVersion messageVersion;
        private XmlBinaryReaderSession readerSession;
        private static readonly byte[] requestFragment1;
        private static readonly byte[] requestFragment2;
        private static readonly byte[] responseFragment1;
        private static readonly byte[] responseFragment2;
        private ToHeader toHeader;
        private const int ToValueSessionKey = 1;

        static MessagePatterns()
        {
            BinaryFormatBuilder builder = new BinaryFormatBuilder();
            MessageDictionary messageDictionary = XD.MessageDictionary;
            Message12Dictionary dictionary2 = XD.Message12Dictionary;
            AddressingDictionary addressingDictionary = XD.AddressingDictionary;
            Addressing10Dictionary dictionary4 = XD.Addressing10Dictionary;
            char prefix = "s"[0];
            char ch2 = "a"[0];
            builder.AppendPrefixDictionaryElement(prefix, builder.GetStaticKey(messageDictionary.Envelope.Key));
            builder.AppendDictionaryXmlnsAttribute(prefix, builder.GetStaticKey(dictionary2.Namespace.Key));
            builder.AppendDictionaryXmlnsAttribute(ch2, builder.GetStaticKey(dictionary4.Namespace.Key));
            builder.AppendPrefixDictionaryElement(prefix, builder.GetStaticKey(messageDictionary.Header.Key));
            builder.AppendPrefixDictionaryElement(ch2, builder.GetStaticKey(addressingDictionary.Action.Key));
            builder.AppendPrefixDictionaryAttribute(prefix, builder.GetStaticKey(messageDictionary.MustUnderstand.Key), '1');
            builder.AppendDictionaryTextWithEndElement();
            commonFragment = builder.ToByteArray();
            builder.AppendPrefixDictionaryElement(ch2, builder.GetStaticKey(addressingDictionary.MessageId.Key));
            builder.AppendUniqueIDWithEndElement();
            requestFragment1 = builder.ToByteArray();
            builder.AppendPrefixDictionaryElement(ch2, builder.GetStaticKey(addressingDictionary.ReplyTo.Key));
            builder.AppendPrefixDictionaryElement(ch2, builder.GetStaticKey(addressingDictionary.Address.Key));
            builder.AppendDictionaryTextWithEndElement(builder.GetStaticKey(dictionary4.Anonymous.Key));
            builder.AppendEndElement();
            builder.AppendPrefixDictionaryElement(ch2, builder.GetStaticKey(addressingDictionary.To.Key));
            builder.AppendPrefixDictionaryAttribute(prefix, builder.GetStaticKey(messageDictionary.MustUnderstand.Key), '1');
            builder.AppendDictionaryTextWithEndElement(builder.GetSessionKey(1));
            builder.AppendEndElement();
            builder.AppendPrefixDictionaryElement(prefix, builder.GetStaticKey(messageDictionary.Body.Key));
            requestFragment2 = builder.ToByteArray();
            builder.AppendPrefixDictionaryElement(ch2, builder.GetStaticKey(addressingDictionary.RelatesTo.Key));
            builder.AppendUniqueIDWithEndElement();
            responseFragment1 = builder.ToByteArray();
            builder.AppendPrefixDictionaryElement(ch2, builder.GetStaticKey(addressingDictionary.To.Key));
            builder.AppendPrefixDictionaryAttribute(prefix, builder.GetStaticKey(messageDictionary.MustUnderstand.Key), '1');
            builder.AppendDictionaryTextWithEndElement(builder.GetStaticKey(dictionary4.Anonymous.Key));
            builder.AppendEndElement();
            builder.AppendPrefixDictionaryElement(prefix, builder.GetStaticKey(messageDictionary.Body.Key));
            responseFragment2 = builder.ToByteArray();
            builder.AppendPrefixDictionaryElement(prefix, builder.GetStaticKey(messageDictionary.Envelope.Key));
            builder.AppendDictionaryXmlnsAttribute(prefix, builder.GetStaticKey(dictionary2.Namespace.Key));
            builder.AppendDictionaryXmlnsAttribute(ch2, builder.GetStaticKey(dictionary4.Namespace.Key));
            builder.AppendPrefixDictionaryElement(prefix, builder.GetStaticKey(messageDictionary.Body.Key));
            bodyFragment = builder.ToByteArray();
        }

        public MessagePatterns(IXmlDictionary dictionary, XmlBinaryReaderSession readerSession, MessageVersion messageVersion)
        {
            this.dictionary = dictionary;
            this.readerSession = readerSession;
            this.messageVersion = messageVersion;
        }

        public Message TryCreateMessage(byte[] buffer, int offset, int size, BufferManager bufferManager, BufferedMessageData messageData)
        {
            RelatesToHeader header;
            MessageIDHeader header2;
            XmlDictionaryString anonymous;
            int num6;
            XmlDictionaryString str2;
            int num = offset;
            int num2 = size;
            int num3 = BinaryFormatParser.MatchBytes(buffer, num, num2, commonFragment);
            if (num3 == 0)
            {
                return null;
            }
            num += num3;
            num2 -= num3;
            num3 = BinaryFormatParser.MatchKey(buffer, num, num2);
            if (num3 == 0)
            {
                return null;
            }
            int num4 = num;
            int num5 = num3;
            num += num3;
            num2 -= num3;
            num3 = BinaryFormatParser.MatchBytes(buffer, num, num2, requestFragment1);
            if (num3 != 0)
            {
                num += num3;
                num2 -= num3;
                num3 = BinaryFormatParser.MatchUniqueID(buffer, num, num2);
                if (num3 == 0)
                {
                    return null;
                }
                int num7 = num;
                int num8 = num3;
                num += num3;
                num2 -= num3;
                num3 = BinaryFormatParser.MatchBytes(buffer, num, num2, requestFragment2);
                if (num3 == 0)
                {
                    return null;
                }
                num += num3;
                num2 -= num3;
                if (BinaryFormatParser.MatchAttributeNode(buffer, num, num2))
                {
                    return null;
                }
                header2 = MessageIDHeader.Create(BinaryFormatParser.ParseUniqueID(buffer, num7, num8), this.messageVersion.Addressing);
                header = null;
                if (!this.readerSession.TryLookup(1, out anonymous))
                {
                    return null;
                }
                num6 = (requestFragment1.Length + num8) + requestFragment2.Length;
            }
            else
            {
                num3 = BinaryFormatParser.MatchBytes(buffer, num, num2, responseFragment1);
                if (num3 == 0)
                {
                    return null;
                }
                num += num3;
                num2 -= num3;
                num3 = BinaryFormatParser.MatchUniqueID(buffer, num, num2);
                if (num3 == 0)
                {
                    return null;
                }
                int num9 = num;
                int num10 = num3;
                num += num3;
                num2 -= num3;
                num3 = BinaryFormatParser.MatchBytes(buffer, num, num2, responseFragment2);
                if (num3 == 0)
                {
                    return null;
                }
                num += num3;
                num2 -= num3;
                if (BinaryFormatParser.MatchAttributeNode(buffer, num, num2))
                {
                    return null;
                }
                header = RelatesToHeader.Create(BinaryFormatParser.ParseUniqueID(buffer, num9, num10), this.messageVersion.Addressing);
                header2 = null;
                anonymous = XD.Addressing10Dictionary.Anonymous;
                num6 = (responseFragment1.Length + num10) + responseFragment2.Length;
            }
            num6 += commonFragment.Length + num5;
            int key = BinaryFormatParser.ParseKey(buffer, num4, num5);
            if (!this.TryLookupKey(key, out str2))
            {
                return null;
            }
            ActionHeader actionHeader = ActionHeader.Create(str2, this.messageVersion.Addressing);
            if (this.toHeader == null)
            {
                this.toHeader = ToHeader.Create(new Uri(anonymous.Value), this.messageVersion.Addressing);
            }
            int num12 = num6 - bodyFragment.Length;
            offset += num12;
            size -= num12;
            Buffer.BlockCopy(bodyFragment, 0, buffer, offset, bodyFragment.Length);
            messageData.Open(new ArraySegment<byte>(buffer, offset, size), bufferManager);
            PatternMessage message = new PatternMessage(messageData, this.messageVersion);
            MessageHeaders headers = message.Headers;
            headers.AddActionHeader(actionHeader);
            if (header2 != null)
            {
                headers.AddMessageIDHeader(header2);
                headers.AddReplyToHeader(ReplyToHeader.AnonymousReplyTo10);
            }
            else
            {
                headers.AddRelatesToHeader(header);
            }
            headers.AddToHeader(this.toHeader);
            return message;
        }

        private bool TryLookupKey(int key, out XmlDictionaryString result)
        {
            if (BinaryFormatParser.IsSessionKey(key))
            {
                return this.readerSession.TryLookup(BinaryFormatParser.GetSessionKey(key), out result);
            }
            return this.dictionary.TryLookup(BinaryFormatParser.GetStaticKey(key), out result);
        }

        private sealed class PatternMessage : ReceivedMessage
        {
            private MessageHeaders headers;
            private IBufferedMessageData messageData;
            private MessageProperties properties;
            private XmlDictionaryReader reader;
            private System.ServiceModel.Channels.RecycledMessageState recycledMessageState;

            public PatternMessage(IBufferedMessageData messageData, MessageVersion messageVersion)
            {
                this.messageData = messageData;
                this.recycledMessageState = messageData.TakeMessageState();
                if (this.recycledMessageState == null)
                {
                    this.recycledMessageState = new System.ServiceModel.Channels.RecycledMessageState();
                }
                this.properties = this.recycledMessageState.TakeProperties();
                if (this.properties == null)
                {
                    this.properties = new MessageProperties();
                }
                this.headers = this.recycledMessageState.TakeHeaders();
                if (this.headers == null)
                {
                    this.headers = new MessageHeaders(messageVersion);
                }
                else
                {
                    this.headers.Init(messageVersion);
                }
                XmlDictionaryReader messageReader = messageData.GetMessageReader();
                messageReader.ReadStartElement();
                ReceivedMessage.VerifyStartBody(messageReader, messageVersion.Envelope);
                base.ReadStartBody(messageReader);
                this.reader = messageReader;
            }

            public PatternMessage(IBufferedMessageData messageData, MessageVersion messageVersion, KeyValuePair<string, object>[] properties, MessageHeaders headers)
            {
                this.messageData = messageData;
                this.messageData.Open();
                this.recycledMessageState = this.messageData.TakeMessageState();
                if (this.recycledMessageState == null)
                {
                    this.recycledMessageState = new System.ServiceModel.Channels.RecycledMessageState();
                }
                this.properties = this.recycledMessageState.TakeProperties();
                if (this.properties == null)
                {
                    this.properties = new MessageProperties();
                }
                if (properties != null)
                {
                    this.properties.CopyProperties(properties);
                }
                this.headers = this.recycledMessageState.TakeHeaders();
                if (this.headers == null)
                {
                    this.headers = new MessageHeaders(messageVersion);
                }
                if (headers != null)
                {
                    this.headers.CopyHeadersFrom(headers);
                }
                XmlDictionaryReader messageReader = messageData.GetMessageReader();
                messageReader.ReadStartElement();
                ReceivedMessage.VerifyStartBody(messageReader, messageVersion.Envelope);
                base.ReadStartBody(messageReader);
                this.reader = messageReader;
            }

            private XmlDictionaryReader GetBufferedReaderAtBody()
            {
                XmlDictionaryReader messageReader = this.messageData.GetMessageReader();
                messageReader.ReadStartElement();
                messageReader.ReadStartElement();
                return messageReader;
            }

            protected override void OnBodyToString(XmlDictionaryWriter writer)
            {
                using (XmlDictionaryReader reader = this.GetBufferedReaderAtBody())
                {
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        writer.WriteNode(reader, false);
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
                KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[this.Properties.Count];
                ((ICollection<KeyValuePair<string, object>>) this.Properties).CopyTo(array, 0);
                this.messageData.EnableMultipleUsers();
                return new MessagePatterns.PatternMessageBuffer(this.messageData, this.Version, array, this.headers);
            }

            protected override string OnGetBodyAttribute(string localName, string ns)
            {
                return null;
            }

            protected override XmlDictionaryReader OnGetReaderAtBodyContents()
            {
                XmlDictionaryReader reader = this.reader;
                this.reader = null;
                return reader;
            }

            public override MessageHeaders Headers
            {
                get
                {
                    if (base.IsDisposed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateMessageDisposedException());
                    }
                    return this.headers;
                }
            }

            public override MessageProperties Properties
            {
                get
                {
                    if (base.IsDisposed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateMessageDisposedException());
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
                    if (base.IsDisposed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateMessageDisposedException());
                    }
                    return this.headers.MessageVersion;
                }
            }
        }

        private class PatternMessageBuffer : MessageBuffer
        {
            private bool closed;
            private MessageHeaders headers;
            private IBufferedMessageData messageDataAtBody;
            private MessageVersion messageVersion;
            private KeyValuePair<string, object>[] properties;
            private RecycledMessageState recycledMessageState;
            private object thisLock = new object();

            public PatternMessageBuffer(IBufferedMessageData messageDataAtBody, MessageVersion messageVersion, KeyValuePair<string, object>[] properties, MessageHeaders headers)
            {
                this.messageDataAtBody = messageDataAtBody;
                this.messageDataAtBody.Open();
                this.recycledMessageState = this.messageDataAtBody.TakeMessageState();
                if (this.recycledMessageState == null)
                {
                    this.recycledMessageState = new RecycledMessageState();
                }
                this.headers = this.recycledMessageState.TakeHeaders();
                if (this.headers == null)
                {
                    this.headers = new MessageHeaders(messageVersion);
                }
                this.headers.CopyHeadersFrom(headers);
                this.properties = properties;
                this.messageVersion = messageVersion;
            }

            public override void Close()
            {
                lock (this.thisLock)
                {
                    if (!this.closed)
                    {
                        this.closed = true;
                        this.recycledMessageState.ReturnHeaders(this.headers);
                        this.messageDataAtBody.ReturnMessageState(this.recycledMessageState);
                        this.messageDataAtBody.Close();
                        this.recycledMessageState = null;
                        this.messageDataAtBody = null;
                        this.properties = null;
                        this.messageVersion = null;
                        this.headers = null;
                    }
                }
            }

            public override Message CreateMessage()
            {
                lock (this.ThisLock)
                {
                    if (this.closed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
                    }
                    return new MessagePatterns.PatternMessage(this.messageDataAtBody, this.messageVersion, this.properties, this.headers);
                }
            }

            public override int BufferSize
            {
                get
                {
                    lock (this.ThisLock)
                    {
                        if (this.closed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
                        }
                        return this.messageDataAtBody.Buffer.Count;
                    }
                }
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }
    }
}

