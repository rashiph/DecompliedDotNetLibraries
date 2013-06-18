namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    public class HttpsTransportBindingElement : HttpTransportBindingElement, ITransportTokenAssertionProvider
    {
        private System.ServiceModel.MessageSecurityVersion messageSecurityVersion;
        private bool requireClientCertificate;

        public HttpsTransportBindingElement()
        {
            this.requireClientCertificate = false;
        }

        protected HttpsTransportBindingElement(HttpsTransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.requireClientCertificate = elementToBeCloned.requireClientCertificate;
            this.messageSecurityVersion = elementToBeCloned.messageSecurityVersion;
        }

        private HttpsTransportBindingElement(HttpTransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            return (IChannelFactory<TChannel>) new HttpsChannelFactory(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (typeof(TChannel) != typeof(IReplyChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            HttpChannelListener listener = new HttpsChannelListener(this, context);
            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            return (IChannelListener<TChannel>) listener;
        }

        public override BindingElement Clone()
        {
            return new HttpsTransportBindingElement(this);
        }

        internal static HttpsTransportBindingElement CreateFromHttpBindingElement(HttpTransportBindingElement elementToBeCloned)
        {
            return new HttpsTransportBindingElement(elementToBeCloned);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) new SecurityCapabilities(this.SupportsClientAuthenticationImpl, true, this.SupportsClientWindowsIdentityImpl, ProtectionLevel.EncryptAndSign, ProtectionLevel.EncryptAndSign);
            }
            return base.GetProperty<T>(context);
        }

        public XmlElement GetTransportTokenAssertion()
        {
            return null;
        }

        internal override void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            base.OnExportPolicy(exporter, context);
            SecurityBindingElement.ExportPolicyForTransportTokenAssertionProviders(exporter, context);
        }

        internal override void OnImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            base.OnImportPolicy(importer, policyContext);
            WSSecurityPolicy securityPolicy = null;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out securityPolicy))
            {
                securityPolicy.TryImportWsspHttpsTokenAssertion(importer, policyContext.GetBindingAssertions(), this);
            }
        }

        internal System.ServiceModel.MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return this.messageSecurityVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.messageSecurityVersion = value;
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

        public override string Scheme
        {
            get
            {
                return "https";
            }
        }

        internal override bool SupportsClientAuthenticationImpl
        {
            get
            {
                if (!this.requireClientCertificate)
                {
                    return base.SupportsClientAuthenticationImpl;
                }
                return true;
            }
        }

        internal override bool SupportsClientWindowsIdentityImpl
        {
            get
            {
                if (!this.requireClientCertificate)
                {
                    return base.SupportsClientWindowsIdentityImpl;
                }
                return true;
            }
        }

        internal override string WsdlTransportUri
        {
            get
            {
                return "http://schemas.xmlsoap.org/soap/http";
            }
        }
    }
}

