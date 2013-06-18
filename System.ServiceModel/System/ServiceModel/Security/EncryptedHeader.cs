namespace System.ServiceModel.Security
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal sealed class EncryptedHeader : DelegatingHeader
    {
        private EncryptedHeaderXml headerXml;
        private string name;
        private string namespaceUri;
        private MessageVersion version;

        public EncryptedHeader(MessageHeader plainTextHeader, EncryptedHeaderXml headerXml, string name, string namespaceUri, MessageVersion version) : base(plainTextHeader)
        {
            if (!headerXml.HasId || (headerXml.Id == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedHeaderXmlMustHaveId")));
            }
            this.headerXml = headerXml;
            this.name = name;
            this.namespaceUri = namespaceUri;
            this.version = version;
        }

        public override bool IsMessageVersionSupported(MessageVersion messageVersion)
        {
            return this.version.Equals(this.version);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.headerXml.WriteHeaderContents(writer);
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (!this.IsMessageVersionSupported(messageVersion))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessageHeaderVersionNotSupported", new object[] { string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { this.Namespace, this.Name }), this.version.ToString() }), "version"));
            }
            this.headerXml.WriteHeaderElement(writer);
            base.WriteHeaderAttributes(writer, messageVersion);
            this.headerXml.WriteHeaderId(writer);
        }

        public override string Actor
        {
            get
            {
                return this.headerXml.Actor;
            }
        }

        public string Id
        {
            get
            {
                return this.headerXml.Id;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.headerXml.MustUnderstand;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.namespaceUri;
            }
        }

        internal MessageHeader OriginalHeader
        {
            get
            {
                return base.InnerHeader;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.headerXml.Relay;
            }
        }
    }
}

