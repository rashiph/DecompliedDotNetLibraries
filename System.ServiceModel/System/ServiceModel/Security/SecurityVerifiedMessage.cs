namespace System.ServiceModel.Security
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class SecurityVerifiedMessage : DelegatingMessage
    {
        private XmlAttributeHolder[] bodyAttributes;
        private bool bodyDecrypted;
        private string bodyPrefix;
        private XmlDictionaryReader cachedDecryptedBodyContentReader;
        private XmlDictionaryReader cachedReaderAtSecurityHeader;
        private bool canDelegateCreateBufferedCopyToInnerMessage;
        private byte[] decryptedBuffer;
        private XmlAttributeHolder[] envelopeAttributes;
        private string envelopePrefix;
        private XmlAttributeHolder[] headerAttributes;
        private bool isDecryptedBodyEmpty;
        private bool isDecryptedBodyFault;
        private bool isDecryptedBodyStatusDetermined;
        private XmlBuffer messageBuffer;
        private readonly ReceiveSecurityHeader securityHeader;
        private BodyState state;

        public SecurityVerifiedMessage(Message messageToProcess, ReceiveSecurityHeader securityHeader) : base(messageToProcess)
        {
            this.securityHeader = securityHeader;
            if (securityHeader.RequireMessageProtection)
            {
                XmlDictionaryReader messageReader;
                BufferedMessage innerMessage = base.InnerMessage as BufferedMessage;
                if ((innerMessage != null) && this.Headers.ContainsOnlyBufferedMessageHeaders)
                {
                    messageReader = innerMessage.GetMessageReader();
                }
                else
                {
                    this.messageBuffer = new XmlBuffer(0x7fffffff);
                    XmlDictionaryWriter writer = this.messageBuffer.OpenSection(this.securityHeader.ReaderQuotas);
                    base.InnerMessage.WriteMessage(writer);
                    this.messageBuffer.CloseSection();
                    this.messageBuffer.Close();
                    messageReader = this.messageBuffer.GetReader(0);
                }
                this.MoveToSecurityHeader(messageReader, securityHeader.HeaderIndex, true);
                this.cachedReaderAtSecurityHeader = messageReader;
                this.state = BodyState.Buffered;
            }
            else
            {
                this.envelopeAttributes = XmlAttributeHolder.emptyArray;
                this.headerAttributes = XmlAttributeHolder.emptyArray;
                this.bodyAttributes = XmlAttributeHolder.emptyArray;
                this.canDelegateCreateBufferedCopyToInnerMessage = true;
            }
        }

        private Exception CreateBadStateException(string operation)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("MessageBodyOperationNotValidInBodyState", new object[] { operation, this.state }));
        }

        public XmlDictionaryReader CreateFullBodyReader()
        {
            switch (this.state)
            {
                case BodyState.Buffered:
                    return this.CreateFullBodyReaderFromBufferedState();

                case BodyState.Decrypted:
                    return this.CreateFullBodyReaderFromDecryptedState();
            }
            throw TraceUtility.ThrowHelperError(this.CreateBadStateException("CreateFullBodyReader"), this);
        }

        private XmlDictionaryReader CreateFullBodyReaderFromBufferedState()
        {
            if (this.messageBuffer != null)
            {
                XmlDictionaryReader reader = this.messageBuffer.GetReader(0);
                this.MoveToBody(reader);
                return reader;
            }
            return ((BufferedMessage) base.InnerMessage).GetBufferedReaderAtBody();
        }

        private XmlDictionaryReader CreateFullBodyReaderFromDecryptedState()
        {
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(this.decryptedBuffer, 0, this.decryptedBuffer.Length, this.securityHeader.ReaderQuotas);
            this.MoveToBody(reader);
            return reader;
        }

        private void EnsureDecryptedBodyStatusDetermined()
        {
            if (!this.isDecryptedBodyStatusDetermined)
            {
                XmlDictionaryReader reader = this.CreateFullBodyReader();
                if (Message.ReadStartBody(reader, base.InnerMessage.Version.Envelope, out this.isDecryptedBodyFault, out this.isDecryptedBodyEmpty))
                {
                    this.cachedDecryptedBodyContentReader = reader;
                }
                else
                {
                    reader.Close();
                }
                this.isDecryptedBodyStatusDetermined = true;
            }
        }

        public XmlAttributeHolder[] GetEnvelopeAttributes()
        {
            return this.envelopeAttributes;
        }

        public XmlAttributeHolder[] GetHeaderAttributes()
        {
            return this.headerAttributes;
        }

        private XmlDictionaryReader GetReaderAtEnvelope()
        {
            if (this.messageBuffer != null)
            {
                return this.messageBuffer.GetReader(0);
            }
            return ((BufferedMessage) base.InnerMessage).GetMessageReader();
        }

        public XmlDictionaryReader GetReaderAtFirstHeader()
        {
            XmlDictionaryReader readerAtEnvelope = this.GetReaderAtEnvelope();
            this.MoveToHeaderBlock(readerAtEnvelope, false);
            readerAtEnvelope.ReadStartElement();
            return readerAtEnvelope;
        }

        public XmlDictionaryReader GetReaderAtSecurityHeader()
        {
            if (this.cachedReaderAtSecurityHeader != null)
            {
                XmlDictionaryReader cachedReaderAtSecurityHeader = this.cachedReaderAtSecurityHeader;
                this.cachedReaderAtSecurityHeader = null;
                return cachedReaderAtSecurityHeader;
            }
            return this.Headers.GetReaderAtHeader(this.securityHeader.HeaderIndex);
        }

        private void MoveToBody(XmlDictionaryReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            reader.ReadStartElement();
            if (reader.IsStartElement(XD.MessageDictionary.Header, this.Version.Envelope.DictionaryNamespace))
            {
                reader.Skip();
            }
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
        }

        private void MoveToHeaderBlock(XmlDictionaryReader reader, bool captureAttributes)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            if (captureAttributes)
            {
                this.envelopePrefix = reader.Prefix;
                this.envelopeAttributes = XmlAttributeHolder.ReadAttributes(reader);
            }
            reader.ReadStartElement();
            reader.MoveToStartElement(XD.MessageDictionary.Header, this.Version.Envelope.DictionaryNamespace);
            if (captureAttributes)
            {
                this.headerAttributes = XmlAttributeHolder.ReadAttributes(reader);
            }
        }

        private void MoveToSecurityHeader(XmlDictionaryReader reader, int headerIndex, bool captureAttributes)
        {
            this.MoveToHeaderBlock(reader, captureAttributes);
            reader.ReadStartElement();
            while (true)
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.MoveToContent();
                }
                if (headerIndex == 0)
                {
                    return;
                }
                reader.Skip();
                headerIndex--;
            }
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if (this.state == BodyState.Created)
            {
                base.OnBodyToString(writer);
            }
            else
            {
                this.OnWriteBodyContents(writer);
            }
        }

        protected override void OnClose()
        {
            if (this.cachedDecryptedBodyContentReader != null)
            {
                try
                {
                    this.cachedDecryptedBodyContentReader.Close();
                }
                catch (IOException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
                finally
                {
                    this.cachedDecryptedBodyContentReader = null;
                }
            }
            if (this.cachedReaderAtSecurityHeader != null)
            {
                try
                {
                    this.cachedReaderAtSecurityHeader.Close();
                }
                catch (IOException exception2)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                }
                finally
                {
                    this.cachedReaderAtSecurityHeader = null;
                }
            }
            this.messageBuffer = null;
            this.decryptedBuffer = null;
            this.state = BodyState.Disposed;
            base.InnerMessage.Close();
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            if (this.canDelegateCreateBufferedCopyToInnerMessage && (base.InnerMessage is BufferedMessage))
            {
                return base.InnerMessage.CreateBufferedCopy(maxBufferSize);
            }
            return base.OnCreateBufferedCopy(maxBufferSize);
        }

        protected override XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            if (this.state == BodyState.Created)
            {
                return base.InnerMessage.GetReaderAtBodyContents();
            }
            if (this.bodyDecrypted)
            {
                this.EnsureDecryptedBodyStatusDetermined();
            }
            if (this.cachedDecryptedBodyContentReader != null)
            {
                XmlDictionaryReader cachedDecryptedBodyContentReader = this.cachedDecryptedBodyContentReader;
                this.cachedDecryptedBodyContentReader = null;
                return cachedDecryptedBodyContentReader;
            }
            XmlDictionaryReader reader2 = this.CreateFullBodyReader();
            reader2.ReadStartElement();
            reader2.MoveToContent();
            return reader2;
        }

        internal void OnMessageProtectionPassComplete(bool atLeastOneHeaderOrBodyEncrypted)
        {
            this.canDelegateCreateBufferedCopyToInnerMessage = !atLeastOneHeaderOrBodyEncrypted;
        }

        internal void OnUnencryptedPart(string name, string ns)
        {
            if (ns == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredMessagePartNotEncrypted", new object[] { name })), this);
            }
            throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredMessagePartNotEncryptedNs", new object[] { name, ns })), this);
        }

        internal void OnUnsignedPart(string name, string ns)
        {
            if (ns == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredMessagePartNotSigned", new object[] { name })), this);
            }
            throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredMessagePartNotSignedNs", new object[] { name, ns })), this);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (this.state == BodyState.Created)
            {
                base.InnerMessage.WriteBodyContents(writer);
            }
            else
            {
                XmlDictionaryReader reader = this.CreateFullBodyReader();
                reader.ReadStartElement();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    writer.WriteNode(reader, false);
                }
                reader.ReadEndElement();
                reader.Close();
            }
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            if (this.state == BodyState.Created)
            {
                base.InnerMessage.WriteStartBody(writer);
            }
            else
            {
                XmlDictionaryReader reader = this.CreateFullBodyReader();
                reader.MoveToContent();
                writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                writer.WriteAttributes(reader, false);
                reader.Close();
            }
        }

        public void SetBodyPrefixAndAttributes(XmlDictionaryReader bodyReader)
        {
            this.bodyPrefix = bodyReader.Prefix;
            this.bodyAttributes = XmlAttributeHolder.ReadAttributes(bodyReader);
        }

        public void SetDecryptedBody(byte[] decryptedBodyContent)
        {
            if (this.state != BodyState.Buffered)
            {
                throw TraceUtility.ThrowHelperError(this.CreateBadStateException("SetDecryptedBody"), this);
            }
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            writer.WriteStartElement(this.envelopePrefix, XD.MessageDictionary.Envelope, this.Version.Envelope.DictionaryNamespace);
            XmlAttributeHolder.WriteAttributes(this.envelopeAttributes, writer);
            writer.WriteStartElement(this.bodyPrefix, XD.MessageDictionary.Body, this.Version.Envelope.DictionaryNamespace);
            XmlAttributeHolder.WriteAttributes(this.bodyAttributes, writer);
            writer.WriteString(" ");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            this.decryptedBuffer = ContextImportHelper.SpliceBuffers(decryptedBodyContent, stream.GetBuffer(), (int) stream.Length, 2);
            this.bodyDecrypted = true;
            this.state = BodyState.Decrypted;
        }

        public override bool IsEmpty
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                if (!this.bodyDecrypted)
                {
                    return base.InnerMessage.IsEmpty;
                }
                this.EnsureDecryptedBodyStatusDetermined();
                return this.isDecryptedBodyEmpty;
            }
        }

        public override bool IsFault
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                if (!this.bodyDecrypted)
                {
                    return base.InnerMessage.IsFault;
                }
                this.EnsureDecryptedBodyStatusDetermined();
                return this.isDecryptedBodyFault;
            }
        }

        internal byte[] PrimarySignatureValue
        {
            get
            {
                return this.securityHeader.PrimarySignatureValue;
            }
        }

        internal ReceiveSecurityHeader ReceivedSecurityHeader
        {
            get
            {
                return this.securityHeader;
            }
        }

        private enum BodyState
        {
            Created,
            Buffered,
            Decrypted,
            Disposed
        }
    }
}

