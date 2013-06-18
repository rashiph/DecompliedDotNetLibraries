namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    public class SslStreamSecurityBindingElement : StreamUpgradeBindingElement, ITransportTokenAssertionProvider, IPolicyExportExtension
    {
        private System.ServiceModel.Security.IdentityVerifier identityVerifier;
        private bool requireClientCertificate;

        public SslStreamSecurityBindingElement()
        {
            this.requireClientCertificate = false;
        }

        protected SslStreamSecurityBindingElement(SslStreamSecurityBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.identityVerifier = elementToBeCloned.identityVerifier;
            this.requireClientCertificate = elementToBeCloned.requireClientCertificate;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override StreamUpgradeProvider BuildClientStreamUpgradeProvider(BindingContext context)
        {
            return SslStreamSecurityUpgradeProvider.CreateClientProvider(this, context);
        }

        public override StreamUpgradeProvider BuildServerStreamUpgradeProvider(BindingContext context)
        {
            return SslStreamSecurityUpgradeProvider.CreateServerProvider(this, context);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new SslStreamSecurityBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) new SecurityCapabilities(this.RequireClientCertificate, true, this.RequireClientCertificate, ProtectionLevel.EncryptAndSign, ProtectionLevel.EncryptAndSign);
            }
            if (typeof(T) == typeof(System.ServiceModel.Security.IdentityVerifier))
            {
                return (T) this.IdentityVerifier;
            }
            return context.GetInnerProperty<T>();
        }

        public XmlElement GetTransportTokenAssertion()
        {
            XmlDocument document = new XmlDocument();
            XmlElement element = document.CreateElement("msf", "SslTransportSecurity", "http://schemas.microsoft.com/ws/2006/05/framing/policy");
            if (this.requireClientCertificate)
            {
                element.AppendChild(document.CreateElement("msf", "RequireClientCertificate", "http://schemas.microsoft.com/ws/2006/05/framing/policy"));
            }
            return element;
        }

        internal static void ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            XmlElement node = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "SslTransportSecurity", "http://schemas.microsoft.com/ws/2006/05/framing/policy", true);
            if (node != null)
            {
                SslStreamSecurityBindingElement item = new SslStreamSecurityBindingElement();
                XmlReader reader = new XmlNodeReader(node);
                reader.ReadStartElement();
                item.RequireClientCertificate = reader.IsStartElement("RequireClientCertificate", "http://schemas.microsoft.com/ws/2006/05/framing/policy");
                if (item.RequireClientCertificate)
                {
                    reader.ReadElementString();
                }
                policyContext.BindingElements.Add(item);
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            SslStreamSecurityBindingElement element = b as SslStreamSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            return (this.requireClientCertificate == element.requireClientCertificate);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeIdentityVerifier()
        {
            return !object.ReferenceEquals(this.IdentityVerifier, System.ServiceModel.Security.IdentityVerifier.CreateDefault());
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            SecurityBindingElement.ExportPolicyForTransportTokenAssertionProviders(exporter, context);
        }

        public System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                if (this.identityVerifier == null)
                {
                    this.identityVerifier = System.ServiceModel.Security.IdentityVerifier.CreateDefault();
                }
                return this.identityVerifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.identityVerifier = value;
            }
        }

        [DefaultValue(false)]
        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
            set
            {
                this.requireClientCertificate = value;
            }
        }
    }
}

