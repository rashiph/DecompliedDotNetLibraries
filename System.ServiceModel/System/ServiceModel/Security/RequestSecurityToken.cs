namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class RequestSecurityToken : BodyWriter
    {
        private object appliesTo;
        private DataContractSerializer appliesToSerializer;
        private System.Type appliesToType;
        private byte[] cachedWriteBuffer;
        private int cachedWriteBufferLength;
        private SecurityKeyIdentifierClause closeTarget;
        private string context;
        private SecurityToken entropyToken;
        private bool isReadOnly;
        private bool isReceiver;
        private int keySize;
        private System.ServiceModel.Channels.Message message;
        private BinaryNegotiation negotiationData;
        private OnGetBinaryNegotiationCallback onGetBinaryNegotiation;
        private SecurityKeyIdentifierClause renewTarget;
        private IList<XmlElement> requestProperties;
        private string requestType;
        private XmlElement rstXml;
        private SecurityStandardsManager standardsManager;
        private object thisLock;
        private string tokenType;

        public RequestSecurityToken() : this(SecurityStandardsManager.DefaultInstance)
        {
        }

        internal RequestSecurityToken(SecurityStandardsManager standardsManager) : this(standardsManager, true)
        {
        }

        public RequestSecurityToken(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer) : this(System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer))
        {
        }

        internal RequestSecurityToken(SecurityStandardsManager standardsManager, bool isBuffered) : base(isBuffered)
        {
            this.thisLock = new object();
            if (standardsManager == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            this.requestType = this.standardsManager.TrustDriver.RequestTypeIssue;
            this.requestProperties = null;
            this.isReceiver = false;
            this.isReadOnly = false;
        }

        public RequestSecurityToken(XmlElement requestSecurityTokenXml, string context, string tokenType, string requestType, int keySize, SecurityKeyIdentifierClause renewTarget, SecurityKeyIdentifierClause closeTarget) : this(SecurityStandardsManager.DefaultInstance, requestSecurityTokenXml, context, tokenType, requestType, keySize, renewTarget, closeTarget)
        {
        }

        internal RequestSecurityToken(SecurityStandardsManager standardsManager, XmlElement rstXml, string context, string tokenType, string requestType, int keySize, SecurityKeyIdentifierClause renewTarget, SecurityKeyIdentifierClause closeTarget) : base(true)
        {
            this.thisLock = new object();
            if (standardsManager == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            if (rstXml == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstXml");
            }
            this.rstXml = rstXml;
            this.context = context;
            this.tokenType = tokenType;
            this.keySize = keySize;
            this.requestType = requestType;
            this.renewTarget = renewTarget;
            this.closeTarget = closeTarget;
            this.isReceiver = true;
            this.isReadOnly = true;
        }

        public RequestSecurityToken(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer, XmlElement requestSecurityTokenXml, string context, string tokenType, string requestType, int keySize, SecurityKeyIdentifierClause renewTarget, SecurityKeyIdentifierClause closeTarget) : this(System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer), requestSecurityTokenXml, context, tokenType, requestType, keySize, renewTarget, closeTarget)
        {
        }

        public static RequestSecurityToken CreateFrom(XmlReader reader)
        {
            return CreateFrom(SecurityStandardsManager.DefaultInstance, reader);
        }

        internal static RequestSecurityToken CreateFrom(SecurityStandardsManager standardsManager, XmlReader reader)
        {
            return standardsManager.TrustDriver.CreateRequestSecurityToken(reader);
        }

        public static RequestSecurityToken CreateFrom(XmlReader reader, MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
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

        internal BinaryNegotiation GetBinaryNegotiation()
        {
            if (this.isReceiver)
            {
                return this.standardsManager.TrustDriver.GetBinaryNegotiation(this);
            }
            if ((this.negotiationData == null) && (this.onGetBinaryNegotiation != null))
            {
                this.onGetBinaryNegotiation(this.GetChannelBinding());
            }
            return this.negotiationData;
        }

        public ChannelBinding GetChannelBinding()
        {
            if (this.message == null)
            {
                return null;
            }
            ChannelBindingMessageProperty property = null;
            ChannelBindingMessageProperty.TryGet(this.message, out property);
            ChannelBinding channelBinding = null;
            if (property != null)
            {
                channelBinding = property.ChannelBinding;
            }
            return channelBinding;
        }

        public SecurityToken GetRequestorEntropy()
        {
            return this.GetRequestorEntropy(null);
        }

        internal SecurityToken GetRequestorEntropy(SecurityTokenResolver resolver)
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
                if (this.requestProperties != null)
                {
                    this.requestProperties = new ReadOnlyCollection<XmlElement>(this.requestProperties);
                }
                this.OnMakeReadOnly();
            }
        }

        protected internal virtual void OnMakeReadOnly()
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

        private void OnWriteTo(XmlWriter writer)
        {
            if (this.isReceiver)
            {
                this.rstXml.WriteTo(writer);
            }
            else
            {
                this.standardsManager.TrustDriver.WriteRequestSecurityToken(this, writer);
            }
        }

        public void SetAppliesTo<T>(T appliesTo, DataContractSerializer serializer)
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

        public void SetRequestorEntropy(byte[] entropy)
        {
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            this.entropyToken = (entropy != null) ? new NonceToken(entropy) : null;
        }

        internal void SetRequestorEntropy(WrappedKeySecurityToken entropyToken)
        {
            if (this.IsReadOnly)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
            this.entropyToken = entropyToken;
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
                    using (XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateBinaryWriter(stream, XD.Dictionary))
                    {
                        this.OnWriteTo(writer2);
                        writer2.Flush();
                        stream.Flush();
                        stream.Seek(0L, SeekOrigin.Begin);
                        this.cachedWriteBuffer = stream.GetBuffer();
                        this.cachedWriteBufferLength = (int) stream.Length;
                    }
                }
                writer.WriteNode(XmlDictionaryReader.CreateBinaryReader(this.cachedWriteBuffer, 0, this.cachedWriteBufferLength, XD.Dictionary, XmlDictionaryReaderQuotas.Max), false);
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

        internal DataContractSerializer AppliesToSerializer
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

        public SecurityKeyIdentifierClause CloseTarget
        {
            get
            {
                return this.closeTarget;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.closeTarget = value;
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

        public System.ServiceModel.Channels.Message Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = value;
            }
        }

        public OnGetBinaryNegotiationCallback OnGetBinaryNegotiation
        {
            get
            {
                return this.onGetBinaryNegotiation;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.onGetBinaryNegotiation = value;
            }
        }

        public SecurityKeyIdentifierClause RenewTarget
        {
            get
            {
                return this.renewTarget;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.renewTarget = value;
            }
        }

        public IEnumerable<XmlElement> RequestProperties
        {
            get
            {
                if (this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemNotAvailableInDeserializedRST", new object[] { "RequestProperties" })));
                }
                return this.requestProperties;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (value != null)
                {
                    int num = 0;
                    Collection<XmlElement> collection = new Collection<XmlElement>();
                    foreach (XmlElement element in value)
                    {
                        if (element == null)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "value[{0}]", new object[] { num })));
                        }
                        collection.Add(element);
                        num++;
                    }
                    this.requestProperties = collection;
                }
                else
                {
                    this.requestProperties = null;
                }
            }
        }

        public XmlElement RequestSecurityTokenXml
        {
            get
            {
                if (!this.isReceiver)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ItemAvailableInDeserializedRSTOnly", new object[] { "RequestSecurityTokenXml" })));
                }
                return this.rstXml;
            }
        }

        public string RequestType
        {
            get
            {
                return this.requestType;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.requestType = value;
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
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.standardsManager = value;
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

        public delegate void OnGetBinaryNegotiationCallback(ChannelBinding channelBinding);
    }
}

