namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class RequestSecurityTokenResponse : BodyWriter
    {
        private object appliesTo;
        private XmlObjectSerializer appliesToSerializer;
        private System.Type appliesToType;
        private byte[] authenticator;
        private byte[] cachedWriteBuffer;
        private int cachedWriteBufferLength;
        private bool computeKey;
        private string context;
        private DateTime effectiveTime;
        private SecurityToken entropyToken;
        private DateTime expirationTime;
        private bool isLifetimeSet;
        private bool isReadOnly;
        private bool isReceiver;
        private bool isRequestedTokenClosed;
        private SecurityToken issuedToken;
        private XmlBuffer issuedTokenBuffer;
        private int keySize;
        private static int maxSaneKeySizeInBits = 0x20000;
        private static int minSaneKeySizeInBits = 0x40;
        private BinaryNegotiation negotiationData;
        private SecurityToken proofToken;
        private SecurityKeyIdentifierClause requestedAttachedReference;
        private SecurityKeyIdentifierClause requestedUnattachedReference;
        private XmlElement rstrXml;
        private SecurityStandardsManager standardsManager;
        private object thisLock;
        private string tokenType;

        public RequestSecurityTokenResponse() : this(SecurityStandardsManager.DefaultInstance)
        {
        }

        internal RequestSecurityTokenResponse(SecurityStandardsManager standardsManager) : base(true)
        {
            this.thisLock = new object();
            if (standardsManager == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            this.effectiveTime = System.ServiceModel.Security.SecurityUtils.MinUtcDateTime;
            this.expirationTime = System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime;
            this.isRequestedTokenClosed = false;
            this.isLifetimeSet = false;
            this.isReceiver = false;
            this.isReadOnly = false;
        }

        public RequestSecurityTokenResponse(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer) : this(System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer))
        {
        }

        public RequestSecurityTokenResponse(XmlElement requestSecurityTokenResponseXml, string context, string tokenType, int keySize, SecurityKeyIdentifierClause requestedAttachedReference, SecurityKeyIdentifierClause requestedUnattachedReference, bool computeKey, DateTime validFrom, DateTime validTo, bool isRequestedTokenClosed) : this(SecurityStandardsManager.DefaultInstance, requestSecurityTokenResponseXml, context, tokenType, keySize, requestedAttachedReference, requestedUnattachedReference, computeKey, validFrom, validTo, isRequestedTokenClosed, null)
        {
        }

        public RequestSecurityTokenResponse(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer, XmlElement requestSecurityTokenResponseXml, string context, string tokenType, int keySize, SecurityKeyIdentifierClause requestedAttachedReference, SecurityKeyIdentifierClause requestedUnattachedReference, bool computeKey, DateTime validFrom, DateTime validTo, bool isRequestedTokenClosed) : this(System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer), requestSecurityTokenResponseXml, context, tokenType, keySize, requestedAttachedReference, requestedUnattachedReference, computeKey, validFrom, validTo, isRequestedTokenClosed, null)
        {
        }

        internal RequestSecurityTokenResponse(SecurityStandardsManager standardsManager, XmlElement rstrXml, string context, string tokenType, int keySize, SecurityKeyIdentifierClause requestedAttachedReference, SecurityKeyIdentifierClause requestedUnattachedReference, bool computeKey, DateTime validFrom, DateTime validTo, bool isRequestedTokenClosed, XmlBuffer issuedTokenBuffer) : base(true)
        {
            this.thisLock = new object();
            if (standardsManager == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            if (rstrXml == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstrXml");
            }
            this.rstrXml = rstrXml;
            this.context = context;
            this.tokenType = tokenType;
            this.keySize = keySize;
            this.requestedAttachedReference = requestedAttachedReference;
            this.requestedUnattachedReference = requestedUnattachedReference;
            this.computeKey = computeKey;
            this.effectiveTime = validFrom.ToUniversalTime();
            this.expirationTime = validTo.ToUniversalTime();
            this.isLifetimeSet = true;
            this.isRequestedTokenClosed = isRequestedTokenClosed;
            this.issuedTokenBuffer = issuedTokenBuffer;
            this.isReceiver = true;
            this.isReadOnly = true;
        }

        public static byte[] ComputeCombinedKey(byte[] requestorEntropy, byte[] issuerEntropy, int keySizeInBits)
        {
            if (requestorEntropy == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestorEntropy");
            }
            if (issuerEntropy == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerEntropy");
            }
            if ((keySizeInBits < minSaneKeySizeInBits) || (keySizeInBits > maxSaneKeySizeInBits))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidKeySizeSpecifiedInNegotiation", new object[] { keySizeInBits, minSaneKeySizeInBits, maxSaneKeySizeInBits })));
            }
            Psha1DerivedKeyGenerator generator = new Psha1DerivedKeyGenerator(requestorEntropy);
            return generator.GenerateDerivedKey(new byte[0], issuerEntropy, keySizeInBits, 0);
        }

        public static RequestSecurityTokenResponse CreateFrom(XmlReader reader)
        {
            return CreateFrom(SecurityStandardsManager.DefaultInstance, reader);
        }

        internal static RequestSecurityTokenResponse CreateFrom(SecurityStandardsManager standardsManager, XmlReader reader)
        {
            return standardsManager.TrustDriver.CreateRequestSecurityTokenResponse(reader);
        }

        public static RequestSecurityTokenResponse CreateFrom(XmlReader reader, MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
        {
            return CreateFrom(System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer), reader);
        }

        public T GetAppliesTo<T>()
        {
            return this.GetAppliesTo<T>(DataContractSerializerDefaults.CreateSerializer(typeof(T), 0x10000));
        }

        public T GetAppliesTo<T>(XmlObjectSerializer serializer)
        {
            if (!this.isReceiver)
            {
                return (T) this.appliesTo;
            }
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            return this.standardsManager.TrustDriver.GetAppliesTo<T>(this, serializer);
        }

        public void GetAppliesToQName(out string localName, out string namespaceUri)
        {
            if (!this.isReceiver)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemAvailableInDeserializedRSTOnly", new object[] { "MatchesAppliesTo" })));
            }
            this.standardsManager.TrustDriver.GetAppliesToQName(this, out localName, out namespaceUri);
        }

        internal byte[] GetAuthenticator()
        {
            if (this.isReceiver)
            {
                return this.standardsManager.TrustDriver.GetAuthenticator(this);
            }
            if (this.authenticator == null)
            {
                return null;
            }
            byte[] dst = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(this.authenticator.Length);
            Buffer.BlockCopy(this.authenticator, 0, dst, 0, this.authenticator.Length);
            return dst;
        }

        internal BinaryNegotiation GetBinaryNegotiation()
        {
            if (this.isReceiver)
            {
                return this.standardsManager.TrustDriver.GetBinaryNegotiation(this);
            }
            return this.negotiationData;
        }

        public virtual GenericXmlSecurityToken GetIssuedToken(string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, RSA clientKey)
        {
            if (!this.isReceiver)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemAvailableInDeserializedRSTROnly", new object[] { "GetIssuedToken" })));
            }
            return this.standardsManager.TrustDriver.GetIssuedToken(this, expectedTokenType, authorizationPolicies, clientKey);
        }

        public GenericXmlSecurityToken GetIssuedToken(SecurityTokenResolver resolver, IList<SecurityTokenAuthenticator> allowedAuthenticators, SecurityKeyEntropyMode keyEntropyMode, byte[] requestorEntropy, string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            return this.GetIssuedToken(resolver, allowedAuthenticators, keyEntropyMode, requestorEntropy, expectedTokenType, authorizationPolicies, 0, false);
        }

        public virtual GenericXmlSecurityToken GetIssuedToken(SecurityTokenResolver resolver, IList<SecurityTokenAuthenticator> allowedAuthenticators, SecurityKeyEntropyMode keyEntropyMode, byte[] requestorEntropy, string expectedTokenType, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, int defaultKeySize, bool isBearerKeyType)
        {
            if (!this.isReceiver)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemAvailableInDeserializedRSTROnly", new object[] { "GetIssuedToken" })));
            }
            return this.standardsManager.TrustDriver.GetIssuedToken(this, resolver, allowedAuthenticators, keyEntropyMode, requestorEntropy, expectedTokenType, authorizationPolicies, defaultKeySize, isBearerKeyType);
        }

        public SecurityToken GetIssuerEntropy()
        {
            return this.GetIssuerEntropy(null);
        }

        internal SecurityToken GetIssuerEntropy(SecurityTokenResolver resolver)
        {
            if (this.isReceiver)
            {
                return this.standardsManager.TrustDriver.GetEntropy(this, resolver);
            }
            return this.entropyToken;
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.isReadOnly = true;
                this.OnMakeReadOnly();
            }
        }

        protected virtual void OnMakeReadOnly()
        {
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.WriteTo(writer);
        }

        protected internal virtual void OnWriteCustomAttributes(XmlWriter writer)
        {
        }

        protected internal virtual void OnWriteCustomElements(XmlWriter writer)
        {
        }

        private void OnWriteTo(XmlWriter w)
        {
            if (this.isReceiver)
            {
                this.rstrXml.WriteTo(w);
            }
            else
            {
                this.standardsManager.TrustDriver.WriteRequestSecurityTokenResponse(this, w);
            }
        }

        public void SetAppliesTo<T>(T appliesTo, XmlObjectSerializer serializer)
        {
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            if ((appliesTo != null) && (serializer == null))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            this.appliesTo = appliesTo;
            this.appliesToSerializer = serializer;
            this.appliesToType = typeof(T);
        }

        internal void SetAuthenticator(byte[] authenticator)
        {
            if (authenticator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authenticator");
            }
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            this.authenticator = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(authenticator.Length);
            Buffer.BlockCopy(authenticator, 0, this.authenticator, 0, authenticator.Length);
        }

        internal void SetBinaryNegotiation(BinaryNegotiation negotiation)
        {
            if (negotiation == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("negotiation");
            }
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            this.negotiationData = negotiation;
        }

        public void SetIssuerEntropy(byte[] issuerEntropy)
        {
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            this.entropyToken = (issuerEntropy != null) ? new NonceToken(issuerEntropy) : null;
        }

        internal void SetIssuerEntropy(WrappedKeySecurityToken issuerEntropy)
        {
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            this.entropyToken = issuerEntropy;
        }

        public void SetLifetime(DateTime validFrom, DateTime validTo)
        {
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            if (validFrom.ToUniversalTime() > validTo.ToUniversalTime())
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("EffectiveGreaterThanExpiration"));
            }
            this.effectiveTime = validFrom.ToUniversalTime();
            this.expirationTime = validTo.ToUniversalTime();
            this.isLifetimeSet = true;
        }

        public void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (this.IsReadOnly)
            {
                if (this.cachedWriteBuffer == null)
                {
                    MemoryStream stream = new MemoryStream();
                    using (XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateBinaryWriter(stream, System.ServiceModel.XD.Dictionary))
                    {
                        this.OnWriteTo(writer2);
                        writer2.Flush();
                        stream.Flush();
                        stream.Seek(0L, SeekOrigin.Begin);
                        this.cachedWriteBuffer = stream.GetBuffer();
                        this.cachedWriteBufferLength = (int) stream.Length;
                    }
                }
                writer.WriteNode(XmlDictionaryReader.CreateBinaryReader(this.cachedWriteBuffer, 0, this.cachedWriteBufferLength, System.ServiceModel.XD.Dictionary, XmlDictionaryReaderQuotas.Max), false);
            }
            else
            {
                this.OnWriteTo(writer);
            }
        }

        internal object AppliesTo
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRST", new object[] { "AppliesTo" })));
                }
                return this.appliesTo;
            }
        }

        internal XmlObjectSerializer AppliesToSerializer
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRST", new object[] { "AppliesToSerializer" })));
                }
                return this.appliesToSerializer;
            }
        }

        internal System.Type AppliesToType
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRST", new object[] { "AppliesToType" })));
                }
                return this.appliesToType;
            }
        }

        public bool ComputeKey
        {
            get
            {
                return this.computeKey;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.computeKey = value;
            }
        }

        public string Context
        {
            get
            {
                return this.context;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.context = value;
            }
        }

        public SecurityToken EntropyToken
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRSTR", new object[] { "EntropyToken" })));
                }
                return this.entropyToken;
            }
        }

        internal bool IsLifetimeSet
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRSTR", new object[] { "IsLifetimeSet" })));
                }
                return this.isLifetimeSet;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        internal bool IsReceiver
        {
            get
            {
                return this.isReceiver;
            }
        }

        public bool IsRequestedTokenClosed
        {
            get
            {
                return this.isRequestedTokenClosed;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.isRequestedTokenClosed = value;
            }
        }

        internal XmlBuffer IssuedTokenBuffer
        {
            get
            {
                return this.issuedTokenBuffer;
            }
        }

        public int KeySize
        {
            get
            {
                return this.keySize;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (value < 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.keySize = value;
            }
        }

        public SecurityKeyIdentifierClause RequestedAttachedReference
        {
            get
            {
                return this.requestedAttachedReference;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.requestedAttachedReference = value;
            }
        }

        public SecurityToken RequestedProofToken
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRSTR", new object[] { "ProofToken" })));
                }
                return this.proofToken;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.proofToken = value;
            }
        }

        public SecurityToken RequestedSecurityToken
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRSTR", new object[] { "IssuedToken" })));
                }
                return this.issuedToken;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.issuedToken = value;
            }
        }

        public SecurityKeyIdentifierClause RequestedUnattachedReference
        {
            get
            {
                return this.requestedUnattachedReference;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.requestedUnattachedReference = value;
            }
        }

        public XmlElement RequestSecurityTokenResponseXml
        {
            get
            {
                if (!this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemAvailableInDeserializedRSTROnly", new object[] { "RequestSecurityTokenXml" })));
                }
                return this.rstrXml;
            }
        }

        internal SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.standardsManager = (value != null) ? value : SecurityStandardsManager.DefaultInstance;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public string TokenType
        {
            get
            {
                return this.tokenType;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.tokenType = value;
            }
        }

        public DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public DateTime ValidTo
        {
            get
            {
                return this.expirationTime;
            }
        }
    }
}

