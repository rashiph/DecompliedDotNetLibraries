namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Text;
    using System.Xml;

    public class BasicHttpBinding : Binding, IBindingRuntimePreferences
    {
        private HttpsTransportBindingElement httpsTransport;
        private HttpTransportBindingElement httpTransport;
        private WSMessageEncoding messageEncoding;
        private MtomMessageEncodingBindingElement mtomEncoding;
        private BasicHttpSecurity security;
        private TextMessageEncodingBindingElement textEncoding;

        public BasicHttpBinding() : this(BasicHttpSecurityMode.None)
        {
        }

        private BasicHttpBinding(BasicHttpSecurity security)
        {
            this.security = new BasicHttpSecurity();
            this.Initialize();
            this.security = security;
        }

        public BasicHttpBinding(BasicHttpSecurityMode securityMode)
        {
            this.security = new BasicHttpSecurity();
            this.Initialize();
            this.security.Mode = securityMode;
        }

        public BasicHttpBinding(string configurationName) : this()
        {
            this.ApplyConfiguration(configurationName);
        }

        private void ApplyConfiguration(string configurationName)
        {
            BasicHttpBindingElement element2 = BasicHttpBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "basicHttpBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            SecurityBindingElement item = this.CreateMessageSecurity();
            if (item != null)
            {
                elements.Add(item);
            }
            WSMessageEncodingHelper.SyncUpEncodingBindingElementProperties(this.textEncoding, this.mtomEncoding);
            if (this.MessageEncoding == WSMessageEncoding.Text)
            {
                elements.Add(this.textEncoding);
            }
            else if (this.MessageEncoding == WSMessageEncoding.Mtom)
            {
                elements.Add(this.mtomEncoding);
            }
            elements.Add(this.GetTransport());
            return elements.Clone();
        }

        private SecurityBindingElement CreateMessageSecurity()
        {
            return this.security.CreateMessageSecurity();
        }

        private static bool GetSecurityModeFromTransport(HttpTransportBindingElement http, HttpTransportSecurity transportSecurity, out UnifiedSecurityMode mode)
        {
            mode = UnifiedSecurityMode.None;
            if (http == null)
            {
                return false;
            }
            if (http is HttpsTransportBindingElement)
            {
                mode = UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Transport;
                BasicHttpSecurity.EnableTransportSecurity((HttpsTransportBindingElement) http, transportSecurity);
            }
            else if (HttpTransportSecurity.IsDisabledTransportAuthentication(http))
            {
                mode = UnifiedSecurityMode.Message | UnifiedSecurityMode.None;
            }
            else
            {
                if (!BasicHttpSecurity.IsEnabledTransportAuthentication(http, transportSecurity))
                {
                    return false;
                }
                mode = UnifiedSecurityMode.TransportCredentialOnly;
            }
            return true;
        }

        private TransportBindingElement GetTransport()
        {
            if ((this.security.Mode == BasicHttpSecurityMode.Transport) || (this.security.Mode == BasicHttpSecurityMode.TransportWithMessageCredential))
            {
                this.security.EnableTransportSecurity(this.httpsTransport);
                return this.httpsTransport;
            }
            if (this.security.Mode == BasicHttpSecurityMode.TransportCredentialOnly)
            {
                this.security.EnableTransportAuthentication(this.httpTransport);
                return this.httpTransport;
            }
            this.security.DisableTransportAuthentication(this.httpTransport);
            return this.httpTransport;
        }

        private void Initialize()
        {
            this.httpTransport = new HttpTransportBindingElement();
            this.httpsTransport = new HttpsTransportBindingElement();
            this.messageEncoding = WSMessageEncoding.Text;
            this.textEncoding = new TextMessageEncodingBindingElement();
            this.textEncoding.MessageVersion = MessageVersion.Soap11;
            this.mtomEncoding = new MtomMessageEncodingBindingElement();
            this.mtomEncoding.MessageVersion = MessageVersion.Soap11;
        }

        private void InitializeFrom(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding)
        {
            this.BypassProxyOnLocal = transport.BypassProxyOnLocal;
            this.HostNameComparisonMode = transport.HostNameComparisonMode;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxBufferSize = transport.MaxBufferSize;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.ProxyAddress = transport.ProxyAddress;
            this.TransferMode = transport.TransferMode;
            this.UseDefaultWebProxy = transport.UseDefaultWebProxy;
            this.Security.Transport.ExtendedProtectionPolicy = transport.ExtendedProtectionPolicy;
            if (encoding is TextMessageEncodingBindingElement)
            {
                this.MessageEncoding = WSMessageEncoding.Text;
                TextMessageEncodingBindingElement element = (TextMessageEncodingBindingElement) encoding;
                this.TextEncoding = element.WriteEncoding;
                this.ReaderQuotas = element.ReaderQuotas;
            }
            else if (encoding is MtomMessageEncodingBindingElement)
            {
                this.messageEncoding = WSMessageEncoding.Mtom;
                MtomMessageEncodingBindingElement element2 = (MtomMessageEncodingBindingElement) encoding;
                this.TextEncoding = element2.WriteEncoding;
                this.ReaderQuotas = element2.ReaderQuotas;
            }
        }

        private bool IsBindingElementsMatch(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding)
        {
            if (this.MessageEncoding == WSMessageEncoding.Text)
            {
                if (!this.textEncoding.IsMatch(encoding))
                {
                    return false;
                }
            }
            else if ((this.MessageEncoding == WSMessageEncoding.Mtom) && !this.mtomEncoding.IsMatch(encoding))
            {
                return false;
            }
            if (!this.GetTransport().IsMatch(transport))
            {
                return false;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return !EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.security.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTextEncoding()
        {
            return !this.TextEncoding.Equals(BasicHttpBindingDefaults.TextEncoding);
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            UnifiedSecurityMode mode;
            BasicHttpSecurity security2;
            binding = null;
            if (elements.Count > 3)
            {
                return false;
            }
            SecurityBindingElement securityElement = null;
            MessageEncodingBindingElement encoding = null;
            HttpTransportBindingElement http = null;
            foreach (BindingElement element4 in elements)
            {
                if (element4 is SecurityBindingElement)
                {
                    securityElement = element4 as SecurityBindingElement;
                }
                else if (element4 is TransportBindingElement)
                {
                    http = element4 as HttpTransportBindingElement;
                }
                else if (element4 is MessageEncodingBindingElement)
                {
                    encoding = element4 as MessageEncodingBindingElement;
                }
                else
                {
                    return false;
                }
            }
            HttpTransportSecurity transportSecurity = new HttpTransportSecurity();
            if (!GetSecurityModeFromTransport(http, transportSecurity, out mode))
            {
                return false;
            }
            if (encoding == null)
            {
                return false;
            }
            if (!encoding.CheckEncodingVersion(System.ServiceModel.EnvelopeVersion.Soap11))
            {
                return false;
            }
            if (!TryCreateSecurity(securityElement, mode, transportSecurity, out security2))
            {
                return false;
            }
            BasicHttpBinding binding2 = new BasicHttpBinding(security2);
            binding2.InitializeFrom(http, encoding);
            if (!binding2.IsBindingElementsMatch(http, encoding))
            {
                return false;
            }
            binding = binding2;
            return true;
        }

        private static bool TryCreateSecurity(SecurityBindingElement securityElement, UnifiedSecurityMode mode, HttpTransportSecurity transportSecurity, out BasicHttpSecurity security)
        {
            return BasicHttpSecurity.TryCreate(securityElement, mode, transportSecurity, out security);
        }

        [DefaultValue(false)]
        public bool AllowCookies
        {
            get
            {
                return this.httpTransport.AllowCookies;
            }
            set
            {
                this.httpTransport.AllowCookies = value;
                this.httpsTransport.AllowCookies = value;
            }
        }

        [DefaultValue(false)]
        public bool BypassProxyOnLocal
        {
            get
            {
                return this.httpTransport.BypassProxyOnLocal;
            }
            set
            {
                this.httpTransport.BypassProxyOnLocal = value;
                this.httpsTransport.BypassProxyOnLocal = value;
            }
        }

        public System.ServiceModel.EnvelopeVersion EnvelopeVersion
        {
            get
            {
                return System.ServiceModel.EnvelopeVersion.Soap11;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.httpTransport.HostNameComparisonMode;
            }
            set
            {
                this.httpTransport.HostNameComparisonMode = value;
                this.httpsTransport.HostNameComparisonMode = value;
            }
        }

        [DefaultValue((long) 0x80000L)]
        public long MaxBufferPoolSize
        {
            get
            {
                return this.httpTransport.MaxBufferPoolSize;
            }
            set
            {
                this.httpTransport.MaxBufferPoolSize = value;
                this.httpsTransport.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(0x10000)]
        public int MaxBufferSize
        {
            get
            {
                return this.httpTransport.MaxBufferSize;
            }
            set
            {
                this.httpTransport.MaxBufferSize = value;
                this.httpsTransport.MaxBufferSize = value;
                this.mtomEncoding.MaxBufferSize = value;
            }
        }

        [DefaultValue((long) 0x10000L)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return this.httpTransport.MaxReceivedMessageSize;
            }
            set
            {
                this.httpTransport.MaxReceivedMessageSize = value;
                this.httpsTransport.MaxReceivedMessageSize = value;
            }
        }

        [DefaultValue(0)]
        public WSMessageEncoding MessageEncoding
        {
            get
            {
                return this.messageEncoding;
            }
            set
            {
                this.messageEncoding = value;
            }
        }

        [TypeConverter(typeof(UriTypeConverter)), DefaultValue(null)]
        public Uri ProxyAddress
        {
            get
            {
                return this.httpTransport.ProxyAddress;
            }
            set
            {
                this.httpTransport.ProxyAddress = value;
                this.httpsTransport.ProxyAddress = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.textEncoding.ReaderQuotas;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                value.CopyTo(this.textEncoding.ReaderQuotas);
                value.CopyTo(this.mtomEncoding.ReaderQuotas);
            }
        }

        public override string Scheme
        {
            get
            {
                return this.GetTransport().Scheme;
            }
        }

        public BasicHttpSecurity Security
        {
            get
            {
                return this.security;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.security = value;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get
            {
                return false;
            }
        }

        [TypeConverter(typeof(EncodingConverter))]
        public Encoding TextEncoding
        {
            get
            {
                return this.textEncoding.WriteEncoding;
            }
            set
            {
                this.textEncoding.WriteEncoding = value;
                this.mtomEncoding.WriteEncoding = value;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.TransferMode TransferMode
        {
            get
            {
                return this.httpTransport.TransferMode;
            }
            set
            {
                this.httpTransport.TransferMode = value;
                this.httpsTransport.TransferMode = value;
            }
        }

        [DefaultValue(true)]
        public bool UseDefaultWebProxy
        {
            get
            {
                return this.httpTransport.UseDefaultWebProxy;
            }
            set
            {
                this.httpTransport.UseDefaultWebProxy = value;
                this.httpsTransport.UseDefaultWebProxy = value;
            }
        }
    }
}

