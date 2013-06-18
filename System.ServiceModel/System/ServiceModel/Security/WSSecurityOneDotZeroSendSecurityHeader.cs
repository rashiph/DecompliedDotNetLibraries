namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class WSSecurityOneDotZeroSendSecurityHeader : SendSecurityHeader
    {
        private MessagePartSpecification effectiveSignatureParts;
        private SymmetricAlgorithm encryptingSymmetricAlgorithm;
        private SecurityKeyIdentifier encryptionKeyIdentifier;
        private HashStream hashStream;
        private bool hasSignedEncryptedMessagePart;
        private ReferenceList referenceList;
        private SecurityKey signatureKey;
        private PreDigestedSignedInfo signedInfo;
        private SignedXml signedXml;
        private byte[] toHeaderHash;
        private string toHeaderId;

        public WSSecurityOneDotZeroSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction) : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction)
        {
        }

        private void AddEncryptionReference(MessageHeader header, string headerId, IPrefixGenerator prefixGenerator, bool sign, out MemoryStream plainTextStream, out string encryptedDataId)
        {
            plainTextStream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(plainTextStream);
            if (sign)
            {
                this.AddSignatureReference(header, headerId, prefixGenerator, writer);
            }
            else
            {
                header.WriteHeader(writer, base.Version);
                writer.Flush();
            }
            encryptedDataId = base.GenerateId();
            this.referenceList.AddReferredId(encryptedDataId);
        }

        private void AddSignatureReference(SecurityToken token)
        {
            if (token.Id == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("ElementToSignMustHaveId")), base.Message);
            }
            HashStream stream = this.TakeHashStream();
            XmlDictionaryWriter writer = this.TakeUtf8Writer();
            writer.StartCanonicalization(stream, false, null);
            base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, token);
            writer.EndCanonicalization();
            this.signedInfo.AddReference(token.Id, stream.FlushHashAndGetValue());
        }

        private void AddSignatureReference(SecurityToken[] tokens)
        {
            if (tokens != null)
            {
                for (int i = 0; i < tokens.Length; i++)
                {
                    this.AddSignatureReference(tokens[i]);
                }
            }
        }

        private void AddSignatureReference(SendSecurityHeaderElement[] elements)
        {
            if (elements != null)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i].Id == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("ElementToSignMustHaveId")), base.Message);
                    }
                    HashStream stream = this.TakeHashStream();
                    XmlDictionaryWriter writer = this.TakeUtf8Writer();
                    writer.StartCanonicalization(stream, false, null);
                    elements[i].Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                    writer.EndCanonicalization();
                    this.signedInfo.AddReference(elements[i].Id, stream.FlushHashAndGetValue());
                }
            }
        }

        private void AddSignatureReference(MessageHeader header, string headerId, IPrefixGenerator prefixGenerator, XmlDictionaryWriter writer)
        {
            byte[] buffer;
            headerId = this.GetSignatureHash(header, headerId, prefixGenerator, writer, out buffer);
            this.signedInfo.AddReference(headerId, buffer);
        }

        public override void ApplyBodySecurity(XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator)
        {
            EncryptedData data;
            HashStream stream;
            SecurityAppliedMessage securityAppliedMessage = base.SecurityAppliedMessage;
            switch (securityAppliedMessage.BodyProtectionMode)
            {
                case MessagePartProtectionMode.Sign:
                    stream = this.TakeHashStream();
                    if (!CanCanonicalizeAndFragment(writer))
                    {
                        securityAppliedMessage.WriteBodyToSign(stream);
                    }
                    else
                    {
                        securityAppliedMessage.WriteBodyToSignWithFragments(stream, false, null, writer);
                    }
                    this.signedInfo.AddReference(securityAppliedMessage.BodyId, stream.FlushHashAndGetValue());
                    return;

                case MessagePartProtectionMode.Encrypt:
                    data = this.CreateEncryptedDataForBody();
                    securityAppliedMessage.WriteBodyToEncrypt(data, this.encryptingSymmetricAlgorithm);
                    this.referenceList.AddReferredId(data.Id);
                    return;

                case MessagePartProtectionMode.SignThenEncrypt:
                    stream = this.TakeHashStream();
                    data = this.CreateEncryptedDataForBody();
                    if (!CanCanonicalizeAndFragment(writer))
                    {
                        securityAppliedMessage.WriteBodyToSignThenEncrypt(stream, data, this.encryptingSymmetricAlgorithm);
                    }
                    else
                    {
                        securityAppliedMessage.WriteBodyToSignThenEncryptWithFragments(stream, false, null, data, this.encryptingSymmetricAlgorithm, writer);
                    }
                    this.signedInfo.AddReference(securityAppliedMessage.BodyId, stream.FlushHashAndGetValue());
                    this.referenceList.AddReferredId(data.Id);
                    this.hasSignedEncryptedMessagePart = true;
                    return;

                case MessagePartProtectionMode.EncryptThenSign:
                    stream = this.TakeHashStream();
                    data = this.CreateEncryptedDataForBody();
                    securityAppliedMessage.WriteBodyToEncryptThenSign(stream, data, this.encryptingSymmetricAlgorithm);
                    this.signedInfo.AddReference(securityAppliedMessage.BodyId, stream.FlushHashAndGetValue());
                    this.referenceList.AddReferredId(data.Id);
                    return;
            }
        }

        private void ApplySecurityAndWriteHeader(MessageHeader header, string headerId, XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator)
        {
            if ((!base.RequireMessageProtection && base.ShouldSignToHeader) && ((header.Name == System.ServiceModel.XD.AddressingDictionary.To.Value) && (header.Namespace == base.Message.Version.Addressing.Namespace)))
            {
                byte[] buffer;
                if (this.toHeaderHash != null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TransportSecuredMessageHasMoreThanOneToHeader")));
                }
                headerId = this.GetSignatureHash(header, headerId, prefixGenerator, writer, out buffer);
                this.toHeaderHash = buffer;
                this.toHeaderId = headerId;
            }
            else
            {
                MemoryStream stream;
                string str;
                switch (this.GetProtectionMode(header))
                {
                    case MessagePartProtectionMode.None:
                        header.WriteHeader(writer, base.Version);
                        return;

                    case MessagePartProtectionMode.Sign:
                        this.AddSignatureReference(header, headerId, prefixGenerator, writer);
                        return;

                    case MessagePartProtectionMode.Encrypt:
                        this.AddEncryptionReference(header, headerId, prefixGenerator, false, out stream, out str);
                        this.EncryptAndWriteHeader(header, str, stream, writer);
                        return;

                    case MessagePartProtectionMode.SignThenEncrypt:
                        this.AddEncryptionReference(header, headerId, prefixGenerator, true, out stream, out str);
                        this.EncryptAndWriteHeader(header, str, stream, writer);
                        this.hasSignedEncryptedMessagePart = true;
                        return;

                    case MessagePartProtectionMode.EncryptThenSign:
                    {
                        this.AddEncryptionReference(header, headerId, prefixGenerator, false, out stream, out str);
                        EncryptedHeader header2 = this.EncryptHeader(header, this.encryptingSymmetricAlgorithm, this.encryptionKeyIdentifier, base.Version, str, stream);
                        this.AddSignatureReference(header2, str, prefixGenerator, writer);
                        return;
                    }
                }
            }
        }

        public override void ApplySecurityAndWriteHeaders(MessageHeaders headers, XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator)
        {
            string[] headerAttributes;
            if (base.RequireMessageProtection || base.ShouldSignToHeader)
            {
                headerAttributes = headers.GetHeaderAttributes("Id", base.StandardsManager.IdManager.DefaultIdNamespaceUri);
            }
            else
            {
                headerAttributes = null;
            }
            for (int i = 0; i < headers.Count; i++)
            {
                MessageHeader messageHeader = headers.GetMessageHeader(i);
                if (((base.Version.Addressing != AddressingVersion.None) || !(messageHeader.Namespace == AddressingVersion.None.Namespace)) && (messageHeader != this))
                {
                    this.ApplySecurityAndWriteHeader(messageHeader, (headerAttributes == null) ? null : headerAttributes[i], writer, prefixGenerator);
                }
            }
        }

        private static bool CanCanonicalizeAndFragment(XmlDictionaryWriter writer)
        {
            if (!writer.CanCanonicalize)
            {
                return false;
            }
            IFragmentCapableXmlDictionaryWriter writer2 = writer as IFragmentCapableXmlDictionaryWriter;
            return ((writer2 != null) && writer2.CanFragment);
        }

        protected static MemoryStream CaptureSecurityElement(ISecurityElement element)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            element.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            writer.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            return stream;
        }

        protected static MemoryStream CaptureToken(SecurityToken token, SecurityStandardsManager serializer)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            serializer.SecurityTokenSerializer.WriteToken(writer, token);
            writer.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            return stream;
        }

        protected override ISecurityElement CompleteEncryptionCore(SendSecurityHeaderElement primarySignature, SendSecurityHeaderElement[] basicTokens, SendSecurityHeaderElement[] signatureConfirmations, SendSecurityHeaderElement[] endorsingSignatures)
        {
            ISecurityElement element;
            if (this.referenceList == null)
            {
                return null;
            }
            if (((primarySignature != null) && (primarySignature.Item != null)) && primarySignature.MarkedForEncryption)
            {
                this.EncryptElement(primarySignature);
            }
            if (basicTokens != null)
            {
                for (int i = 0; i < basicTokens.Length; i++)
                {
                    if (basicTokens[i].MarkedForEncryption)
                    {
                        this.EncryptElement(basicTokens[i]);
                    }
                }
            }
            if (signatureConfirmations != null)
            {
                for (int j = 0; j < signatureConfirmations.Length; j++)
                {
                    if (signatureConfirmations[j].MarkedForEncryption)
                    {
                        this.EncryptElement(signatureConfirmations[j]);
                    }
                }
            }
            if (endorsingSignatures != null)
            {
                for (int k = 0; k < endorsingSignatures.Length; k++)
                {
                    if (endorsingSignatures[k].MarkedForEncryption)
                    {
                        this.EncryptElement(endorsingSignatures[k]);
                    }
                }
            }
            try
            {
                element = (this.referenceList.DataReferenceCount > 0) ? this.referenceList : null;
            }
            finally
            {
                this.referenceList = null;
                this.encryptingSymmetricAlgorithm = null;
                this.encryptionKeyIdentifier = null;
            }
            return element;
        }

        protected override ISignatureValueSecurityElement CompletePrimarySignatureCore(SendSecurityHeaderElement[] signatureConfirmations, SecurityToken[] signedEndorsingTokens, SecurityToken[] signedTokens, SendSecurityHeaderElement[] basicTokens)
        {
            ISignatureValueSecurityElement signedXml;
            if (this.signedXml == null)
            {
                return null;
            }
            SecurityTimestamp timestamp = base.Timestamp;
            if (timestamp != null)
            {
                if (timestamp.Id == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TimestampToSignHasNoId")));
                }
                HashStream stream = this.TakeHashStream();
                base.StandardsManager.WSUtilitySpecificationVersion.WriteTimestampCanonicalForm(stream, timestamp, this.signedInfo.ResourcePool.TakeEncodingBuffer());
                this.signedInfo.AddReference(timestamp.Id, stream.FlushHashAndGetValue());
            }
            if ((base.ShouldSignToHeader && (this.signatureKey is AsymmetricSecurityKey)) && (base.Version.Addressing != AddressingVersion.None))
            {
                if (this.toHeaderHash == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransportSecurityRequireToHeader")));
                }
                this.signedInfo.AddReference(this.toHeaderId, this.toHeaderHash);
            }
            this.AddSignatureReference(signatureConfirmations);
            if (base.RequireMessageProtection)
            {
                this.AddSignatureReference(signedEndorsingTokens);
                this.AddSignatureReference(signedTokens);
                this.AddSignatureReference(basicTokens);
            }
            if (this.signedInfo.ReferenceCount == 0)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoPartsOfMessageMatchedPartsToSign")), base.Message);
            }
            try
            {
                this.signedXml.ComputeSignature(this.signatureKey);
                signedXml = this.signedXml;
            }
            finally
            {
                this.hashStream = null;
                this.signedInfo = null;
                this.signedXml = null;
                this.signatureKey = null;
                this.effectiveSignatureParts = null;
            }
            return signedXml;
        }

        private EncryptedData CreateEncryptedData()
        {
            return new EncryptedData { SecurityTokenSerializer = base.StandardsManager.SecurityTokenSerializer, KeyIdentifier = this.encryptionKeyIdentifier, EncryptionMethod = this.EncryptionAlgorithm, EncryptionMethodDictionaryString = this.EncryptionAlgorithmDictionaryString };
        }

        private EncryptedData CreateEncryptedData(MemoryStream stream, string id, bool typeElement)
        {
            EncryptedData data = this.CreateEncryptedData();
            data.Id = id;
            data.SetUpEncryption(this.encryptingSymmetricAlgorithm, new ArraySegment<byte>(stream.GetBuffer(), 0, (int) stream.Length));
            if (typeElement)
            {
                data.Type = EncryptedData.ElementType;
            }
            return data;
        }

        private EncryptedData CreateEncryptedDataForBody()
        {
            EncryptedData data = this.CreateEncryptedData();
            data.Type = EncryptedData.ContentType;
            return data;
        }

        protected override ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier)
        {
            this.StartPrimarySignatureCore(token, identifier, MessagePartSpecification.NoParts, false);
            return this.CompletePrimarySignatureCore(null, null, null, null);
        }

        protected override ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier, ISecurityElement elementToSign)
        {
            string str;
            XmlDictionaryString str2;
            SecurityKey key;
            SecurityAlgorithmSuite algorithmSuite = base.AlgorithmSuite;
            algorithmSuite.GetSignatureAlgorithmAndKey(token, out str, out key, out str2);
            SignedXml xml = new SignedXml(ServiceModelDictionaryManager.Instance, base.StandardsManager.SecurityTokenSerializer);
            SignedInfo signedInfo = xml.Signature.SignedInfo;
            signedInfo.CanonicalizationMethod = algorithmSuite.DefaultCanonicalizationAlgorithm;
            signedInfo.CanonicalizationMethodDictionaryString = algorithmSuite.DefaultCanonicalizationAlgorithmDictionaryString;
            signedInfo.SignatureMethod = str;
            signedInfo.SignatureMethodDictionaryString = str2;
            if (elementToSign.Id == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ElementToSignMustHaveId")));
            }
            Reference reference = new Reference(ServiceModelDictionaryManager.Instance, "#" + elementToSign.Id, elementToSign) {
                DigestMethod = algorithmSuite.DefaultDigestAlgorithm,
                DigestMethodDictionaryString = algorithmSuite.DefaultDigestAlgorithmDictionaryString
            };
            reference.AddTransform(new ExclusiveCanonicalizationTransform());
            ((StandardSignedInfo) signedInfo).AddReference(reference);
            xml.ComputeSignature(key);
            if (identifier != null)
            {
                xml.Signature.KeyIdentifier = identifier;
            }
            return xml;
        }

        private void EncryptAndWriteHeader(MessageHeader plainTextHeader, string id, MemoryStream stream, XmlDictionaryWriter writer)
        {
            this.EncryptHeader(plainTextHeader, this.encryptingSymmetricAlgorithm, this.encryptionKeyIdentifier, base.Version, id, stream).WriteHeader(writer, base.Version);
        }

        private void EncryptElement(SendSecurityHeaderElement element)
        {
            string id = base.GenerateId();
            ISecurityElement item = this.CreateEncryptedData(CaptureSecurityElement(element.Item), id, true);
            this.referenceList.AddReferredId(id);
            element.Replace(id, item);
        }

        protected virtual EncryptedHeader EncryptHeader(MessageHeader plainTextHeader, SymmetricAlgorithm algorithm, SecurityKeyIdentifier keyIdentifier, MessageVersion version, string id, MemoryStream stream)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("HeaderEncryptionNotSupportedInWsSecurityJan2004", new object[] { plainTextHeader.Name, plainTextHeader.Namespace })));
        }

        private MessagePartProtectionMode GetProtectionMode(MessageHeader header)
        {
            if (!base.RequireMessageProtection)
            {
                return MessagePartProtectionMode.None;
            }
            bool sign = (this.signedInfo != null) && this.effectiveSignatureParts.IsHeaderIncluded(header);
            bool encrypt = (this.referenceList != null) && base.EncryptionParts.IsHeaderIncluded(header);
            return MessagePartProtectionModeHelper.GetProtectionMode(sign, encrypt, base.SignThenEncrypt);
        }

        private string GetSignatureHash(MessageHeader header, string headerId, IPrefixGenerator prefixGenerator, XmlDictionaryWriter writer, out byte[] hash)
        {
            XmlDictionaryWriter writer2;
            HashStream stream = this.TakeHashStream();
            XmlBuffer buffer = null;
            if (writer.CanCanonicalize)
            {
                writer2 = writer;
            }
            else
            {
                buffer = new XmlBuffer(0x7fffffff);
                writer2 = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            }
            writer2.StartCanonicalization(stream, false, null);
            header.WriteStartHeader(writer2, base.Version);
            if (headerId == null)
            {
                headerId = base.GenerateId();
                base.StandardsManager.IdManager.WriteIdAttribute(writer2, headerId);
            }
            header.WriteHeaderContents(writer2, base.Version);
            writer2.WriteEndElement();
            writer2.EndCanonicalization();
            writer2.Flush();
            if (!object.ReferenceEquals(writer2, writer))
            {
                buffer.CloseSection();
                buffer.Close();
                XmlDictionaryReader reader = buffer.GetReader(0);
                writer.WriteNode(reader, false);
                reader.Close();
            }
            hash = stream.FlushHashAndGetValue();
            return headerId;
        }

        protected override void StartEncryptionCore(SecurityToken token, SecurityKeyIdentifier keyIdentifier)
        {
            this.encryptingSymmetricAlgorithm = System.ServiceModel.Security.SecurityUtils.GetSymmetricAlgorithm(this.EncryptionAlgorithm, token);
            if (this.encryptingSymmetricAlgorithm == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToCreateSymmetricAlgorithmFromToken", new object[] { this.EncryptionAlgorithm })));
            }
            this.encryptionKeyIdentifier = keyIdentifier;
            this.referenceList = new ReferenceList();
        }

        protected override void StartPrimarySignatureCore(SecurityToken token, SecurityKeyIdentifier keyIdentifier, MessagePartSpecification signatureParts, bool generateTargettableSignature)
        {
            string str3;
            XmlDictionaryString str4;
            SecurityAlgorithmSuite algorithmSuite = base.AlgorithmSuite;
            string defaultCanonicalizationAlgorithm = algorithmSuite.DefaultCanonicalizationAlgorithm;
            XmlDictionaryString defaultCanonicalizationAlgorithmDictionaryString = algorithmSuite.DefaultCanonicalizationAlgorithmDictionaryString;
            if (defaultCanonicalizationAlgorithm != "http://www.w3.org/2001/10/xml-exc-c14n#")
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnsupportedCanonicalizationAlgorithm", new object[] { algorithmSuite.DefaultCanonicalizationAlgorithm })));
            }
            algorithmSuite.GetSignatureAlgorithmAndKey(token, out str3, out this.signatureKey, out str4);
            string defaultDigestAlgorithm = algorithmSuite.DefaultDigestAlgorithm;
            XmlDictionaryString defaultDigestAlgorithmDictionaryString = algorithmSuite.DefaultDigestAlgorithmDictionaryString;
            this.signedInfo = new PreDigestedSignedInfo(ServiceModelDictionaryManager.Instance, defaultCanonicalizationAlgorithm, defaultCanonicalizationAlgorithmDictionaryString, defaultDigestAlgorithm, defaultDigestAlgorithmDictionaryString, str3, str4);
            this.signedXml = new SignedXml(this.signedInfo, ServiceModelDictionaryManager.Instance, base.StandardsManager.SecurityTokenSerializer);
            if (keyIdentifier != null)
            {
                this.signedXml.Signature.KeyIdentifier = keyIdentifier;
            }
            if (generateTargettableSignature)
            {
                this.signedXml.Id = base.GenerateId();
            }
            this.effectiveSignatureParts = signatureParts;
            this.hashStream = this.signedInfo.ResourcePool.TakeHashStream(defaultDigestAlgorithm);
        }

        private HashStream TakeHashStream()
        {
            HashStream hashStream = null;
            if (this.hashStream == null)
            {
                this.hashStream = hashStream = new HashStream(System.ServiceModel.Security.CryptoHelper.CreateHashAlgorithm(base.AlgorithmSuite.DefaultDigestAlgorithm));
                return hashStream;
            }
            hashStream = this.hashStream;
            hashStream.Reset();
            return hashStream;
        }

        private XmlDictionaryWriter TakeUtf8Writer()
        {
            return this.signedInfo.ResourcePool.TakeUtf8Writer();
        }

        protected string EncryptionAlgorithm
        {
            get
            {
                return base.AlgorithmSuite.DefaultEncryptionAlgorithm;
            }
        }

        protected XmlDictionaryString EncryptionAlgorithmDictionaryString
        {
            get
            {
                return base.AlgorithmSuite.DefaultEncryptionAlgorithmDictionaryString;
            }
        }

        protected override bool HasSignedEncryptedMessagePart
        {
            get
            {
                return this.hasSignedEncryptedMessagePart;
            }
        }
    }
}

