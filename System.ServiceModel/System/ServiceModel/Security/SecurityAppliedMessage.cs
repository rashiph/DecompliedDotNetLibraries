namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal sealed class SecurityAppliedMessage : DelegatingMessage
    {
        private System.IdentityModel.XmlAttributeHolder[] bodyAttributes;
        private string bodyId;
        private bool bodyIdInserted;
        private string bodyPrefix;
        private readonly MessagePartProtectionMode bodyProtectionMode;
        private bool delayedApplicationHandled;
        private ISecurityElement encryptedBodyContent;
        private MemoryStream endBodyFragment;
        private XmlBuffer fullBodyBuffer;
        private byte[] fullBodyFragment;
        private int fullBodyFragmentLength;
        private readonly SendSecurityHeader securityHeader;
        private MemoryStream startBodyFragment;
        private BodyState state;

        public SecurityAppliedMessage(Message messageToProcess, SendSecurityHeader securityHeader, bool signBody, bool encryptBody) : base(messageToProcess)
        {
            this.bodyPrefix = "s";
            this.securityHeader = securityHeader;
            this.bodyProtectionMode = MessagePartProtectionModeHelper.GetProtectionMode(signBody, encryptBody, securityHeader.SignThenEncrypt);
        }

        private void AttachChannelBindingTokenIfFound()
        {
            ChannelBindingMessageProperty property = null;
            ChannelBindingMessageProperty.TryGet(base.InnerMessage, out property);
            if (((property != null) && (this.securityHeader.ElementContainer != null)) && (this.securityHeader.ElementContainer.EndorsingSupportingTokens != null))
            {
                foreach (SecurityToken token in this.securityHeader.ElementContainer.EndorsingSupportingTokens)
                {
                    ProviderBackedSecurityToken token2 = token as ProviderBackedSecurityToken;
                    if (token2 != null)
                    {
                        token2.ChannelBinding = property.ChannelBinding;
                    }
                }
            }
        }

        private Exception CreateBadStateException(string operation)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("MessageBodyOperationNotValidInBodyState", new object[] { operation, this.state }));
        }

        private void EnsureUniqueSecurityApplication()
        {
            if (this.delayedApplicationHandled)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("DelayedSecurityApplicationAlreadyCompleted")));
            }
            this.delayedApplicationHandled = true;
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if ((this.state == BodyState.Created) || (this.fullBodyFragment != null))
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
            try
            {
                base.InnerMessage.Close();
            }
            finally
            {
                this.fullBodyBuffer = null;
                this.bodyAttributes = null;
                this.encryptedBodyContent = null;
                this.state = BodyState.Disposed;
            }
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            switch (this.state)
            {
                case BodyState.Created:
                    base.InnerMessage.WriteBodyContents(writer);
                    return;

                case BodyState.Signed:
                case BodyState.EncryptedThenSigned:
                {
                    XmlDictionaryReader reader = this.fullBodyBuffer.GetReader(0);
                    reader.ReadStartElement();
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        writer.WriteNode(reader, false);
                    }
                    reader.ReadEndElement();
                    reader.Close();
                    return;
                }
                case BodyState.SignedThenEncrypted:
                case BodyState.Encrypted:
                    this.encryptedBodyContent.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                    return;
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateBadStateException("OnWriteBodyContents"));
        }

        protected override void OnWriteMessage(XmlDictionaryWriter writer)
        {
            this.AttachChannelBindingTokenIfFound();
            this.EnsureUniqueSecurityApplication();
            MessagePrefixGenerator prefixGenerator = new MessagePrefixGenerator(writer);
            this.securityHeader.StartSecurityApplication();
            this.Headers.Add(this.securityHeader);
            base.InnerMessage.WriteStartEnvelope(writer);
            this.Headers.RemoveAt(this.Headers.Count - 1);
            this.securityHeader.ApplyBodySecurity(writer, prefixGenerator);
            base.InnerMessage.WriteStartHeaders(writer);
            this.securityHeader.ApplySecurityAndWriteHeaders(this.Headers, writer, prefixGenerator);
            this.securityHeader.RemoveSignatureEncryptionIfAppropriate();
            this.securityHeader.CompleteSecurityApplication();
            this.securityHeader.WriteHeader(writer, this.Version);
            writer.WriteEndElement();
            if (this.fullBodyFragment != null)
            {
                ((IFragmentCapableXmlDictionaryWriter) writer).WriteFragment(this.fullBodyFragment, 0, this.fullBodyFragmentLength);
            }
            else
            {
                if (this.startBodyFragment != null)
                {
                    ((IFragmentCapableXmlDictionaryWriter) writer).WriteFragment(this.startBodyFragment.GetBuffer(), 0, (int) this.startBodyFragment.Length);
                }
                else
                {
                    this.OnWriteStartBody(writer);
                }
                this.OnWriteBodyContents(writer);
                if (this.endBodyFragment != null)
                {
                    ((IFragmentCapableXmlDictionaryWriter) writer).WriteFragment(this.endBodyFragment.GetBuffer(), 0, (int) this.endBodyFragment.Length);
                }
                else
                {
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            if ((this.startBodyFragment != null) || (this.fullBodyFragment != null))
            {
                this.WriteStartInnerMessageWithId(writer);
            }
            else
            {
                switch (this.state)
                {
                    case BodyState.Created:
                    case BodyState.Encrypted:
                        base.InnerMessage.WriteStartBody(writer);
                        return;

                    case BodyState.Signed:
                    case BodyState.EncryptedThenSigned:
                    {
                        XmlDictionaryReader reader = this.fullBodyBuffer.GetReader(0);
                        writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        writer.WriteAttributes(reader, false);
                        reader.Close();
                        return;
                    }
                    case BodyState.SignedThenEncrypted:
                        writer.WriteStartElement(this.bodyPrefix, System.ServiceModel.XD.MessageDictionary.Body, this.Version.Envelope.DictionaryNamespace);
                        if (this.bodyAttributes != null)
                        {
                            System.IdentityModel.XmlAttributeHolder.WriteAttributes(this.bodyAttributes, writer);
                        }
                        return;
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateBadStateException("OnWriteStartBody"));
            }
        }

        private void SetBodyId()
        {
            this.bodyId = base.InnerMessage.GetBodyAttribute("Id", this.securityHeader.StandardsManager.IdManager.DefaultIdNamespaceUri);
            if (this.bodyId == null)
            {
                this.bodyId = this.securityHeader.GenerateId();
                this.bodyIdInserted = true;
            }
        }

        public void WriteBodyToEncrypt(EncryptedData encryptedData, SymmetricAlgorithm algorithm)
        {
            encryptedData.Id = this.securityHeader.GenerateId();
            BodyContentHelper helper = new BodyContentHelper();
            XmlDictionaryWriter writer = helper.CreateWriter();
            base.InnerMessage.WriteBodyContents(writer);
            encryptedData.SetUpEncryption(algorithm, helper.ExtractResult());
            this.encryptedBodyContent = encryptedData;
            this.state = BodyState.Encrypted;
        }

        public void WriteBodyToEncryptThenSign(Stream canonicalStream, EncryptedData encryptedData, SymmetricAlgorithm algorithm)
        {
            encryptedData.Id = this.securityHeader.GenerateId();
            this.SetBodyId();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(Stream.Null);
            writer.WriteStartElement("a");
            MemoryStream stream = new MemoryStream();
            ((IFragmentCapableXmlDictionaryWriter) writer).StartFragment(stream, true);
            base.InnerMessage.WriteBodyContents(writer);
            ((IFragmentCapableXmlDictionaryWriter) writer).EndFragment();
            writer.WriteEndElement();
            stream.Flush();
            encryptedData.SetUpEncryption(algorithm, new ArraySegment<byte>(stream.GetBuffer(), 0, (int) stream.Length));
            this.fullBodyBuffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer2 = this.fullBodyBuffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            writer2.StartCanonicalization(canonicalStream, false, null);
            this.WriteStartInnerMessageWithId(writer2);
            encryptedData.WriteTo(writer2, ServiceModelDictionaryManager.Instance);
            writer2.WriteEndElement();
            writer2.EndCanonicalization();
            writer2.Flush();
            this.fullBodyBuffer.CloseSection();
            this.fullBodyBuffer.Close();
            this.state = BodyState.EncryptedThenSigned;
        }

        public void WriteBodyToSign(Stream canonicalStream)
        {
            this.SetBodyId();
            this.fullBodyBuffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = this.fullBodyBuffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            writer.StartCanonicalization(canonicalStream, false, null);
            this.WriteInnerMessageWithId(writer);
            writer.EndCanonicalization();
            writer.Flush();
            this.fullBodyBuffer.CloseSection();
            this.fullBodyBuffer.Close();
            this.state = BodyState.Signed;
        }

        public void WriteBodyToSignThenEncrypt(Stream canonicalStream, EncryptedData encryptedData, SymmetricAlgorithm algorithm)
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            this.WriteBodyToSignThenEncryptWithFragments(canonicalStream, false, null, encryptedData, algorithm, writer);
            ((IFragmentCapableXmlDictionaryWriter) writer).WriteFragment(this.startBodyFragment.GetBuffer(), 0, (int) this.startBodyFragment.Length);
            ((IFragmentCapableXmlDictionaryWriter) writer).WriteFragment(this.endBodyFragment.GetBuffer(), 0, (int) this.endBodyFragment.Length);
            buffer.CloseSection();
            buffer.Close();
            this.startBodyFragment = null;
            this.endBodyFragment = null;
            XmlDictionaryReader reader = buffer.GetReader(0);
            reader.MoveToContent();
            this.bodyPrefix = reader.Prefix;
            if (reader.HasAttributes)
            {
                this.bodyAttributes = System.IdentityModel.XmlAttributeHolder.ReadAttributes(reader);
            }
            reader.Close();
        }

        public void WriteBodyToSignThenEncryptWithFragments(Stream stream, bool includeComments, string[] inclusivePrefixes, EncryptedData encryptedData, SymmetricAlgorithm algorithm, XmlDictionaryWriter writer)
        {
            int num;
            IFragmentCapableXmlDictionaryWriter writer2 = (IFragmentCapableXmlDictionaryWriter) writer;
            this.SetBodyId();
            encryptedData.Id = this.securityHeader.GenerateId();
            this.startBodyFragment = new MemoryStream();
            BufferedOutputStream stream2 = new BufferManagerOutputStream("XmlBufferQuotaExceeded", 0x400, 0x7fffffff, BufferManager.CreateBufferManager(0L, 0x7fffffff));
            this.endBodyFragment = new MemoryStream();
            writer.StartCanonicalization(stream, includeComments, inclusivePrefixes);
            writer2.StartFragment(this.startBodyFragment, false);
            this.WriteStartInnerMessageWithId(writer);
            writer2.EndFragment();
            writer2.StartFragment(stream2, true);
            base.InnerMessage.WriteBodyContents(writer);
            writer2.EndFragment();
            writer2.StartFragment(this.endBodyFragment, false);
            writer.WriteEndElement();
            writer2.EndFragment();
            writer.EndCanonicalization();
            byte[] array = stream2.ToArray(out num);
            encryptedData.SetUpEncryption(algorithm, new ArraySegment<byte>(array, 0, num));
            this.encryptedBodyContent = encryptedData;
            this.state = BodyState.SignedThenEncrypted;
        }

        public void WriteBodyToSignWithFragments(Stream stream, bool includeComments, string[] inclusivePrefixes, XmlDictionaryWriter writer)
        {
            IFragmentCapableXmlDictionaryWriter writer2 = (IFragmentCapableXmlDictionaryWriter) writer;
            this.SetBodyId();
            BufferedOutputStream stream2 = new BufferManagerOutputStream("XmlBufferQuotaExceeded", 0x400, 0x7fffffff, BufferManager.CreateBufferManager(0L, 0x7fffffff));
            writer.StartCanonicalization(stream, includeComments, inclusivePrefixes);
            writer2.StartFragment(stream2, false);
            this.WriteStartInnerMessageWithId(writer);
            base.InnerMessage.WriteBodyContents(writer);
            writer.WriteEndElement();
            writer2.EndFragment();
            writer.EndCanonicalization();
            this.fullBodyFragment = stream2.ToArray(out this.fullBodyFragmentLength);
            this.state = BodyState.Signed;
        }

        private void WriteInnerMessageWithId(XmlDictionaryWriter writer)
        {
            this.WriteStartInnerMessageWithId(writer);
            base.InnerMessage.WriteBodyContents(writer);
            writer.WriteEndElement();
        }

        private void WriteStartInnerMessageWithId(XmlDictionaryWriter writer)
        {
            base.InnerMessage.WriteStartBody(writer);
            if (this.bodyIdInserted)
            {
                this.securityHeader.StandardsManager.IdManager.WriteIdAttribute(writer, this.bodyId);
            }
        }

        public string BodyId
        {
            get
            {
                return this.bodyId;
            }
        }

        public MessagePartProtectionMode BodyProtectionMode
        {
            get
            {
                return this.bodyProtectionMode;
            }
        }

        internal byte[] PrimarySignatureValue
        {
            get
            {
                return this.securityHeader.PrimarySignatureValue;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BodyContentHelper
        {
            private MemoryStream stream;
            private XmlDictionaryWriter writer;
            public XmlDictionaryWriter CreateWriter()
            {
                this.stream = new MemoryStream();
                this.writer = XmlDictionaryWriter.CreateTextWriter(this.stream);
                return this.writer;
            }

            public ArraySegment<byte> ExtractResult()
            {
                this.writer.Flush();
                return new ArraySegment<byte>(this.stream.GetBuffer(), 0, (int) this.stream.Length);
            }
        }

        private enum BodyState
        {
            Created,
            Signed,
            SignedThenEncrypted,
            EncryptedThenSigned,
            Encrypted,
            Disposed
        }

        private sealed class MessagePrefixGenerator : IPrefixGenerator
        {
            private XmlWriter writer;

            public MessagePrefixGenerator(XmlWriter writer)
            {
                this.writer = writer;
            }

            public string GetPrefix(string namespaceUri, int depth, bool isForAttribute)
            {
                return this.writer.LookupPrefix(namespaceUri);
            }
        }
    }
}

