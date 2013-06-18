namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    public class WSSecurityTokenSerializer : SecurityTokenSerializer
    {
        private const int DefaultMaximumKeyDerivationLabelLength = 0x80;
        private const int DefaultMaximumKeyDerivationNonceLength = 0x80;
        private const int DefaultMaximumKeyDerivationOffset = 0x40;
        private readonly bool emitBspRequiredAttributes;
        private static WSSecurityTokenSerializer instance;
        private readonly List<KeyIdentifierClauseEntry> keyIdentifierClauseEntries;
        private readonly List<KeyIdentifierEntry> keyIdentifierEntries;
        private int maximumKeyDerivationLabelLength;
        private int maximumKeyDerivationNonceLength;
        private int maximumKeyDerivationOffset;
        private WSSecureConversation secureConversation;
        private readonly System.ServiceModel.Security.SecurityVersion securityVersion;
        private readonly List<SerializerEntries> serializerEntries;
        private readonly List<TokenEntry> tokenEntries;

        public WSSecurityTokenSerializer() : this(System.ServiceModel.Security.SecurityVersion.WSSecurity11)
        {
        }

        public WSSecurityTokenSerializer(bool emitBspRequiredAttributes) : this(System.ServiceModel.Security.SecurityVersion.WSSecurity11, emitBspRequiredAttributes)
        {
        }

        public WSSecurityTokenSerializer(System.ServiceModel.Security.SecurityVersion securityVersion) : this(securityVersion, false)
        {
        }

        public WSSecurityTokenSerializer(System.ServiceModel.Security.SecurityVersion securityVersion, bool emitBspRequiredAttributes) : this(securityVersion, emitBspRequiredAttributes, null)
        {
        }

        public WSSecurityTokenSerializer(System.ServiceModel.Security.SecurityVersion securityVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer) : this(securityVersion, emitBspRequiredAttributes, samlSerializer, null, null)
        {
        }

        public WSSecurityTokenSerializer(System.ServiceModel.Security.SecurityVersion securityVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes) : this(securityVersion, emitBspRequiredAttributes, samlSerializer, securityStateEncoder, knownTypes, 0x40, 0x80, 0x80)
        {
        }

        public WSSecurityTokenSerializer(System.ServiceModel.Security.SecurityVersion securityVersion, TrustVersion trustVersion, SecureConversationVersion secureConversationVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes) : this(securityVersion, trustVersion, secureConversationVersion, emitBspRequiredAttributes, samlSerializer, securityStateEncoder, knownTypes, 0x40, 0x80, 0x80)
        {
        }

        public WSSecurityTokenSerializer(System.ServiceModel.Security.SecurityVersion securityVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes, int maximumKeyDerivationOffset, int maximumKeyDerivationLabelLength, int maximumKeyDerivationNonceLength) : this(securityVersion, TrustVersion.Default, SecureConversationVersion.Default, emitBspRequiredAttributes, samlSerializer, securityStateEncoder, knownTypes, maximumKeyDerivationOffset, maximumKeyDerivationLabelLength, maximumKeyDerivationNonceLength)
        {
        }

        public WSSecurityTokenSerializer(System.ServiceModel.Security.SecurityVersion securityVersion, TrustVersion trustVersion, SecureConversationVersion secureConversationVersion, bool emitBspRequiredAttributes, SamlSerializer samlSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes, int maximumKeyDerivationOffset, int maximumKeyDerivationLabelLength, int maximumKeyDerivationNonceLength)
        {
            if (securityVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("securityVersion"));
            }
            if (maximumKeyDerivationOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maximumKeyDerivationOffset", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if (maximumKeyDerivationLabelLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maximumKeyDerivationLabelLength", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if (maximumKeyDerivationNonceLength <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maximumKeyDerivationNonceLength", System.ServiceModel.SR.GetString("ValueMustBeGreaterThanZero")));
            }
            this.securityVersion = securityVersion;
            this.emitBspRequiredAttributes = emitBspRequiredAttributes;
            this.maximumKeyDerivationOffset = maximumKeyDerivationOffset;
            this.maximumKeyDerivationNonceLength = maximumKeyDerivationNonceLength;
            this.maximumKeyDerivationLabelLength = maximumKeyDerivationLabelLength;
            this.serializerEntries = new List<SerializerEntries>();
            if (secureConversationVersion == SecureConversationVersion.WSSecureConversationFeb2005)
            {
                this.secureConversation = new WSSecureConversationFeb2005(this, securityStateEncoder, knownTypes, maximumKeyDerivationOffset, maximumKeyDerivationLabelLength, maximumKeyDerivationNonceLength);
            }
            else
            {
                if (secureConversationVersion != SecureConversationVersion.WSSecureConversation13)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                this.secureConversation = new WSSecureConversationDec2005(this, securityStateEncoder, knownTypes, maximumKeyDerivationOffset, maximumKeyDerivationLabelLength, maximumKeyDerivationNonceLength);
            }
            this.serializerEntries.Add(new XmlDsigSep2000(this));
            this.serializerEntries.Add(new XmlEncApr2001(this));
            if (securityVersion == System.ServiceModel.Security.SecurityVersion.WSSecurity10)
            {
                this.serializerEntries.Add(new WSSecurityJan2004(this, samlSerializer));
            }
            else
            {
                if (securityVersion != System.ServiceModel.Security.SecurityVersion.WSSecurity11)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("securityVersion", System.ServiceModel.SR.GetString("MessageSecurityVersionOutOfRange")));
                }
                this.serializerEntries.Add(new WSSecurityXXX2005(this, samlSerializer));
            }
            this.serializerEntries.Add(this.secureConversation);
            if (trustVersion == TrustVersion.WSTrustFeb2005)
            {
                this.serializerEntries.Add(new WSTrustFeb2005(this));
            }
            else
            {
                if (trustVersion != TrustVersion.WSTrust13)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                this.serializerEntries.Add(new WSTrustDec2005(this));
            }
            this.tokenEntries = new List<TokenEntry>();
            this.keyIdentifierEntries = new List<KeyIdentifierEntry>();
            this.keyIdentifierClauseEntries = new List<KeyIdentifierClauseEntry>();
            for (int i = 0; i < this.serializerEntries.Count; i++)
            {
                SerializerEntries entries = this.serializerEntries[i];
                entries.PopulateTokenEntries(this.tokenEntries);
                entries.PopulateKeyIdentifierEntries(this.keyIdentifierEntries);
                entries.PopulateKeyIdentifierClauseEntries(this.keyIdentifierClauseEntries);
            }
        }

        protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; i++)
            {
                KeyIdentifierClauseEntry entry = this.keyIdentifierClauseEntries[i];
                if (entry.CanReadKeyIdentifierClauseCore(reader2))
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool CanReadKeyIdentifierCore(XmlReader reader)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.keyIdentifierEntries.Count; i++)
            {
                KeyIdentifierEntry entry = this.keyIdentifierEntries[i];
                if (entry.CanReadKeyIdentifierCore(reader2))
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool CanReadTokenCore(XmlReader reader)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry entry = this.tokenEntries[i];
                if (entry.CanReadTokenCore(reader2))
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; i++)
            {
                KeyIdentifierClauseEntry entry = this.keyIdentifierClauseEntries[i];
                if (entry.SupportsCore(keyIdentifierClause))
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
        {
            for (int i = 0; i < this.keyIdentifierEntries.Count; i++)
            {
                KeyIdentifierEntry entry = this.keyIdentifierEntries[i];
                if (entry.SupportsCore(keyIdentifier))
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool CanWriteTokenCore(SecurityToken token)
        {
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry entry = this.tokenEntries[i];
                if (entry.SupportsCore(token.GetType()))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry entry = this.tokenEntries[i];
                if (entry.CanReadTokenCore(element))
                {
                    try
                    {
                        return entry.CreateKeyIdentifierClauseFromTokenXmlCore(element, tokenReferenceStyle);
                    }
                    catch (Exception exception)
                    {
                        if (!this.ShouldWrapException(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorDeserializingKeyIdentifierClauseFromTokenXml"), exception));
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CannotReadToken", new object[] { element.LocalName, element.NamespaceURI, element.GetAttribute("ValueType", null) })));
        }

        internal Type[] GetTokenTypes(string tokenTypeUri)
        {
            if (tokenTypeUri != null)
            {
                for (int i = 0; i < this.tokenEntries.Count; i++)
                {
                    TokenEntry entry = this.tokenEntries[i];
                    if (entry.SupportsTokenTypeUri(tokenTypeUri))
                    {
                        return entry.GetTokenTypes();
                    }
                }
            }
            return null;
        }

        protected internal virtual string GetTokenTypeUri(Type tokenType)
        {
            if (tokenType != null)
            {
                for (int i = 0; i < this.tokenEntries.Count; i++)
                {
                    TokenEntry entry = this.tokenEntries[i];
                    if (entry.SupportsCore(tokenType))
                    {
                        return entry.TokenTypeUri;
                    }
                }
            }
            return null;
        }

        internal void PopulateStrEntries(IList<StrEntry> strEntries)
        {
            foreach (SerializerEntries entries in this.serializerEntries)
            {
                entries.PopulateStrEntries(strEntries);
            }
        }

        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; i++)
            {
                KeyIdentifierClauseEntry entry = this.keyIdentifierClauseEntries[i];
                if (entry.CanReadKeyIdentifierClauseCore(reader2))
                {
                    try
                    {
                        return entry.ReadKeyIdentifierClauseCore(reader2);
                    }
                    catch (Exception exception)
                    {
                        if (!this.ShouldWrapException(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorDeserializingKeyIdentifierClause"), exception));
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CannotReadKeyIdentifierClause", new object[] { reader.LocalName, reader.NamespaceURI })));
        }

        protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.keyIdentifierEntries.Count; i++)
            {
                KeyIdentifierEntry entry = this.keyIdentifierEntries[i];
                if (entry.CanReadKeyIdentifierCore(reader2))
                {
                    try
                    {
                        return entry.ReadKeyIdentifierCore(reader2);
                    }
                    catch (Exception exception)
                    {
                        if (!this.ShouldWrapException(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorDeserializingKeyIdentifier"), exception));
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CannotReadKeyIdentifier", new object[] { reader.LocalName, reader.NamespaceURI })));
        }

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry entry = this.tokenEntries[i];
                if (entry.CanReadTokenCore(reader2))
                {
                    try
                    {
                        return entry.ReadTokenCore(reader2, tokenResolver);
                    }
                    catch (Exception exception)
                    {
                        if (!this.ShouldWrapException(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorDeserializingTokenXml"), exception));
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CannotReadToken", new object[] { reader.LocalName, reader.NamespaceURI, reader2.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null) })));
        }

        private bool ShouldWrapException(Exception e)
        {
            if (Fx.IsFatal(e))
            {
                return false;
            }
            return (((e is ArgumentException) || (e is FormatException)) || (e is InvalidOperationException));
        }

        public virtual bool TryCreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle, out SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            securityKeyIdentifierClause = null;
            try
            {
                securityKeyIdentifierClause = this.CreateKeyIdentifierClauseFromTokenXml(element, tokenReferenceStyle);
            }
            catch (XmlException exception)
            {
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x70000, System.ServiceModel.SR.GetString("TraceCodeSecurity"), null, exception);
                }
                return false;
            }
            return true;
        }

        protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            bool flag = false;
            XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; i++)
            {
                KeyIdentifierClauseEntry entry = this.keyIdentifierClauseEntries[i];
                if (entry.SupportsCore(keyIdentifierClause))
                {
                    try
                    {
                        entry.WriteKeyIdentifierClauseCore(writer2, keyIdentifierClause);
                    }
                    catch (Exception exception)
                    {
                        if (!this.ShouldWrapException(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorSerializingKeyIdentifierClause"), exception));
                    }
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StandardsManagerCannotWriteObject", new object[] { keyIdentifierClause.GetType() })));
            }
            writer2.Flush();
        }

        protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            bool flag = false;
            XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            for (int i = 0; i < this.keyIdentifierEntries.Count; i++)
            {
                KeyIdentifierEntry entry = this.keyIdentifierEntries[i];
                if (entry.SupportsCore(keyIdentifier))
                {
                    try
                    {
                        entry.WriteKeyIdentifierCore(writer2, keyIdentifier);
                    }
                    catch (Exception exception)
                    {
                        if (!this.ShouldWrapException(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorSerializingKeyIdentifier"), exception));
                    }
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StandardsManagerCannotWriteObject", new object[] { keyIdentifier.GetType() })));
            }
            writer2.Flush();
        }

        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            bool flag = false;
            XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            if (token.GetType() == typeof(ProviderBackedSecurityToken))
            {
                token = (token as ProviderBackedSecurityToken).Token;
            }
            for (int i = 0; i < this.tokenEntries.Count; i++)
            {
                TokenEntry entry = this.tokenEntries[i];
                if (entry.SupportsCore(token.GetType()))
                {
                    try
                    {
                        entry.WriteTokenCore(writer2, token);
                    }
                    catch (Exception exception)
                    {
                        if (!this.ShouldWrapException(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorSerializingSecurityToken"), exception));
                    }
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StandardsManagerCannotWriteObject", new object[] { token.GetType() })));
            }
            writer2.Flush();
        }

        public static WSSecurityTokenSerializer DefaultInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WSSecurityTokenSerializer();
                }
                return instance;
            }
        }

        public bool EmitBspRequiredAttributes
        {
            get
            {
                return this.emitBspRequiredAttributes;
            }
        }

        public int MaximumKeyDerivationLabelLength
        {
            get
            {
                return this.maximumKeyDerivationLabelLength;
            }
        }

        public int MaximumKeyDerivationNonceLength
        {
            get
            {
                return this.maximumKeyDerivationNonceLength;
            }
        }

        public int MaximumKeyDerivationOffset
        {
            get
            {
                return this.maximumKeyDerivationOffset;
            }
        }

        internal WSSecureConversation SecureConversation
        {
            get
            {
                return this.secureConversation;
            }
        }

        public System.ServiceModel.Security.SecurityVersion SecurityVersion
        {
            get
            {
                return this.securityVersion;
            }
        }

        internal abstract class KeyIdentifierClauseEntry
        {
            protected KeyIdentifierClauseEntry()
            {
            }

            public virtual bool CanReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(this.LocalName, this.NamespaceUri);
            }

            public abstract SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader);
            public abstract bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause);
            public abstract void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause);

            protected abstract XmlDictionaryString LocalName { get; }

            protected abstract XmlDictionaryString NamespaceUri { get; }
        }

        internal abstract class KeyIdentifierEntry
        {
            protected KeyIdentifierEntry()
            {
            }

            public virtual bool CanReadKeyIdentifierCore(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(this.LocalName, this.NamespaceUri);
            }

            public abstract SecurityKeyIdentifier ReadKeyIdentifierCore(XmlDictionaryReader reader);
            public abstract bool SupportsCore(SecurityKeyIdentifier keyIdentifier);
            public abstract void WriteKeyIdentifierCore(XmlDictionaryWriter writer, SecurityKeyIdentifier keyIdentifier);

            protected abstract XmlDictionaryString LocalName { get; }

            protected abstract XmlDictionaryString NamespaceUri { get; }
        }

        internal abstract class SerializerEntries
        {
            protected SerializerEntries()
            {
            }

            public virtual void PopulateKeyIdentifierClauseEntries(IList<WSSecurityTokenSerializer.KeyIdentifierClauseEntry> keyIdentifierClauseEntries)
            {
            }

            public virtual void PopulateKeyIdentifierEntries(IList<WSSecurityTokenSerializer.KeyIdentifierEntry> keyIdentifierEntries)
            {
            }

            public virtual void PopulateStrEntries(IList<WSSecurityTokenSerializer.StrEntry> strEntries)
            {
            }

            public virtual void PopulateTokenEntries(IList<WSSecurityTokenSerializer.TokenEntry> tokenEntries)
            {
            }
        }

        internal abstract class StrEntry
        {
            protected StrEntry()
            {
            }

            public abstract bool CanReadClause(XmlDictionaryReader reader, string tokenType);
            public abstract Type GetTokenType(SecurityKeyIdentifierClause clause);
            public abstract string GetTokenTypeUri();
            public abstract SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType);
            public abstract bool SupportsCore(SecurityKeyIdentifierClause clause);
            public abstract void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause);
        }

        internal abstract class TokenEntry
        {
            private Type[] tokenTypes;

            protected TokenEntry()
            {
            }

            public virtual IAsyncResult BeginReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult<SecurityToken>(this.ReadTokenCore(reader, tokenResolver), callback, state);
            }

            public virtual bool CanReadTokenCore(XmlDictionaryReader reader)
            {
                return (reader.IsStartElement(this.LocalName, this.NamespaceUri) && (reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null) == this.ValueTypeUri));
            }

            public virtual bool CanReadTokenCore(XmlElement element)
            {
                string attribute = null;
                if (element.HasAttribute("ValueType", null))
                {
                    attribute = element.GetAttribute("ValueType", null);
                }
                return (((element.LocalName == this.LocalName.Value) && (element.NamespaceURI == this.NamespaceUri.Value)) && (attribute == this.ValueTypeUri));
            }

            protected static SecurityKeyIdentifierClause CreateDirectReference(XmlElement issuedTokenXml, string idAttributeLocalName, string idAttributeNamespace, Type tokenType)
            {
                string attribute = issuedTokenXml.GetAttribute(idAttributeLocalName, idAttributeNamespace);
                if (attribute == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { idAttributeLocalName, issuedTokenXml.LocalName })));
                }
                return new LocalIdKeyIdentifierClause(attribute, tokenType);
            }

            public abstract SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle);
            public virtual SecurityToken EndReadTokenCore(IAsyncResult result)
            {
                return CompletedAsyncResult<SecurityToken>.End(result);
            }

            public Type[] GetTokenTypes()
            {
                if (this.tokenTypes == null)
                {
                    this.tokenTypes = this.GetTokenTypesCore();
                }
                return this.tokenTypes;
            }

            protected abstract Type[] GetTokenTypesCore();
            public abstract SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver);
            public bool SupportsCore(Type tokenType)
            {
                Type[] tokenTypes = this.GetTokenTypes();
                for (int i = 0; i < tokenTypes.Length; i++)
                {
                    if (tokenTypes[i].IsAssignableFrom(tokenType))
                    {
                        return true;
                    }
                }
                return false;
            }

            public virtual bool SupportsTokenTypeUri(string tokenTypeUri)
            {
                return (this.TokenTypeUri == tokenTypeUri);
            }

            public abstract void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token);

            protected abstract XmlDictionaryString LocalName { get; }

            protected abstract XmlDictionaryString NamespaceUri { get; }

            public Type TokenType
            {
                get
                {
                    return this.GetTokenTypes()[0];
                }
            }

            public abstract string TokenTypeUri { get; }

            protected abstract string ValueTypeUri { get; }
        }
    }
}

