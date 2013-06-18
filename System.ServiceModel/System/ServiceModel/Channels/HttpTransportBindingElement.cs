namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.Xml;

    public class HttpTransportBindingElement : TransportBindingElement, IWsdlExportExtension, IPolicyExportExtension, ITransportPolicyImport
    {
        private bool allowCookies;
        private HttpAnonymousUriPrefixMatcher anonymousUriPrefixMatcher;
        private AuthenticationSchemes authenticationScheme;
        private bool bypassProxyOnLocal;
        private bool decompressionEnabled;
        private System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy extendedProtectionPolicy;
        private System.ServiceModel.HostNameComparisonMode hostNameComparisonMode;
        private bool inheritBaseAddressSettings;
        private bool keepAliveEnabled;
        private int maxBufferSize;
        private bool maxBufferSizeInitialized;
        private string method;
        private Uri proxyAddress;
        private AuthenticationSchemes proxyAuthenticationScheme;
        private string realm;
        private System.ServiceModel.TransferMode transferMode;
        private bool unsafeConnectionNtlmAuthentication;
        private bool useDefaultWebProxy;
        private IWebProxy webProxy;

        public HttpTransportBindingElement()
        {
            this.allowCookies = false;
            this.authenticationScheme = AuthenticationSchemes.Anonymous;
            this.bypassProxyOnLocal = false;
            this.decompressionEnabled = true;
            this.hostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
            this.keepAliveEnabled = true;
            this.maxBufferSize = 0x10000;
            this.method = string.Empty;
            this.proxyAuthenticationScheme = AuthenticationSchemes.Anonymous;
            this.proxyAddress = null;
            this.realm = "";
            this.transferMode = System.ServiceModel.TransferMode.Buffered;
            this.unsafeConnectionNtlmAuthentication = false;
            this.useDefaultWebProxy = true;
            this.webProxy = null;
            this.extendedProtectionPolicy = ChannelBindingUtility.DefaultPolicy;
        }

        protected HttpTransportBindingElement(HttpTransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.allowCookies = elementToBeCloned.allowCookies;
            this.authenticationScheme = elementToBeCloned.authenticationScheme;
            this.bypassProxyOnLocal = elementToBeCloned.bypassProxyOnLocal;
            this.decompressionEnabled = elementToBeCloned.decompressionEnabled;
            this.hostNameComparisonMode = elementToBeCloned.hostNameComparisonMode;
            this.inheritBaseAddressSettings = elementToBeCloned.InheritBaseAddressSettings;
            this.keepAliveEnabled = elementToBeCloned.keepAliveEnabled;
            this.maxBufferSize = elementToBeCloned.maxBufferSize;
            this.maxBufferSizeInitialized = elementToBeCloned.maxBufferSizeInitialized;
            this.method = elementToBeCloned.method;
            this.proxyAddress = elementToBeCloned.proxyAddress;
            this.proxyAuthenticationScheme = elementToBeCloned.proxyAuthenticationScheme;
            this.realm = elementToBeCloned.realm;
            this.transferMode = elementToBeCloned.transferMode;
            this.unsafeConnectionNtlmAuthentication = elementToBeCloned.unsafeConnectionNtlmAuthentication;
            this.useDefaultWebProxy = elementToBeCloned.useDefaultWebProxy;
            this.webProxy = elementToBeCloned.webProxy;
            this.extendedProtectionPolicy = elementToBeCloned.ExtendedProtectionPolicy;
            if (elementToBeCloned.anonymousUriPrefixMatcher != null)
            {
                this.anonymousUriPrefixMatcher = new HttpAnonymousUriPrefixMatcher(elementToBeCloned.anonymousUriPrefixMatcher);
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("CouldnTCreateChannelForChannelType2", new object[] { context.Binding.Name, typeof(TChannel) }));
            }
            return (IChannelFactory<TChannel>) new HttpChannelFactory(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("CouldnTCreateChannelForChannelType2", new object[] { context.Binding.Name, typeof(TChannel) }));
            }
            HttpChannelListener listener = new HttpChannelListener(this, context);
            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            return (IChannelListener<TChannel>) listener;
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return (typeof(TChannel) == typeof(IRequestChannel));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            return (typeof(TChannel) == typeof(IReplyChannel));
        }

        public override BindingElement Clone()
        {
            return new HttpTransportBindingElement(this);
        }

        private MessageEncodingBindingElement FindMessageEncodingBindingElement(BindingElementCollection bindingElements, out bool createdNew)
        {
            createdNew = false;
            MessageEncodingBindingElement element = bindingElements.Find<MessageEncodingBindingElement>();
            if (element == null)
            {
                createdNew = true;
                element = new TextMessageEncodingBindingElement();
            }
            return element;
        }

        private MessageEncodingBindingElement FindMessageEncodingBindingElement(WsdlEndpointConversionContext endpointContext, out bool createdNew)
        {
            BindingElementCollection bindingElements = endpointContext.Endpoint.Binding.CreateBindingElements();
            return this.FindMessageEncodingBindingElement(bindingElements, out createdNew);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) new SecurityCapabilities(this.SupportsClientAuthenticationImpl, this.AuthenticationScheme == AuthenticationSchemes.Negotiate, this.SupportsClientWindowsIdentityImpl, ProtectionLevel.None, ProtectionLevel.None);
            }
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T) new BindingDeliveryCapabilitiesHelper();
            }
            if (typeof(T) == typeof(System.ServiceModel.TransferMode))
            {
                return (T) this.TransferMode;
            }
            if (typeof(T) == typeof(System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy))
            {
                return (T) this.ExtendedProtectionPolicy;
            }
            if (typeof(T) == typeof(IAnonymousUriPrefixMatcher))
            {
                if (this.anonymousUriPrefixMatcher == null)
                {
                    this.anonymousUriPrefixMatcher = new HttpAnonymousUriPrefixMatcher();
                }
                return (T) this.anonymousUriPrefixMatcher;
            }
            if (context.BindingParameters.Find<MessageEncodingBindingElement>() == null)
            {
                context.BindingParameters.Add(new TextMessageEncodingBindingElement());
            }
            return base.GetProperty<T>(context);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
            {
                return false;
            }
            HttpTransportBindingElement element = b as HttpTransportBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.allowCookies != element.allowCookies)
            {
                return false;
            }
            if (this.authenticationScheme != element.authenticationScheme)
            {
                return false;
            }
            if (this.decompressionEnabled != element.decompressionEnabled)
            {
                return false;
            }
            if (this.hostNameComparisonMode != element.hostNameComparisonMode)
            {
                return false;
            }
            if (this.inheritBaseAddressSettings != element.inheritBaseAddressSettings)
            {
                return false;
            }
            if (this.keepAliveEnabled != element.keepAliveEnabled)
            {
                return false;
            }
            if (this.maxBufferSize != element.maxBufferSize)
            {
                return false;
            }
            if (this.method != element.method)
            {
                return false;
            }
            if (this.proxyAddress != element.proxyAddress)
            {
                return false;
            }
            if (this.proxyAuthenticationScheme != element.proxyAuthenticationScheme)
            {
                return false;
            }
            if (this.realm != element.realm)
            {
                return false;
            }
            if (this.transferMode != element.transferMode)
            {
                return false;
            }
            if (this.unsafeConnectionNtlmAuthentication != element.unsafeConnectionNtlmAuthentication)
            {
                return false;
            }
            if (this.useDefaultWebProxy != element.useDefaultWebProxy)
            {
                return false;
            }
            if (this.webProxy != element.webProxy)
            {
                return false;
            }
            if (!ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, element.ExtendedProtectionPolicy))
            {
                return false;
            }
            return true;
        }

        internal virtual void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            string localName = null;
            switch (this.AuthenticationScheme)
            {
                case AuthenticationSchemes.Digest:
                    localName = "DigestAuthentication";
                    break;

                case AuthenticationSchemes.Negotiate:
                    localName = "NegotiateAuthentication";
                    break;

                case AuthenticationSchemes.Ntlm:
                    localName = "NtlmAuthentication";
                    break;

                case AuthenticationSchemes.Basic:
                    localName = "BasicAuthentication";
                    break;
            }
            if (localName != null)
            {
                policyContext.GetBindingAssertions().Add(new XmlDocument().CreateElement("http", localName, "http://schemas.microsoft.com/ws/06/2004/policy/http"));
            }
        }

        internal virtual void OnImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeExtendedProtectionPolicy()
        {
            return !ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, ChannelBindingUtility.DefaultPolicy);
        }

        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            ICollection<XmlElement> bindingAssertions = policyContext.GetBindingAssertions();
            List<XmlElement> list = new List<XmlElement>();
            bool flag = false;
            foreach (XmlElement element in bindingAssertions)
            {
                string str;
                if ((element.NamespaceURI != "http://schemas.microsoft.com/ws/06/2004/policy/http") || ((str = element.LocalName) == null))
                {
                    continue;
                }
                if (!(str == "BasicAuthentication"))
                {
                    if (str == "DigestAuthentication")
                    {
                        goto Label_009A;
                    }
                    if (str == "NegotiateAuthentication")
                    {
                        goto Label_00A3;
                    }
                    if (str == "NtlmAuthentication")
                    {
                        goto Label_00AC;
                    }
                    continue;
                }
                this.AuthenticationScheme = AuthenticationSchemes.Basic;
                goto Label_00B3;
            Label_009A:
                this.AuthenticationScheme = AuthenticationSchemes.Digest;
                goto Label_00B3;
            Label_00A3:
                this.AuthenticationScheme = AuthenticationSchemes.Negotiate;
                goto Label_00B3;
            Label_00AC:
                this.AuthenticationScheme = AuthenticationSchemes.Ntlm;
            Label_00B3:
                if (flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("HttpTransportCannotHaveMultipleAuthenticationSchemes", new object[] { policyContext.Contract.Namespace, policyContext.Contract.Name })));
                }
                flag = true;
                list.Add(element);
            }
            list.ForEach(element => bindingAssertions.Remove(element));
            this.OnImportPolicy(importer, policyContext);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            bool flag;
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.OnExportPolicy(exporter, context);
            MessageEncodingBindingElement element = this.FindMessageEncodingBindingElement(context.BindingElements, out flag);
            if (flag && (element is IPolicyExportExtension))
            {
                ((IPolicyExportExtension) element).ExportPolicy(exporter, context);
            }
            WsdlExporter.WSAddressingHelper.AddWSAddressingAssertion(exporter, context, element.MessageVersion.Addressing);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            bool flag;
            MessageEncodingBindingElement element = this.FindMessageEncodingBindingElement(endpointContext, out flag);
            TransportBindingElement.ExportWsdlEndpoint(exporter, endpointContext, this.WsdlTransportUri, element.MessageVersion.Addressing);
        }

        [DefaultValue(false)]
        public bool AllowCookies
        {
            get
            {
                return this.allowCookies;
            }
            set
            {
                this.allowCookies = value;
            }
        }

        internal HttpAnonymousUriPrefixMatcher AnonymousUriPrefixMatcher
        {
            get
            {
                return this.anonymousUriPrefixMatcher;
            }
        }

        [DefaultValue(0x8000)]
        public AuthenticationSchemes AuthenticationScheme
        {
            get
            {
                return this.authenticationScheme;
            }
            set
            {
                if (!AuthenticationSchemesHelper.IsSingleton(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.ServiceModel.SR.GetString("HttpRequiresSingleAuthScheme", new object[] { value }));
                }
                this.authenticationScheme = value;
            }
        }

        [DefaultValue(false)]
        public bool BypassProxyOnLocal
        {
            get
            {
                return this.bypassProxyOnLocal;
            }
            set
            {
                this.bypassProxyOnLocal = value;
            }
        }

        [DefaultValue(true)]
        public bool DecompressionEnabled
        {
            get
            {
                return this.decompressionEnabled;
            }
            set
            {
                this.decompressionEnabled = value;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if ((value.PolicyEnforcement == PolicyEnforcement.Always) && !System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.OSSupportsExtendedProtection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PlatformNotSupportedException(System.ServiceModel.SR.GetString("ExtendedProtectionNotSupported")));
                }
                this.extendedProtectionPolicy = value;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
            set
            {
                HostNameComparisonModeHelper.Validate(value);
                this.hostNameComparisonMode = value;
            }
        }

        internal bool InheritBaseAddressSettings
        {
            get
            {
                return this.inheritBaseAddressSettings;
            }
            set
            {
                this.inheritBaseAddressSettings = value;
            }
        }

        [DefaultValue(true)]
        public bool KeepAliveEnabled
        {
            get
            {
                return this.keepAliveEnabled;
            }
            set
            {
                this.keepAliveEnabled = value;
            }
        }

        [DefaultValue(0x10000)]
        public int MaxBufferSize
        {
            get
            {
                if (this.maxBufferSizeInitialized || (this.TransferMode != System.ServiceModel.TransferMode.Buffered))
                {
                    return this.maxBufferSize;
                }
                long maxReceivedMessageSize = this.MaxReceivedMessageSize;
                if (maxReceivedMessageSize > 0x7fffffffL)
                {
                    return 0x7fffffff;
                }
                return (int) maxReceivedMessageSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxBufferSizeInitialized = true;
                this.maxBufferSize = value;
            }
        }

        internal string Method
        {
            get
            {
                return this.method;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.method = value;
            }
        }

        internal IWebProxy Proxy
        {
            get
            {
                return this.webProxy;
            }
            set
            {
                this.webProxy = value;
            }
        }

        [DefaultValue(null), TypeConverter(typeof(UriTypeConverter))]
        public Uri ProxyAddress
        {
            get
            {
                return this.proxyAddress;
            }
            set
            {
                this.proxyAddress = value;
            }
        }

        [DefaultValue(0x8000)]
        public AuthenticationSchemes ProxyAuthenticationScheme
        {
            get
            {
                return this.proxyAuthenticationScheme;
            }
            set
            {
                if (!AuthenticationSchemesHelper.IsSingleton(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.ServiceModel.SR.GetString("HttpProxyRequiresSingleAuthScheme", new object[] { value }));
                }
                this.proxyAuthenticationScheme = value;
            }
        }

        [DefaultValue("")]
        public string Realm
        {
            get
            {
                return this.realm;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.realm = value;
            }
        }

        public override string Scheme
        {
            get
            {
                return "http";
            }
        }

        internal virtual bool SupportsClientAuthenticationImpl
        {
            get
            {
                return (this.authenticationScheme != AuthenticationSchemes.Anonymous);
            }
        }

        internal virtual bool SupportsClientWindowsIdentityImpl
        {
            get
            {
                return (this.authenticationScheme != AuthenticationSchemes.Anonymous);
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.TransferMode TransferMode
        {
            get
            {
                return this.transferMode;
            }
            set
            {
                TransferModeHelper.Validate(value);
                this.transferMode = value;
            }
        }

        [DefaultValue(false)]
        public bool UnsafeConnectionNtlmAuthentication
        {
            get
            {
                return this.unsafeConnectionNtlmAuthentication;
            }
            set
            {
                this.unsafeConnectionNtlmAuthentication = value;
            }
        }

        [DefaultValue(true)]
        public bool UseDefaultWebProxy
        {
            get
            {
                return this.useDefaultWebProxy;
            }
            set
            {
                this.useDefaultWebProxy = value;
            }
        }

        internal virtual string WsdlTransportUri
        {
            get
            {
                return "http://schemas.xmlsoap.org/soap/http";
            }
        }

        private class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }

            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get
                {
                    return false;
                }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

