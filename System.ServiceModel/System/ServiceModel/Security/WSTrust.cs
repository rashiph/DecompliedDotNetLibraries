namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    internal abstract class WSTrust : System.ServiceModel.Security.WSSecurityTokenSerializer.SerializerEntries
    {
        private System.ServiceModel.Security.WSSecurityTokenSerializer tokenSerializer;

        public WSTrust(System.ServiceModel.Security.WSSecurityTokenSerializer tokenSerializer)
        {
            this.tokenSerializer = tokenSerializer;
        }

        protected static bool CheckElement(XmlElement element, string name, string ns, out string value)
        {
            value = null;
            if (((element.LocalName == name) && (element.NamespaceURI == ns)) && (element.FirstChild is XmlText))
            {
                value = ((XmlText) element.FirstChild).Value;
                return true;
            }
            return false;
        }

        public override void PopulateKeyIdentifierClauseEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.KeyIdentifierClauseEntry> keyIdentifierClauseEntries)
        {
            keyIdentifierClauseEntries.Add(new BinarySecretClauseEntry(this));
        }

        public override void PopulateTokenEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            tokenEntryList.Add(new BinarySecretTokenEntry(this));
        }

        public abstract TrustDictionary SerializerDictionary { get; }

        public System.ServiceModel.Security.WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get
            {
                return this.tokenSerializer;
            }
        }

        internal class BinarySecretClauseEntry : WSSecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            private TrustDictionary otherDictionary;
            private WSTrust parent;

            public BinarySecretClauseEntry(WSTrust parent)
            {
                this.parent = parent;
                this.otherDictionary = null;
                if (parent.SerializerDictionary is TrustDec2005Dictionary)
                {
                    this.otherDictionary = XD.TrustFeb2005Dictionary;
                }
                if (parent.SerializerDictionary is TrustFeb2005Dictionary)
                {
                    this.otherDictionary = DXD.TrustDec2005Dictionary;
                }
                if (this.otherDictionary == null)
                {
                    this.otherDictionary = this.parent.SerializerDictionary;
                }
            }

            public override bool CanReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                if (!reader.IsStartElement(this.LocalName, this.NamespaceUri))
                {
                    return reader.IsStartElement(this.LocalName, this.otherDictionary.Namespace);
                }
                return true;
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                return new BinarySecretKeyIdentifierClause(reader.ReadElementContentAsBase64(), false);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return (keyIdentifierClause is BinarySecretKeyIdentifierClause);
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                byte[] keyBytes = (keyIdentifierClause as BinarySecretKeyIdentifierClause).GetKeyBytes();
                writer.WriteStartElement(this.parent.SerializerDictionary.Prefix.Value, this.parent.SerializerDictionary.BinarySecret, this.parent.SerializerDictionary.Namespace);
                writer.WriteBase64(keyBytes, 0, keyBytes.Length);
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return this.parent.SerializerDictionary.BinarySecret;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return this.parent.SerializerDictionary.Namespace;
                }
            }
        }

        private class BinarySecretTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            private TrustDictionary otherDictionary;
            private WSTrust parent;

            public BinarySecretTokenEntry(WSTrust parent)
            {
                this.parent = parent;
                this.otherDictionary = null;
                if (parent.SerializerDictionary is TrustDec2005Dictionary)
                {
                    this.otherDictionary = XD.TrustFeb2005Dictionary;
                }
                if (parent.SerializerDictionary is TrustFeb2005Dictionary)
                {
                    this.otherDictionary = DXD.TrustDec2005Dictionary;
                }
                if (this.otherDictionary == null)
                {
                    this.otherDictionary = this.parent.SerializerDictionary;
                }
            }

            public override bool CanReadTokenCore(XmlDictionaryReader reader)
            {
                if (!reader.IsStartElement(this.LocalName, this.NamespaceUri) && !reader.IsStartElement(this.LocalName, this.otherDictionary.Namespace))
                {
                    return false;
                }
                return (reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null) == this.ValueTypeUri);
            }

            public override bool CanReadTokenCore(XmlElement element)
            {
                string attribute = null;
                if (element.HasAttribute("ValueType", null))
                {
                    attribute = element.GetAttribute("ValueType", null);
                }
                if (!(element.LocalName == this.LocalName.Value) || (!(element.NamespaceURI == this.NamespaceUri.Value) && !(element.NamespaceURI == this.otherDictionary.Namespace.Value)))
                {
                    return false;
                }
                return (attribute == this.ValueTypeUri);
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);
                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return WSSecurityTokenSerializer.TokenEntry.CreateDirectReference(issuedTokenXml, "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", typeof(GenericXmlSecurityToken));

                    case SecurityTokenReferenceStyle.External:
                        return null;
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
            }

            protected override Type[] GetTokenTypesCore()
            {
                return new Type[] { typeof(BinarySecretSecurityToken) };
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.TypeAttribute, null);
                string id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                bool flag = false;
                if ((attribute != null) && (attribute.Length > 0))
                {
                    if ((attribute == this.parent.SerializerDictionary.NonceBinarySecret.Value) || (attribute == this.otherDictionary.NonceBinarySecret.Value))
                    {
                        flag = true;
                    }
                    else if ((attribute != this.parent.SerializerDictionary.SymmetricKeyBinarySecret.Value) && (attribute != this.otherDictionary.SymmetricKeyBinarySecret.Value))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnexpectedBinarySecretType", new object[] { this.parent.SerializerDictionary.SymmetricKeyBinarySecret.Value, attribute })));
                    }
                }
                byte[] key = reader.ReadElementContentAsBase64();
                if (flag)
                {
                    return new NonceToken(id, key);
                }
                return new BinarySecretSecurityToken(id, key);
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                BinarySecretSecurityToken token2 = token as BinarySecretSecurityToken;
                byte[] keyBytes = token2.GetKeyBytes();
                writer.WriteStartElement(this.parent.SerializerDictionary.Prefix.Value, this.parent.SerializerDictionary.BinarySecret, this.parent.SerializerDictionary.Namespace);
                if (token2.Id != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, token2.Id);
                }
                if (token is NonceToken)
                {
                    writer.WriteAttributeString(XD.SecurityJan2004Dictionary.TypeAttribute, null, this.parent.SerializerDictionary.NonceBinarySecret.Value);
                }
                writer.WriteBase64(keyBytes, 0, keyBytes.Length);
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return this.parent.SerializerDictionary.BinarySecret;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return this.parent.SerializerDictionary.Namespace;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return null;
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return null;
                }
            }
        }

        public abstract class Driver : TrustDriver
        {
            private static readonly string base64Uri = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
            private List<SecurityTokenAuthenticator> entropyAuthenticators;
            private static readonly string hexBinaryUri = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary";
            private SecurityStandardsManager standardsManager;

            public Driver(SecurityStandardsManager standardsManager)
            {
                this.standardsManager = standardsManager;
                this.entropyAuthenticators = new List<SecurityTokenAuthenticator>(2);
            }

            public override XmlElement CreateCanonicalizationAlgorithmElement(string algorithm)
            {
                if (algorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("algorithm");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.CanonicalizationAlgorithm.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(algorithm));
                return element;
            }

            public override XmlElement CreateComputedKeyAlgorithmElement(string algorithm)
            {
                if (algorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("algorithm");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.ComputedKeyAlgorithm.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(algorithm));
                return element;
            }

            public override XmlElement CreateEncryptionAlgorithmElement(string encryptionAlgorithm)
            {
                if (encryptionAlgorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptionAlgorithm");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.EncryptionAlgorithm.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(encryptionAlgorithm));
                return element;
            }

            public override XmlElement CreateEncryptWithElement(string encryptionAlgorithm)
            {
                if (encryptionAlgorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptionAlgorithm");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.EncryptWith.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(encryptionAlgorithm));
                return element;
            }

            public override XmlElement CreateKeySizeElement(int keySize)
            {
                if (keySize < 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keySize", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeySize.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(keySize.ToString(CultureInfo.InvariantCulture.NumberFormat)));
                return element;
            }

            public override XmlElement CreateKeyTypeElement(SecurityKeyType keyType)
            {
                if (keyType == SecurityKeyType.SymmetricKey)
                {
                    return this.CreateSymmetricKeyTypeElement();
                }
                if (keyType != SecurityKeyType.AsymmetricKey)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnableToCreateKeyTypeElementForUnknownKeyType", new object[] { keyType.ToString() })));
                }
                return this.CreatePublicKeyTypeElement();
            }

            private XmlElement CreatePublicKeyTypeElement()
            {
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeyType.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(this.DriverDictionary.PublicKeyType.Value));
                return element;
            }

            public override RequestSecurityToken CreateRequestSecurityToken(XmlReader xmlReader)
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
                reader.MoveToStartElement(this.DriverDictionary.RequestSecurityToken, this.DriverDictionary.Namespace);
                string context = null;
                string tokenType = null;
                string requestType = null;
                int keySize = 0;
                XmlDocument document = new XmlDocument();
                XmlElement rstXml = document.ReadNode(reader) as XmlElement;
                SecurityKeyIdentifierClause renewTarget = null;
                SecurityKeyIdentifierClause closeTarget = null;
                for (int i = 0; i < rstXml.Attributes.Count; i++)
                {
                    System.Xml.XmlAttribute attribute = rstXml.Attributes[i];
                    if (attribute.LocalName == this.DriverDictionary.Context.Value)
                    {
                        context = attribute.Value;
                    }
                }
                for (int j = 0; j < rstXml.ChildNodes.Count; j++)
                {
                    XmlElement element = rstXml.ChildNodes[j] as XmlElement;
                    if (element != null)
                    {
                        if ((element.LocalName == this.DriverDictionary.TokenType.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            tokenType = XmlHelper.ReadTextElementAsTrimmedString(element);
                        }
                        else if ((element.LocalName == this.DriverDictionary.RequestType.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            requestType = XmlHelper.ReadTextElementAsTrimmedString(element);
                        }
                        else if ((element.LocalName == this.DriverDictionary.KeySize.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            keySize = int.Parse(XmlHelper.ReadTextElementAsTrimmedString(element), NumberFormatInfo.InvariantInfo);
                        }
                    }
                }
                this.ReadTargets(rstXml, out renewTarget, out closeTarget);
                return new RequestSecurityToken(this.standardsManager, rstXml, context, tokenType, requestType, keySize, renewTarget, closeTarget);
            }

            public override RequestSecurityTokenResponse CreateRequestSecurityTokenResponse(XmlReader xmlReader)
            {
                XmlElement element;
                if (xmlReader == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
                }
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
                if (!reader.IsStartElement(this.DriverDictionary.RequestSecurityTokenResponse, this.DriverDictionary.Namespace))
                {
                    XmlHelper.OnRequiredElementMissing(this.DriverDictionary.RequestSecurityTokenResponse.Value, this.DriverDictionary.Namespace.Value);
                }
                XmlBuffer rstrBuffer = new XmlBuffer(0x7fffffff);
                using (XmlDictionaryWriter writer = rstrBuffer.OpenSection(reader.Quotas))
                {
                    writer.WriteNode(reader, false);
                    rstrBuffer.CloseSection();
                    rstrBuffer.Close();
                }
                XmlDocument document = new XmlDocument();
                using (XmlReader reader2 = rstrBuffer.GetReader(0))
                {
                    element = document.ReadNode(reader2) as XmlElement;
                }
                XmlBuffer issuedTokenBuffer = this.GetIssuedTokenBuffer(rstrBuffer);
                string context = null;
                string tokenType = null;
                int keySize = 0;
                SecurityKeyIdentifierClause requestedAttachedReference = null;
                SecurityKeyIdentifierClause requestedUnattachedReference = null;
                bool computeKey = false;
                DateTime utcNow = DateTime.UtcNow;
                DateTime maxUtcDateTime = System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
                bool isRequestedTokenClosed = false;
                for (int i = 0; i < element.Attributes.Count; i++)
                {
                    System.Xml.XmlAttribute attribute = element.Attributes[i];
                    if (attribute.LocalName == this.DriverDictionary.Context.Value)
                    {
                        context = attribute.Value;
                    }
                }
                for (int j = 0; j < element.ChildNodes.Count; j++)
                {
                    XmlElement element2 = element.ChildNodes[j] as XmlElement;
                    if (element2 != null)
                    {
                        if ((element2.LocalName == this.DriverDictionary.TokenType.Value) && (element2.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            tokenType = XmlHelper.ReadTextElementAsTrimmedString(element2);
                        }
                        else if ((element2.LocalName == this.DriverDictionary.KeySize.Value) && (element2.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            keySize = int.Parse(XmlHelper.ReadTextElementAsTrimmedString(element2), NumberFormatInfo.InvariantInfo);
                        }
                        else if ((element2.LocalName == this.DriverDictionary.RequestedProofToken.Value) && (element2.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            XmlElement childElement = XmlHelper.GetChildElement(element2);
                            if ((childElement.LocalName == this.DriverDictionary.ComputedKey.Value) && (childElement.NamespaceURI == this.DriverDictionary.Namespace.Value))
                            {
                                string str3 = XmlHelper.ReadTextElementAsTrimmedString(childElement);
                                if (str3 != this.DriverDictionary.Psha1ComputedKeyUri.Value)
                                {
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("UnknownComputedKeyAlgorithm", new object[] { str3 })));
                                }
                                computeKey = true;
                            }
                        }
                        else if ((element2.LocalName == this.DriverDictionary.Lifetime.Value) && (element2.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            XmlElement element4 = XmlHelper.GetChildElement(element2, "Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                            if (element4 != null)
                            {
                                utcNow = DateTime.ParseExact(XmlHelper.ReadTextElementAsTrimmedString(element4), WSUtilitySpecificationVersion.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
                            }
                            XmlElement element5 = XmlHelper.GetChildElement(element2, "Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                            if (element5 != null)
                            {
                                maxUtcDateTime = DateTime.ParseExact(XmlHelper.ReadTextElementAsTrimmedString(element5), WSUtilitySpecificationVersion.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
                            }
                        }
                    }
                }
                isRequestedTokenClosed = this.ReadRequestedTokenClosed(element);
                this.ReadReferences(element, out requestedAttachedReference, out requestedUnattachedReference);
                return new RequestSecurityTokenResponse(this.standardsManager, element, context, tokenType, keySize, requestedAttachedReference, requestedUnattachedReference, computeKey, utcNow, maxUtcDateTime, isRequestedTokenClosed, issuedTokenBuffer);
            }

            public override RequestSecurityTokenResponseCollection CreateRequestSecurityTokenResponseCollection(XmlReader xmlReader)
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
                List<RequestSecurityTokenResponse> list = new List<RequestSecurityTokenResponse>(2);
                string name = reader.Name;
                reader.ReadStartElement(this.DriverDictionary.RequestSecurityTokenResponseCollection, this.DriverDictionary.Namespace);
                while (reader.IsStartElement(this.DriverDictionary.RequestSecurityTokenResponse.Value, this.DriverDictionary.Namespace.Value))
                {
                    RequestSecurityTokenResponse item = this.CreateRequestSecurityTokenResponse(reader);
                    list.Add(item);
                }
                reader.ReadEndElement();
                if (list.Count == 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("NoRequestSecurityTokenResponseElements")));
                }
                return new RequestSecurityTokenResponseCollection(list.AsReadOnly(), this.StandardsManager);
            }

            public override XmlElement CreateRequiredClaimsElement(IEnumerable<XmlElement> claimsList)
            {
                if (claimsList == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimsList");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.Claims.Value, this.DriverDictionary.Namespace.Value);
                foreach (XmlElement element2 in claimsList)
                {
                    XmlElement newChild = (XmlElement) document.ImportNode(element2, true);
                    element.AppendChild(newChild);
                }
                return element;
            }

            public override XmlElement CreateSignWithElement(string signatureAlgorithm)
            {
                if (signatureAlgorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signatureAlgorithm");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.SignWith.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(signatureAlgorithm));
                return element;
            }

            private XmlElement CreateSymmetricKeyTypeElement()
            {
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeyType.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(this.DriverDictionary.SymmetricKeyType.Value));
                return element;
            }

            public override XmlElement CreateTokenTypeElement(string tokenTypeUri)
            {
                if (tokenTypeUri == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenTypeUri");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.TokenType.Value, this.DriverDictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(tokenTypeUri));
                return element;
            }

            public override XmlElement CreateUseKeyElement(SecurityKeyIdentifier keyIdentifier, SecurityStandardsManager standardsManager)
            {
                if (keyIdentifier == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
                }
                if (standardsManager == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("standardsManager");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(this.DriverDictionary.UseKey.Value, this.DriverDictionary.Namespace.Value);
                MemoryStream w = new MemoryStream();
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(new XmlTextWriter(w, Encoding.UTF8)))
                {
                    System.Xml.XmlNode node;
                    standardsManager.SecurityTokenSerializer.WriteKeyIdentifier(writer, keyIdentifier);
                    writer.Flush();
                    w.Seek(0L, SeekOrigin.Begin);
                    using (XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(new XmlTextReader(w)))
                    {
                        reader.MoveToContent();
                        node = document.ReadNode(reader);
                    }
                    element.AppendChild(node);
                }
                return element;
            }

            public override T GetAppliesTo<T>(RequestSecurityToken rst, XmlObjectSerializer serializer)
            {
                if (rst == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
                }
                return this.GetAppliesTo<T>(rst.RequestSecurityTokenXml, serializer);
            }

            public override T GetAppliesTo<T>(RequestSecurityTokenResponse rstr, XmlObjectSerializer serializer)
            {
                if (rstr == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
                }
                return this.GetAppliesTo<T>(rstr.RequestSecurityTokenResponseXml, serializer);
            }

            private T GetAppliesTo<T>(XmlElement rootXml, XmlObjectSerializer serializer)
            {
                XmlElement appliesToElement = this.GetAppliesToElement(rootXml);
                if (appliesToElement != null)
                {
                    using (XmlReader reader = new XmlNodeReader(appliesToElement))
                    {
                        reader.ReadStartElement();
                        lock (serializer)
                        {
                            return (T) serializer.ReadObject(reader);
                        }
                    }
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoAppliesToPresent")));
            }

            private XmlElement GetAppliesToElement(XmlElement rootElement)
            {
                if (rootElement != null)
                {
                    for (int i = 0; i < rootElement.ChildNodes.Count; i++)
                    {
                        XmlElement element = rootElement.ChildNodes[i] as XmlElement;
                        if (((element != null) && (element.LocalName == this.DriverDictionary.AppliesTo.Value)) && (element.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/policy"))
                        {
                            return element;
                        }
                    }
                }
                return null;
            }

            public override void GetAppliesToQName(RequestSecurityToken rst, out string localName, out string namespaceUri)
            {
                if (rst == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
                }
                this.GetAppliesToQName(rst.RequestSecurityTokenXml, out localName, out namespaceUri);
            }

            public override void GetAppliesToQName(RequestSecurityTokenResponse rstr, out string localName, out string namespaceUri)
            {
                if (rstr == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
                }
                this.GetAppliesToQName(rstr.RequestSecurityTokenResponseXml, out localName, out namespaceUri);
            }

            private void GetAppliesToQName(XmlElement rootElement, out string localName, out string namespaceUri)
            {
                localName = (string) (namespaceUri = null);
                XmlElement appliesToElement = this.GetAppliesToElement(rootElement);
                if (appliesToElement != null)
                {
                    using (XmlReader reader = new XmlNodeReader(appliesToElement))
                    {
                        reader.ReadStartElement();
                        reader.MoveToContent();
                        localName = reader.LocalName;
                        namespaceUri = reader.NamespaceURI;
                    }
                }
            }

            public override byte[] GetAuthenticator(RequestSecurityTokenResponse rstr)
            {
                if (((rstr != null) && (rstr.RequestSecurityTokenResponseXml != null)) && (rstr.RequestSecurityTokenResponseXml.ChildNodes != null))
                {
                    for (int i = 0; i < rstr.RequestSecurityTokenResponseXml.ChildNodes.Count; i++)
                    {
                        XmlElement parent = rstr.RequestSecurityTokenResponseXml.ChildNodes[i] as XmlElement;
                        if (((parent != null) && (parent.LocalName == this.DriverDictionary.Authenticator.Value)) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            XmlElement childElement = XmlHelper.GetChildElement(parent);
                            if ((childElement.LocalName == this.DriverDictionary.CombinedHash.Value) && (childElement.NamespaceURI == this.DriverDictionary.Namespace.Value))
                            {
                                return Convert.FromBase64String(XmlHelper.ReadTextElementAsTrimmedString(childElement));
                            }
                        }
                    }
                }
                return null;
            }

            public override BinaryNegotiation GetBinaryNegotiation(RequestSecurityToken rst)
            {
                if (rst == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
                }
                return this.GetBinaryNegotiation(rst.RequestSecurityTokenXml);
            }

            public override BinaryNegotiation GetBinaryNegotiation(RequestSecurityTokenResponse rstr)
            {
                if (rstr == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
                }
                return this.GetBinaryNegotiation(rstr.RequestSecurityTokenResponseXml);
            }

            private BinaryNegotiation GetBinaryNegotiation(XmlElement rootElement)
            {
                if (rootElement != null)
                {
                    for (int i = 0; i < rootElement.ChildNodes.Count; i++)
                    {
                        XmlElement elem = rootElement.ChildNodes[i] as XmlElement;
                        if (((elem != null) && (elem.LocalName == this.DriverDictionary.BinaryExchange.Value)) && (elem.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            return ReadBinaryNegotiation(elem);
                        }
                    }
                }
                return null;
            }

            public override SecurityToken GetEntropy(RequestSecurityToken rst, SecurityTokenResolver resolver)
            {
                if (rst == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
                }
                return this.GetEntropy(rst.RequestSecurityTokenXml, resolver);
            }

            public override SecurityToken GetEntropy(RequestSecurityTokenResponse rstr, SecurityTokenResolver resolver)
            {
                if (rstr == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
                }
                return this.GetEntropy(rstr.RequestSecurityTokenResponseXml, resolver);
            }

            private SecurityToken GetEntropy(XmlElement rootElement, SecurityTokenResolver resolver)
            {
                if ((rootElement != null) && (rootElement.ChildNodes != null))
                {
                    for (int i = 0; i < rootElement.ChildNodes.Count; i++)
                    {
                        XmlElement parent = rootElement.ChildNodes[i] as XmlElement;
                        if (((parent != null) && (parent.LocalName == this.DriverDictionary.Entropy.Value)) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            XmlElement childElement = XmlHelper.GetChildElement(parent);
                            if (parent.GetAttribute("ValueType").Length == 0)
                            {
                            }
                            return this.standardsManager.SecurityTokenSerializer.ReadToken(new XmlNodeReader(childElement), resolver);
                        }
                    }
                }
                return null;
            }

            private void GetIssuedAndProofXml(RequestSecurityTokenResponse rstr, out XmlElement issuedTokenXml, out XmlElement proofTokenXml)
            {
                issuedTokenXml = null;
                proofTokenXml = null;
                if ((rstr.RequestSecurityTokenResponseXml != null) && (rstr.RequestSecurityTokenResponseXml.ChildNodes != null))
                {
                    for (int i = 0; i < rstr.RequestSecurityTokenResponseXml.ChildNodes.Count; i++)
                    {
                        XmlElement parent = rstr.RequestSecurityTokenResponseXml.ChildNodes[i] as XmlElement;
                        if (parent != null)
                        {
                            if ((parent.LocalName == this.DriverDictionary.RequestedSecurityToken.Value) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                            {
                                if (issuedTokenXml != null)
                                {
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("RstrHasMultipleIssuedTokens")));
                                }
                                issuedTokenXml = XmlHelper.GetChildElement(parent);
                            }
                            else if ((parent.LocalName == this.DriverDictionary.RequestedProofToken.Value) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                            {
                                if (proofTokenXml != null)
                                {
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("RstrHasMultipleProofTokens")));
                                }
                                proofTokenXml = XmlHelper.GetChildElement(parent);
                            }
                        }
                    }
                }
            }

            public override GenericXmlSecurityToken GetIssuedToken(RequestSecurityTokenResponse rstr, string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, RSA clientKey)
            {
                XmlElement element;
                XmlElement element2;
                if (rstr == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("rstr"));
                }
                if (rstr.TokenType != null)
                {
                    if ((expectedTokenType != null) && (expectedTokenType != rstr.TokenType))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BadIssuedTokenType", new object[] { rstr.TokenType, expectedTokenType })));
                    }
                    string tokenType = rstr.TokenType;
                }
                DateTime validFrom = rstr.ValidFrom;
                DateTime validTo = rstr.ValidTo;
                this.GetIssuedAndProofXml(rstr, out element2, out element);
                if (element2 == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoLicenseXml")));
                }
                if (element != null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProofTokenXmlUnexpectedInRstr")));
                }
                SecurityKeyIdentifierClause requestedAttachedReference = rstr.RequestedAttachedReference;
                SecurityKeyIdentifierClause requestedUnattachedReference = rstr.RequestedUnattachedReference;
                return new BufferedGenericXmlSecurityToken(element2, new RsaSecurityToken(clientKey), validFrom, validTo, requestedAttachedReference, requestedUnattachedReference, authorizationPolicies, rstr.IssuedTokenBuffer);
            }

            public override GenericXmlSecurityToken GetIssuedToken(RequestSecurityTokenResponse rstr, SecurityTokenResolver resolver, IList<SecurityTokenAuthenticator> allowedAuthenticators, SecurityKeyEntropyMode keyEntropyMode, byte[] requestorEntropy, string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, int defaultKeySize, bool isBearerKeyType)
            {
                XmlElement element;
                XmlElement element2;
                SecurityToken token;
                SecurityKeyEntropyModeHelper.Validate(keyEntropyMode);
                if (defaultKeySize < 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("defaultKeySize", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if (rstr == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
                }
                if (rstr.TokenType != null)
                {
                    if ((expectedTokenType != null) && (expectedTokenType != rstr.TokenType))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BadIssuedTokenType", new object[] { rstr.TokenType, expectedTokenType })));
                    }
                    string tokenType = rstr.TokenType;
                }
                DateTime validFrom = rstr.ValidFrom;
                DateTime validTo = rstr.ValidTo;
                this.GetIssuedAndProofXml(rstr, out element2, out element);
                if (element2 == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoLicenseXml")));
                }
                if (isBearerKeyType)
                {
                    if (element != null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BearerKeyTypeCannotHaveProofKey")));
                    }
                    return new GenericXmlSecurityToken(element2, null, validFrom, validTo, rstr.RequestedAttachedReference, rstr.RequestedUnattachedReference, authorizationPolicies);
                }
                SecurityToken entropy = this.GetEntropy(rstr, resolver);
                if (keyEntropyMode == SecurityKeyEntropyMode.ClientEntropy)
                {
                    if (requestorEntropy == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeRequiresRequestorEntropy", new object[] { keyEntropyMode })));
                    }
                    if ((element != null) || (entropy != null))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeCannotHaveProofTokenOrIssuerEntropy", new object[] { keyEntropyMode })));
                    }
                    token = new BinarySecretSecurityToken(requestorEntropy);
                }
                else if (keyEntropyMode == SecurityKeyEntropyMode.ServerEntropy)
                {
                    if (requestorEntropy != null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeCannotHaveRequestorEntropy", new object[] { keyEntropyMode })));
                    }
                    if (rstr.ComputeKey || (entropy != null))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeCannotHaveComputedKey", new object[] { keyEntropyMode })));
                    }
                    if (element == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeRequiresProofToken", new object[] { keyEntropyMode })));
                    }
                    if (element.GetAttribute("ValueType").Length == 0)
                    {
                    }
                    token = this.standardsManager.SecurityTokenSerializer.ReadToken(new XmlNodeReader(element), resolver);
                }
                else
                {
                    byte[] keyBytes;
                    if (!rstr.ComputeKey)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeRequiresComputedKey", new object[] { keyEntropyMode })));
                    }
                    if (entropy == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeRequiresIssuerEntropy", new object[] { keyEntropyMode })));
                    }
                    if (requestorEntropy == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeRequiresRequestorEntropy", new object[] { keyEntropyMode })));
                    }
                    if ((rstr.KeySize == 0) && (defaultKeySize == 0))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("RstrKeySizeNotProvided")));
                    }
                    int keySizeInBits = (rstr.KeySize != 0) ? rstr.KeySize : defaultKeySize;
                    if (entropy is BinarySecretSecurityToken)
                    {
                        keyBytes = ((BinarySecretSecurityToken) entropy).GetKeyBytes();
                    }
                    else
                    {
                        if (!(entropy is WrappedKeySecurityToken))
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnsupportedIssuerEntropyType")));
                        }
                        keyBytes = ((WrappedKeySecurityToken) entropy).GetWrappedKey();
                    }
                    token = new BinarySecretSecurityToken(RequestSecurityTokenResponse.ComputeCombinedKey(requestorEntropy, keyBytes, keySizeInBits));
                }
                SecurityKeyIdentifierClause requestedAttachedReference = rstr.RequestedAttachedReference;
                return new BufferedGenericXmlSecurityToken(element2, token, validFrom, validTo, requestedAttachedReference, rstr.RequestedUnattachedReference, authorizationPolicies, rstr.IssuedTokenBuffer);
            }

            private XmlBuffer GetIssuedTokenBuffer(XmlBuffer rstrBuffer)
            {
                XmlBuffer buffer = null;
                using (XmlDictionaryReader reader = rstrBuffer.GetReader(0))
                {
                    reader.ReadFullStartElement();
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement(this.DriverDictionary.RequestedSecurityToken, this.DriverDictionary.Namespace))
                        {
                            reader.ReadStartElement();
                            reader.MoveToContent();
                            buffer = new XmlBuffer(0x7fffffff);
                            using (XmlDictionaryWriter writer = buffer.OpenSection(reader.Quotas))
                            {
                                writer.WriteNode(reader, false);
                                buffer.CloseSection();
                                buffer.Close();
                            }
                            reader.ReadEndElement();
                            return buffer;
                        }
                        reader.Skip();
                    }
                }
                return buffer;
            }

            public override bool IsAppliesTo(string localName, string namespaceUri)
            {
                return ((localName == this.DriverDictionary.AppliesTo.Value) && (namespaceUri == "http://schemas.xmlsoap.org/ws/2004/09/policy"));
            }

            public override bool IsAtRequestSecurityTokenResponse(XmlReader reader)
            {
                if (reader == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
                }
                return reader.IsStartElement(this.DriverDictionary.RequestSecurityTokenResponse.Value, this.DriverDictionary.Namespace.Value);
            }

            public override bool IsAtRequestSecurityTokenResponseCollection(XmlReader reader)
            {
                if (reader == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
                }
                return reader.IsStartElement(this.DriverDictionary.RequestSecurityTokenResponseCollection.Value, this.DriverDictionary.Namespace.Value);
            }

            internal override bool IsCanonicalizationAlgorithmElement(XmlElement element, out string canonicalizationAlgorithm)
            {
                return WSTrust.CheckElement(element, this.DriverDictionary.CanonicalizationAlgorithm.Value, this.DriverDictionary.Namespace.Value, out canonicalizationAlgorithm);
            }

            internal override bool IsEncryptionAlgorithmElement(XmlElement element, out string encryptionAlgorithm)
            {
                return WSTrust.CheckElement(element, this.DriverDictionary.EncryptionAlgorithm.Value, this.DriverDictionary.Namespace.Value, out encryptionAlgorithm);
            }

            internal override bool IsEncryptWithElement(XmlElement element, out string encryptWithAlgorithm)
            {
                return WSTrust.CheckElement(element, this.DriverDictionary.EncryptWith.Value, this.DriverDictionary.Namespace.Value, out encryptWithAlgorithm);
            }

            public override bool IsRequestedProofTokenElement(string name, string nameSpace)
            {
                return ((name == this.DriverDictionary.RequestedProofToken.Value) && (nameSpace == this.DriverDictionary.Namespace.Value));
            }

            public override bool IsRequestedSecurityTokenElement(string name, string nameSpace)
            {
                return ((name == this.DriverDictionary.RequestedSecurityToken.Value) && (nameSpace == this.DriverDictionary.Namespace.Value));
            }

            internal override bool IsSignWithElement(XmlElement element, out string signatureAlgorithm)
            {
                return WSTrust.CheckElement(element, this.DriverDictionary.SignWith.Value, this.DriverDictionary.Namespace.Value, out signatureAlgorithm);
            }

            public override void OnRSTRorRSTRCMissingException()
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ExpectedOneOfTwoElementsFromNamespace", new object[] { this.DriverDictionary.RequestSecurityTokenResponse, this.DriverDictionary.RequestSecurityTokenResponseCollection, this.DriverDictionary.Namespace })));
            }

            internal static void ProcessRstAndIssueKey(RequestSecurityToken requestSecurityToken, SecurityTokenResolver resolver, SecurityKeyEntropyMode keyEntropyMode, SecurityAlgorithmSuite algorithmSuite, out int issuedKeySize, out byte[] issuerEntropy, out byte[] proofKey, out SecurityToken proofToken)
            {
                byte[] keyBytes;
                SecurityToken requestorEntropy = requestSecurityToken.GetRequestorEntropy(resolver);
                ValidateRequestorEntropy(requestorEntropy, keyEntropyMode);
                if (requestorEntropy != null)
                {
                    if (requestorEntropy is BinarySecretSecurityToken)
                    {
                        keyBytes = ((BinarySecretSecurityToken) requestorEntropy).GetKeyBytes();
                    }
                    else
                    {
                        if (!(requestorEntropy is WrappedKeySecurityToken))
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("TokenCannotCreateSymmetricCrypto", new object[] { requestorEntropy })));
                        }
                        keyBytes = ((WrappedKeySecurityToken) requestorEntropy).GetWrappedKey();
                    }
                }
                else
                {
                    keyBytes = null;
                }
                if (keyEntropyMode == SecurityKeyEntropyMode.ClientEntropy)
                {
                    if (keyBytes != null)
                    {
                        ValidateRequestedKeySize(keyBytes.Length * 8, algorithmSuite);
                    }
                    proofKey = keyBytes;
                    issuerEntropy = null;
                    issuedKeySize = 0;
                    proofToken = null;
                }
                else
                {
                    if (requestSecurityToken.KeySize != 0)
                    {
                        ValidateRequestedKeySize(requestSecurityToken.KeySize, algorithmSuite);
                        issuedKeySize = requestSecurityToken.KeySize;
                    }
                    else
                    {
                        issuedKeySize = algorithmSuite.DefaultSymmetricKeyLength;
                    }
                    RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                    if (keyEntropyMode == SecurityKeyEntropyMode.ServerEntropy)
                    {
                        proofKey = new byte[issuedKeySize / 8];
                        provider.GetNonZeroBytes(proofKey);
                        issuerEntropy = null;
                        proofToken = new BinarySecretSecurityToken(proofKey);
                    }
                    else
                    {
                        issuerEntropy = new byte[issuedKeySize / 8];
                        provider.GetNonZeroBytes(issuerEntropy);
                        proofKey = RequestSecurityTokenResponse.ComputeCombinedKey(keyBytes, issuerEntropy, issuedKeySize);
                        proofToken = null;
                    }
                }
            }

            public static BinaryNegotiation ReadBinaryNegotiation(XmlElement elem)
            {
                if (elem == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elem");
                }
                string str = null;
                string valueTypeUri = null;
                byte[] negotiationData = null;
                if (elem.Attributes != null)
                {
                    for (int i = 0; i < elem.Attributes.Count; i++)
                    {
                        System.Xml.XmlAttribute attribute = elem.Attributes[i];
                        if ((attribute.LocalName == "EncodingType") && (attribute.NamespaceURI.Length == 0))
                        {
                            str = attribute.Value;
                            if ((str != base64Uri) && (str != hexBinaryUri))
                            {
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnsupportedBinaryEncoding", new object[] { str })));
                            }
                        }
                        else if ((attribute.LocalName == "ValueType") && (attribute.NamespaceURI.Length == 0))
                        {
                            valueTypeUri = attribute.Value;
                        }
                    }
                }
                if (str == null)
                {
                    XmlHelper.OnRequiredAttributeMissing("EncodingType", elem.Name);
                }
                if (valueTypeUri == null)
                {
                    XmlHelper.OnRequiredAttributeMissing("ValueType", elem.Name);
                }
                string s = XmlHelper.ReadTextElementAsTrimmedString(elem);
                if (str == base64Uri)
                {
                    negotiationData = Convert.FromBase64String(s);
                }
                else
                {
                    negotiationData = SoapHexBinary.Parse(s).Value;
                }
                return new BinaryNegotiation(valueTypeUri, negotiationData);
            }

            protected virtual void ReadReferences(XmlElement rstrXml, out SecurityKeyIdentifierClause requestedAttachedReference, out SecurityKeyIdentifierClause requestedUnattachedReference)
            {
                XmlElement childElement = null;
                requestedAttachedReference = null;
                requestedUnattachedReference = null;
                for (int i = 0; i < rstrXml.ChildNodes.Count; i++)
                {
                    XmlElement parent = rstrXml.ChildNodes[i] as XmlElement;
                    if (parent != null)
                    {
                        if ((parent.LocalName == this.DriverDictionary.RequestedSecurityToken.Value) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            childElement = XmlHelper.GetChildElement(parent);
                        }
                        else if ((parent.LocalName == this.DriverDictionary.RequestedTokenReference.Value) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            requestedUnattachedReference = this.standardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(new XmlNodeReader(XmlHelper.GetChildElement(parent)));
                        }
                    }
                }
                if (childElement != null)
                {
                    requestedAttachedReference = this.standardsManager.CreateKeyIdentifierClauseFromTokenXml(childElement, SecurityTokenReferenceStyle.Internal);
                    if (requestedUnattachedReference == null)
                    {
                        try
                        {
                            requestedUnattachedReference = this.standardsManager.CreateKeyIdentifierClauseFromTokenXml(childElement, SecurityTokenReferenceStyle.External);
                        }
                        catch (XmlException)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("TrustDriverIsUnableToCreatedNecessaryAttachedOrUnattachedReferences", new object[] { childElement.ToString() })));
                        }
                    }
                }
            }

            protected virtual bool ReadRequestedTokenClosed(XmlElement rstrXml)
            {
                return false;
            }

            protected virtual void ReadTargets(XmlElement rstXml, out SecurityKeyIdentifierClause renewTarget, out SecurityKeyIdentifierClause closeTarget)
            {
                renewTarget = null;
                closeTarget = null;
            }

            protected void SetProtectionLevelForFederation(OperationDescriptionCollection operations)
            {
                foreach (OperationDescription description in operations)
                {
                    foreach (MessageDescription description2 in description.Messages)
                    {
                        if (description2.Body.Parts.Count > 0)
                        {
                            foreach (MessagePartDescription description3 in description2.Body.Parts)
                            {
                                description3.ProtectionLevel = ProtectionLevel.EncryptAndSign;
                            }
                        }
                        if (OperationFormatter.IsValidReturnValue(description2.Body.ReturnValue))
                        {
                            description2.Body.ReturnValue.ProtectionLevel = ProtectionLevel.EncryptAndSign;
                        }
                    }
                }
            }

            public override bool TryParseKeySizeElement(XmlElement element, out int keySize)
            {
                if (element == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
                if ((element.LocalName == this.DriverDictionary.KeySize.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                {
                    keySize = int.Parse(XmlHelper.ReadTextElementAsTrimmedString(element), NumberFormatInfo.InvariantInfo);
                    return true;
                }
                keySize = 0;
                return false;
            }

            public override bool TryParseKeyTypeElement(XmlElement element, out SecurityKeyType keyType)
            {
                if (element == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
                if (this.TryParseSymmetricKeyElement(element))
                {
                    keyType = SecurityKeyType.SymmetricKey;
                    return true;
                }
                if (this.TryParsePublicKeyElement(element))
                {
                    keyType = SecurityKeyType.AsymmetricKey;
                    return true;
                }
                keyType = SecurityKeyType.SymmetricKey;
                return false;
            }

            private bool TryParsePublicKeyElement(XmlElement element)
            {
                if (element == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
                return (((element.LocalName == this.DriverDictionary.KeyType.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value)) && (element.InnerText == this.DriverDictionary.PublicKeyType.Value));
            }

            public override bool TryParseRequiredClaimsElement(XmlElement element, out Collection<XmlElement> requiredClaims)
            {
                if (element == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
                if ((element.LocalName == this.DriverDictionary.Claims.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                {
                    requiredClaims = new Collection<XmlElement>();
                    foreach (System.Xml.XmlNode node in element.ChildNodes)
                    {
                        if (node is XmlElement)
                        {
                            requiredClaims.Add((XmlElement) node);
                        }
                    }
                    return true;
                }
                requiredClaims = null;
                return false;
            }

            public bool TryParseSymmetricKeyElement(XmlElement element)
            {
                if (element == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
                return (((element.LocalName == this.DriverDictionary.KeyType.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value)) && (element.InnerText == this.DriverDictionary.SymmetricKeyType.Value));
            }

            public override bool TryParseTokenTypeElement(XmlElement element, out string tokenType)
            {
                if (element == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
                if ((element.LocalName == this.DriverDictionary.TokenType.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                {
                    tokenType = element.InnerText;
                    return true;
                }
                tokenType = null;
                return false;
            }

            internal static void ValidateRequestedKeySize(int keySize, SecurityAlgorithmSuite algorithmSuite)
            {
                if (((keySize % 8) != 0) || !algorithmSuite.IsSymmetricKeyLengthSupported(keySize))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidKeyLengthRequested", new object[] { keySize })));
                }
            }

            private static void ValidateRequestorEntropy(SecurityToken entropy, SecurityKeyEntropyMode mode)
            {
                if (((mode == SecurityKeyEntropyMode.ClientEntropy) || (mode == SecurityKeyEntropyMode.CombinedEntropy)) && (entropy == null))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeRequiresRequestorEntropy", new object[] { mode })));
                }
                if ((mode == SecurityKeyEntropyMode.ServerEntropy) && (entropy != null))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("EntropyModeCannotHaveRequestorEntropy", new object[] { mode })));
                }
            }

            private void WriteAppliesTo(object appliesTo, Type appliesToType, XmlObjectSerializer serializer, XmlWriter xmlWriter)
            {
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                writer.WriteStartElement("wsp", this.DriverDictionary.AppliesTo.Value, "http://schemas.xmlsoap.org/ws/2004/09/policy");
                lock (serializer)
                {
                    serializer.WriteObject(writer, appliesTo);
                }
                writer.WriteEndElement();
            }

            public void WriteBinaryNegotiation(BinaryNegotiation negotiation, XmlWriter xmlWriter)
            {
                if (negotiation == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("negotiation");
                }
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                negotiation.WriteTo(writer, this.DriverDictionary.Prefix.Value, this.DriverDictionary.BinaryExchange, this.DriverDictionary.Namespace, XD.SecurityJan2004Dictionary.ValueType, null);
            }

            protected virtual void WriteReferences(RequestSecurityTokenResponse rstr, XmlDictionaryWriter writer)
            {
                if (rstr.RequestedUnattachedReference != null)
                {
                    writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestedTokenReference, this.DriverDictionary.Namespace);
                    this.standardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, rstr.RequestedUnattachedReference);
                    writer.WriteEndElement();
                }
            }

            protected virtual void WriteRequestedTokenClosed(RequestSecurityTokenResponse rstr, XmlDictionaryWriter writer)
            {
            }

            public override void WriteRequestSecurityToken(RequestSecurityToken rst, XmlWriter xmlWriter)
            {
                if (rst == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
                }
                if (xmlWriter == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlWriter");
                }
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                if (rst.IsReceiver)
                {
                    rst.WriteTo(writer);
                }
                else
                {
                    writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestSecurityToken, this.DriverDictionary.Namespace);
                    XmlHelper.AddNamespaceDeclaration(writer, this.DriverDictionary.Prefix.Value, this.DriverDictionary.Namespace);
                    if (rst.Context != null)
                    {
                        writer.WriteAttributeString(this.DriverDictionary.Context, null, rst.Context);
                    }
                    rst.OnWriteCustomAttributes(writer);
                    if (rst.TokenType != null)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.TokenType, this.DriverDictionary.Namespace);
                        writer.WriteString(rst.TokenType);
                        writer.WriteEndElement();
                    }
                    if (rst.RequestType != null)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestType, this.DriverDictionary.Namespace);
                        writer.WriteString(rst.RequestType);
                        writer.WriteEndElement();
                    }
                    if (rst.AppliesTo != null)
                    {
                        this.WriteAppliesTo(rst.AppliesTo, rst.AppliesToType, rst.AppliesToSerializer, writer);
                    }
                    SecurityToken requestorEntropy = rst.GetRequestorEntropy();
                    if (requestorEntropy != null)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.Entropy, this.DriverDictionary.Namespace);
                        this.standardsManager.SecurityTokenSerializer.WriteToken(writer, requestorEntropy);
                        writer.WriteEndElement();
                    }
                    if (rst.KeySize != 0)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeySize, this.DriverDictionary.Namespace);
                        writer.WriteValue(rst.KeySize);
                        writer.WriteEndElement();
                    }
                    BinaryNegotiation binaryNegotiation = rst.GetBinaryNegotiation();
                    if (binaryNegotiation != null)
                    {
                        this.WriteBinaryNegotiation(binaryNegotiation, writer);
                    }
                    this.WriteTargets(rst, writer);
                    if (rst.RequestProperties != null)
                    {
                        foreach (XmlElement element in rst.RequestProperties)
                        {
                            element.WriteTo(writer);
                        }
                    }
                    rst.OnWriteCustomElements(writer);
                    writer.WriteEndElement();
                }
            }

            public override void WriteRequestSecurityTokenResponse(RequestSecurityTokenResponse rstr, XmlWriter xmlWriter)
            {
                if (rstr == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
                }
                if (xmlWriter == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlWriter");
                }
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                if (rstr.IsReceiver)
                {
                    rstr.WriteTo(writer);
                }
                else
                {
                    writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestSecurityTokenResponse, this.DriverDictionary.Namespace);
                    if (rstr.Context != null)
                    {
                        writer.WriteAttributeString(this.DriverDictionary.Context, null, rstr.Context);
                    }
                    XmlHelper.AddNamespaceDeclaration(writer, "u", XD.UtilityDictionary.Namespace);
                    rstr.OnWriteCustomAttributes(writer);
                    if (rstr.TokenType != null)
                    {
                        writer.WriteElementString(this.DriverDictionary.Prefix.Value, this.DriverDictionary.TokenType, this.DriverDictionary.Namespace, rstr.TokenType);
                    }
                    if (rstr.RequestedSecurityToken != null)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestedSecurityToken, this.DriverDictionary.Namespace);
                        this.standardsManager.SecurityTokenSerializer.WriteToken(writer, rstr.RequestedSecurityToken);
                        writer.WriteEndElement();
                    }
                    if (rstr.AppliesTo != null)
                    {
                        this.WriteAppliesTo(rstr.AppliesTo, rstr.AppliesToType, rstr.AppliesToSerializer, writer);
                    }
                    this.WriteReferences(rstr, writer);
                    if (rstr.ComputeKey || (rstr.RequestedProofToken != null))
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestedProofToken, this.DriverDictionary.Namespace);
                        if (rstr.ComputeKey)
                        {
                            writer.WriteElementString(this.DriverDictionary.Prefix.Value, this.DriverDictionary.ComputedKey, this.DriverDictionary.Namespace, this.DriverDictionary.Psha1ComputedKeyUri.Value);
                        }
                        else
                        {
                            this.standardsManager.SecurityTokenSerializer.WriteToken(writer, rstr.RequestedProofToken);
                        }
                        writer.WriteEndElement();
                    }
                    SecurityToken issuerEntropy = rstr.GetIssuerEntropy();
                    if (issuerEntropy != null)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.Entropy, this.DriverDictionary.Namespace);
                        this.standardsManager.SecurityTokenSerializer.WriteToken(writer, issuerEntropy);
                        writer.WriteEndElement();
                    }
                    if (rstr.IsLifetimeSet || (rstr.RequestedSecurityToken != null))
                    {
                        DateTime minUtcDateTime = System.ServiceModel.Security.SecurityUtils.MinUtcDateTime;
                        DateTime maxUtcDateTime = System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
                        if (rstr.IsLifetimeSet)
                        {
                            minUtcDateTime = rstr.ValidFrom.ToUniversalTime();
                            maxUtcDateTime = rstr.ValidTo.ToUniversalTime();
                        }
                        else if (rstr.RequestedSecurityToken != null)
                        {
                            minUtcDateTime = rstr.RequestedSecurityToken.ValidFrom.ToUniversalTime();
                            maxUtcDateTime = rstr.RequestedSecurityToken.ValidTo.ToUniversalTime();
                        }
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.Lifetime, this.DriverDictionary.Namespace);
                        writer.WriteStartElement(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.CreatedElement, XD.UtilityDictionary.Namespace);
                        writer.WriteString(minUtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture.DateTimeFormat));
                        writer.WriteEndElement();
                        writer.WriteStartElement(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.ExpiresElement, XD.UtilityDictionary.Namespace);
                        writer.WriteString(maxUtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture.DateTimeFormat));
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    byte[] authenticator = rstr.GetAuthenticator();
                    if (authenticator != null)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.Authenticator, this.DriverDictionary.Namespace);
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.CombinedHash, this.DriverDictionary.Namespace);
                        writer.WriteBase64(authenticator, 0, authenticator.Length);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    if (rstr.KeySize > 0)
                    {
                        writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeySize, this.DriverDictionary.Namespace);
                        writer.WriteValue(rstr.KeySize);
                        writer.WriteEndElement();
                    }
                    this.WriteRequestedTokenClosed(rstr, writer);
                    BinaryNegotiation binaryNegotiation = rstr.GetBinaryNegotiation();
                    if (binaryNegotiation != null)
                    {
                        this.WriteBinaryNegotiation(binaryNegotiation, writer);
                    }
                    rstr.OnWriteCustomElements(writer);
                    writer.WriteEndElement();
                }
            }

            public override void WriteRequestSecurityTokenResponseCollection(RequestSecurityTokenResponseCollection rstrCollection, XmlWriter xmlWriter)
            {
                if (rstrCollection == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstrCollection");
                }
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestSecurityTokenResponseCollection, this.DriverDictionary.Namespace);
                foreach (RequestSecurityTokenResponse response in rstrCollection.RstrCollection)
                {
                    response.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            protected virtual void WriteTargets(RequestSecurityToken rst, XmlDictionaryWriter writer)
            {
            }

            public override string ComputedKeyAlgorithm
            {
                get
                {
                    return this.DriverDictionary.Psha1ComputedKeyUri.Value;
                }
            }

            public abstract TrustDictionary DriverDictionary { get; }

            public override XmlDictionaryString Namespace
            {
                get
                {
                    return this.DriverDictionary.Namespace;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenAction
            {
                get
                {
                    return this.DriverDictionary.RequestSecurityTokenIssuance;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenResponseAction
            {
                get
                {
                    return this.DriverDictionary.RequestSecurityTokenIssuanceResponse;
                }
            }

            public override string RequestTypeIssue
            {
                get
                {
                    return this.DriverDictionary.RequestTypeIssue.Value;
                }
            }

            public override SecurityStandardsManager StandardsManager
            {
                get
                {
                    return this.standardsManager;
                }
            }
        }
    }
}

