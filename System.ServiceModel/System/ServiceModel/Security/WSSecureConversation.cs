namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal abstract class WSSecureConversation : System.ServiceModel.Security.WSSecurityTokenSerializer.SerializerEntries
    {
        private DerivedKeyTokenEntry derivedKeyEntry;
        private System.ServiceModel.Security.WSSecurityTokenSerializer tokenSerializer;

        protected WSSecureConversation(System.ServiceModel.Security.WSSecurityTokenSerializer tokenSerializer, int maxKeyDerivationOffset, int maxKeyDerivationLabelLength, int maxKeyDerivationNonceLength)
        {
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer");
            }
            this.tokenSerializer = tokenSerializer;
            this.derivedKeyEntry = new DerivedKeyTokenEntry(this, maxKeyDerivationOffset, maxKeyDerivationLabelLength, maxKeyDerivationNonceLength);
        }

        public virtual SecurityToken CreateDerivedKeyToken(string id, string derivationAlgorithm, string label, int length, byte[] nonce, int offset, int generation, SecurityKeyIdentifierClause tokenToDeriveIdentifier, SecurityToken tokenToDerive)
        {
            return this.derivedKeyEntry.CreateDerivedKeyToken(id, derivationAlgorithm, label, length, nonce, offset, generation, tokenToDeriveIdentifier, tokenToDerive);
        }

        public virtual bool IsAtDerivedKeyToken(XmlDictionaryReader reader)
        {
            return this.derivedKeyEntry.CanReadTokenCore(reader);
        }

        public override void PopulateTokenEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            if (tokenEntryList == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenEntryList");
            }
            tokenEntryList.Add(this.derivedKeyEntry);
        }

        public virtual void ReadDerivedKeyTokenParameters(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver, out string id, out string derivationAlgorithm, out string label, out int length, out byte[] nonce, out int offset, out int generation, out SecurityKeyIdentifierClause tokenToDeriveIdentifier, out SecurityToken tokenToDerive)
        {
            this.derivedKeyEntry.ReadDerivedKeyTokenParameters(reader, tokenResolver, out id, out derivationAlgorithm, out label, out length, out nonce, out offset, out generation, out tokenToDeriveIdentifier, out tokenToDerive);
        }

        public virtual string DerivationAlgorithm
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1";
            }
        }

        public abstract SecureConversationDictionary SerializerDictionary { get; }

        public System.ServiceModel.Security.WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get
            {
                return this.tokenSerializer;
            }
        }

        protected class DerivedKeyTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            public const string DefaultLabel = "WS-SecureConversation";
            private int maxKeyDerivationLabelLength;
            private int maxKeyDerivationNonceLength;
            private int maxKeyDerivationOffset;
            private WSSecureConversation parent;

            public DerivedKeyTokenEntry(WSSecureConversation parent, int maxKeyDerivationOffset, int maxKeyDerivationLabelLength, int maxKeyDerivationNonceLength)
            {
                if (parent == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
                }
                this.parent = parent;
                this.maxKeyDerivationOffset = maxKeyDerivationOffset;
                this.maxKeyDerivationLabelLength = maxKeyDerivationLabelLength;
                this.maxKeyDerivationNonceLength = maxKeyDerivationNonceLength;
            }

            public virtual SecurityToken CreateDerivedKeyToken(string id, string derivationAlgorithm, string label, int length, byte[] nonce, int offset, int generation, SecurityKeyIdentifierClause tokenToDeriveIdentifier, SecurityToken tokenToDerive)
            {
                if (tokenToDerive == null)
                {
                    return new DerivedKeySecurityTokenStub(generation, offset, length, label, nonce, tokenToDeriveIdentifier, derivationAlgorithm, id);
                }
                return new DerivedKeySecurityToken(generation, offset, length, label, nonce, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm, id);
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);
                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return WSSecurityTokenSerializer.TokenEntry.CreateDirectReference(issuedTokenXml, "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", typeof(DerivedKeySecurityToken));

                    case SecurityTokenReferenceStyle.External:
                        return null;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
            }

            protected override Type[] GetTokenTypesCore()
            {
                return new Type[] { typeof(DerivedKeySecurityToken) };
            }

            public virtual void ReadDerivedKeyTokenParameters(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver, out string id, out string derivationAlgorithm, out string label, out int length, out byte[] nonce, out int offset, out int generation, out SecurityKeyIdentifierClause tokenToDeriveIdentifier, out SecurityToken tokenToDerive)
            {
                if (tokenResolver == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenResolver");
                }
                id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                derivationAlgorithm = reader.GetAttribute(XD.XmlSignatureDictionary.Algorithm, null);
                if (derivationAlgorithm == null)
                {
                    derivationAlgorithm = this.parent.DerivationAlgorithm;
                }
                reader.ReadStartElement();
                tokenToDeriveIdentifier = null;
                tokenToDerive = null;
                if (!reader.IsStartElement(XD.SecurityJan2004Dictionary.SecurityTokenReference, XD.SecurityJan2004Dictionary.Namespace))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("DerivedKeyTokenRequiresTokenReference")));
                }
                tokenToDeriveIdentifier = this.parent.WSSecurityTokenSerializer.ReadKeyIdentifierClause(reader);
                tokenResolver.TryResolveToken(tokenToDeriveIdentifier, out tokenToDerive);
                generation = -1;
                if (reader.IsStartElement(this.parent.SerializerDictionary.Generation, this.parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    generation = reader.ReadContentAsInt();
                    reader.ReadEndElement();
                    if (generation < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("DerivedKeyInvalidGenerationSpecified", new object[] { (int) generation })));
                    }
                }
                offset = -1;
                if (reader.IsStartElement(this.parent.SerializerDictionary.Offset, this.parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    offset = reader.ReadContentAsInt();
                    reader.ReadEndElement();
                    if (offset < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("DerivedKeyInvalidOffsetSpecified", new object[] { (int) offset })));
                    }
                }
                length = 0x20;
                if (reader.IsStartElement(this.parent.SerializerDictionary.Length, this.parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    length = reader.ReadContentAsInt();
                    reader.ReadEndElement();
                }
                if ((offset == -1) && (generation == -1))
                {
                    offset = 0;
                }
                DerivedKeySecurityToken.EnsureAcceptableOffset(offset, generation, length, this.maxKeyDerivationOffset);
                label = null;
                if (reader.IsStartElement(this.parent.SerializerDictionary.Label, this.parent.SerializerDictionary.Namespace))
                {
                    reader.ReadStartElement();
                    label = reader.ReadString();
                    reader.ReadEndElement();
                }
                if ((label != null) && (label.Length > this.maxKeyDerivationLabelLength))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("DerivedKeyTokenLabelTooLong", new object[] { label.Length, this.maxKeyDerivationLabelLength })));
                }
                nonce = null;
                reader.ReadStartElement(this.parent.SerializerDictionary.Nonce, this.parent.SerializerDictionary.Namespace);
                nonce = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                if ((nonce != null) && (nonce.Length > this.maxKeyDerivationNonceLength))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("DerivedKeyTokenNonceTooLong", new object[] { nonce.Length, this.maxKeyDerivationNonceLength })));
                }
                reader.ReadEndElement();
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                string str;
                string str2;
                string str3;
                int num;
                byte[] buffer;
                int num2;
                int num3;
                SecurityKeyIdentifierClause clause;
                SecurityToken token;
                this.ReadDerivedKeyTokenParameters(reader, tokenResolver, out str, out str2, out str3, out num, out buffer, out num2, out num3, out clause, out token);
                return this.CreateDerivedKeyToken(str, str2, str3, num, buffer, num2, num3, clause, token);
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                DerivedKeySecurityToken token2 = token as DerivedKeySecurityToken;
                string prefix = this.parent.SerializerDictionary.Prefix.Value;
                writer.WriteStartElement(prefix, this.parent.SerializerDictionary.DerivedKeyToken, this.parent.SerializerDictionary.Namespace);
                if (token2.Id != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, token2.Id);
                }
                if (token2.KeyDerivationAlgorithm != this.parent.DerivationAlgorithm)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnsupportedKeyDerivationAlgorithm", new object[] { token2.KeyDerivationAlgorithm })));
                }
                this.parent.WSSecurityTokenSerializer.WriteKeyIdentifierClause(writer, token2.TokenToDeriveIdentifier);
                if (((token2.Generation > 0) || (token2.Offset > 0)) || (token2.Length != 0x20))
                {
                    if ((token2.Generation >= 0) && (token2.Offset >= 0))
                    {
                        writer.WriteStartElement(prefix, this.parent.SerializerDictionary.Generation, this.parent.SerializerDictionary.Namespace);
                        writer.WriteValue(token2.Generation);
                        writer.WriteEndElement();
                    }
                    else if (token2.Generation != -1)
                    {
                        writer.WriteStartElement(prefix, this.parent.SerializerDictionary.Generation, this.parent.SerializerDictionary.Namespace);
                        writer.WriteValue(token2.Generation);
                        writer.WriteEndElement();
                    }
                    else if (token2.Offset != -1)
                    {
                        writer.WriteStartElement(prefix, this.parent.SerializerDictionary.Offset, this.parent.SerializerDictionary.Namespace);
                        writer.WriteValue(token2.Offset);
                        writer.WriteEndElement();
                    }
                    if (token2.Length != 0x20)
                    {
                        writer.WriteStartElement(prefix, this.parent.SerializerDictionary.Length, this.parent.SerializerDictionary.Namespace);
                        writer.WriteValue(token2.Length);
                        writer.WriteEndElement();
                    }
                }
                if (token2.Label != null)
                {
                    writer.WriteStartElement(prefix, this.parent.SerializerDictionary.Generation, this.parent.SerializerDictionary.Namespace);
                    writer.WriteString(token2.Label);
                    writer.WriteEndElement();
                }
                writer.WriteStartElement(prefix, this.parent.SerializerDictionary.Nonce, this.parent.SerializerDictionary.Namespace);
                writer.WriteBase64(token2.Nonce, 0, token2.Nonce.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return this.parent.SerializerDictionary.DerivedKeyToken;
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
                    return this.parent.SerializerDictionary.DerivedKeyTokenType.Value;
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

        public abstract class Driver : SecureConversationDriver
        {
            public override UniqueId GetSecurityContextTokenId(XmlDictionaryReader reader)
            {
                if (reader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
                }
                reader.ReadStartElement(this.DriverDictionary.SecurityContextToken, this.DriverDictionary.Namespace);
                UniqueId id = XmlHelper.ReadElementStringAsUniqueId(reader, this.DriverDictionary.Identifier, this.DriverDictionary.Namespace);
                while (reader.IsStartElement())
                {
                    reader.Skip();
                }
                reader.ReadEndElement();
                return id;
            }

            public override bool IsAtSecurityContextToken(XmlDictionaryReader reader)
            {
                if (reader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
                }
                return reader.IsStartElement(this.DriverDictionary.SecurityContextToken, this.DriverDictionary.Namespace);
            }

            public override XmlDictionaryString BadContextTokenFaultCode
            {
                get
                {
                    return this.DriverDictionary.BadContextTokenFaultCode;
                }
            }

            protected abstract SecureConversationDictionary DriverDictionary { get; }

            public override XmlDictionaryString IssueAction
            {
                get
                {
                    return this.DriverDictionary.RequestSecurityContextIssuance;
                }
            }

            public override XmlDictionaryString IssueResponseAction
            {
                get
                {
                    return this.DriverDictionary.RequestSecurityContextIssuanceResponse;
                }
            }

            public override XmlDictionaryString RenewNeededFaultCode
            {
                get
                {
                    return this.DriverDictionary.RenewNeededFaultCode;
                }
            }
        }

        protected abstract class SctStrEntry : WSSecurityTokenSerializer.StrEntry
        {
            private WSSecureConversation parent;

            public SctStrEntry(WSSecureConversation parent)
            {
                this.parent = parent;
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (((tokenType == null) || (tokenType == this.parent.SerializerDictionary.SecurityContextTokenType.Value)) && reader.IsStartElement(XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                    if ((attribute != null) && (attribute != this.parent.SerializerDictionary.SecurityContextTokenReferenceValueType.Value))
                    {
                        return false;
                    }
                    string str2 = reader.GetAttribute(XD.SecurityJan2004Dictionary.URI, null);
                    if (((str2 != null) && (str2.Length > 0)) && (str2[0] != '#'))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return typeof(SecurityContextSecurityToken);
            }

            public override string GetTokenTypeUri()
            {
                return null;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType)
            {
                UniqueId contextId = XmlHelper.GetAttributeAsUniqueId(reader, XD.SecurityJan2004Dictionary.URI, null);
                UniqueId generation = this.ReadGeneration(reader);
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadStartElement();
                    while (reader.IsStartElement())
                    {
                        reader.Skip();
                    }
                    reader.ReadEndElement();
                }
                return new SecurityContextKeyIdentifierClause(contextId, generation, derivationNonce, derivationLength);
            }

            protected abstract UniqueId ReadGeneration(XmlDictionaryReader reader);
            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return (clause is SecurityContextKeyIdentifierClause);
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                SecurityContextKeyIdentifierClause clause2 = clause as SecurityContextKeyIdentifierClause;
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace);
                XmlHelper.WriteAttributeStringAsUniqueId(writer, null, XD.SecurityJan2004Dictionary.URI, null, clause2.ContextId);
                this.WriteGeneration(writer, clause2);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, this.parent.SerializerDictionary.SecurityContextTokenReferenceValueType.Value);
                writer.WriteEndElement();
            }

            protected abstract void WriteGeneration(XmlDictionaryWriter writer, SecurityContextKeyIdentifierClause clause);

            protected WSSecureConversation Parent
            {
                get
                {
                    return this.parent;
                }
            }
        }

        protected abstract class SecurityContextTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            private SecurityContextCookieSerializer cookieSerializer;
            private WSSecureConversation parent;

            public SecurityContextTokenEntry(WSSecureConversation parent, SecurityStateEncoder securityStateEncoder, IList<Type> knownClaimTypes)
            {
                this.parent = parent;
                this.cookieSerializer = new SecurityContextCookieSerializer(securityStateEncoder, knownClaimTypes);
            }

            protected abstract bool CanReadGeneration(XmlDictionaryReader reader);
            protected abstract bool CanReadGeneration(XmlElement element);
            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);
                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return WSSecurityTokenSerializer.TokenEntry.CreateDirectReference(issuedTokenXml, "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", typeof(SecurityContextSecurityToken));

                    case SecurityTokenReferenceStyle.External:
                    {
                        UniqueId contextId = null;
                        UniqueId generation = null;
                        foreach (System.Xml.XmlNode node in issuedTokenXml.ChildNodes)
                        {
                            XmlElement element = node as XmlElement;
                            if (element != null)
                            {
                                if ((element.LocalName == this.parent.SerializerDictionary.Identifier.Value) && (element.NamespaceURI == this.parent.SerializerDictionary.Namespace.Value))
                                {
                                    contextId = XmlHelper.ReadTextElementAsUniqueId(element);
                                }
                                else if (this.CanReadGeneration(element))
                                {
                                    generation = this.ReadGeneration(element);
                                }
                            }
                        }
                        return new SecurityContextKeyIdentifierClause(contextId, generation);
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
            }

            protected override Type[] GetTokenTypesCore()
            {
                return new Type[] { typeof(SecurityContextSecurityToken) };
            }

            protected abstract UniqueId ReadGeneration(XmlDictionaryReader reader);
            protected abstract UniqueId ReadGeneration(XmlElement element);
            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                UniqueId contextId = null;
                byte[] encodedCookie = null;
                UniqueId generation = null;
                bool flag = false;
                string attribute = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                SecurityContextSecurityToken token = null;
                reader.ReadFullStartElement();
                reader.MoveToStartElement(this.parent.SerializerDictionary.Identifier, this.parent.SerializerDictionary.Namespace);
                contextId = reader.ReadElementContentAsUniqueId();
                if (this.CanReadGeneration(reader))
                {
                    generation = this.ReadGeneration(reader);
                }
                if (reader.IsStartElement(this.parent.SerializerDictionary.Cookie, XD.DotNetSecurityDictionary.Namespace))
                {
                    ISecurityContextSecurityTokenCache cache;
                    flag = true;
                    token = this.TryResolveSecurityContextToken(contextId, generation, attribute, tokenResolver, out cache);
                    if (token == null)
                    {
                        encodedCookie = reader.ReadElementContentAsBase64();
                        if (encodedCookie != null)
                        {
                            token = this.cookieSerializer.CreateSecurityContextFromCookie(encodedCookie, contextId, generation, attribute, reader.Quotas);
                            if (cache != null)
                            {
                                cache.AddContext(token);
                            }
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
                if (contextId == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSecurityContextIdentifier")));
                }
                if ((token == null) && !flag)
                {
                    ISecurityContextSecurityTokenCache cache2;
                    token = this.TryResolveSecurityContextToken(contextId, generation, attribute, tokenResolver, out cache2);
                }
                if (token == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityContextTokenValidationException(System.ServiceModel.SR.GetString("SecurityContextNotRegistered", new object[] { contextId, generation })));
                }
                return token;
            }

            private SecurityContextSecurityToken TryResolveSecurityContextToken(UniqueId contextId, UniqueId generation, string id, SecurityTokenResolver tokenResolver, out ISecurityContextSecurityTokenCache sctCache)
            {
                SecurityContextSecurityToken sourceToken = null;
                sctCache = null;
                if (tokenResolver is ISecurityContextSecurityTokenCache)
                {
                    sctCache = (ISecurityContextSecurityTokenCache) tokenResolver;
                    sourceToken = sctCache.GetContext(contextId, generation);
                }
                else if (tokenResolver is AggregateTokenResolver)
                {
                    AggregateTokenResolver resolver = tokenResolver as AggregateTokenResolver;
                    for (int i = 0; i < resolver.OutOfBandTokenResolver.Count; i++)
                    {
                        ISecurityContextSecurityTokenCache cache = resolver.OutOfBandTokenResolver[i] as ISecurityContextSecurityTokenCache;
                        if (cache != null)
                        {
                            if (sctCache == null)
                            {
                                sctCache = cache;
                            }
                            sourceToken = cache.GetContext(contextId, generation);
                            if (sourceToken != null)
                            {
                                break;
                            }
                        }
                    }
                }
                if (sourceToken == null)
                {
                    return null;
                }
                if (sourceToken.Id == id)
                {
                    return sourceToken;
                }
                return new SecurityContextSecurityToken(sourceToken, id);
            }

            protected virtual void WriteGeneration(XmlDictionaryWriter writer, SecurityContextSecurityToken sct)
            {
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                SecurityContextSecurityToken sct = token as SecurityContextSecurityToken;
                writer.WriteStartElement(this.parent.SerializerDictionary.Prefix.Value, this.parent.SerializerDictionary.SecurityContextToken, this.parent.SerializerDictionary.Namespace);
                if (sct.Id != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, sct.Id);
                }
                writer.WriteStartElement(this.parent.SerializerDictionary.Prefix.Value, this.parent.SerializerDictionary.Identifier, this.parent.SerializerDictionary.Namespace);
                XmlHelper.WriteStringAsUniqueId(writer, sct.ContextId);
                writer.WriteEndElement();
                this.WriteGeneration(writer, sct);
                if (sct.IsCookieMode)
                {
                    if (sct.CookieBlob == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoCookieInSct")));
                    }
                    writer.WriteStartElement(XD.DotNetSecurityDictionary.Prefix.Value, this.parent.SerializerDictionary.Cookie, XD.DotNetSecurityDictionary.Namespace);
                    writer.WriteBase64(sct.CookieBlob, 0, sct.CookieBlob.Length);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return this.parent.SerializerDictionary.SecurityContextToken;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return this.parent.SerializerDictionary.Namespace;
                }
            }

            protected WSSecureConversation Parent
            {
                get
                {
                    return this.parent;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return this.parent.SerializerDictionary.SecurityContextTokenType.Value;
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
    }
}

