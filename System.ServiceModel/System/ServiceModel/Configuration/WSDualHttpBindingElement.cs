namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    public class WSDualHttpBindingElement : StandardBindingElement
    {
        private ConfigurationPropertyCollection properties;

        public WSDualHttpBindingElement() : this(null)
        {
        }

        public WSDualHttpBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            WSDualHttpBinding binding2 = (WSDualHttpBinding) binding;
            this.BypassProxyOnLocal = binding2.BypassProxyOnLocal;
            if (binding2.ClientBaseAddress != null)
            {
                this.ClientBaseAddress = binding2.ClientBaseAddress;
            }
            this.TransactionFlow = binding2.TransactionFlow;
            this.HostNameComparisonMode = binding2.HostNameComparisonMode;
            this.MaxBufferPoolSize = binding2.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = binding2.MaxReceivedMessageSize;
            this.MessageEncoding = binding2.MessageEncoding;
            if (binding2.ProxyAddress != null)
            {
                this.ProxyAddress = binding2.ProxyAddress;
            }
            this.ReliableSession.InitializeFrom(binding2.ReliableSession);
            this.TextEncoding = binding2.TextEncoding;
            this.UseDefaultWebProxy = binding2.UseDefaultWebProxy;
            this.Security.InitializeFrom(binding2.Security);
            this.ReaderQuotas.InitializeFrom(binding2.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            WSDualHttpBinding binding2 = (WSDualHttpBinding) binding;
            binding2.BypassProxyOnLocal = this.BypassProxyOnLocal;
            if (this.ClientBaseAddress != null)
            {
                binding2.ClientBaseAddress = this.ClientBaseAddress;
            }
            binding2.TransactionFlow = this.TransactionFlow;
            binding2.HostNameComparisonMode = this.HostNameComparisonMode;
            binding2.MaxBufferPoolSize = this.MaxBufferPoolSize;
            binding2.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            binding2.MessageEncoding = this.MessageEncoding;
            if (this.ProxyAddress != null)
            {
                binding2.ProxyAddress = this.ProxyAddress;
            }
            this.ReliableSession.ApplyConfiguration(binding2.ReliableSession);
            binding2.TextEncoding = this.TextEncoding;
            binding2.UseDefaultWebProxy = this.UseDefaultWebProxy;
            this.Security.ApplyConfiguration(binding2.Security);
            this.ReaderQuotas.ApplyConfiguration(binding2.ReaderQuotas);
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(WSDualHttpBinding);
            }
        }

        [ConfigurationProperty("bypassProxyOnLocal", DefaultValue=false)]
        public bool BypassProxyOnLocal
        {
            get
            {
                return (bool) base["bypassProxyOnLocal"];
            }
            set
            {
                base["bypassProxyOnLocal"] = value;
            }
        }

        [ConfigurationProperty("clientBaseAddress", DefaultValue=null)]
        public Uri ClientBaseAddress
        {
            get
            {
                return (Uri) base["clientBaseAddress"];
            }
            set
            {
                base["clientBaseAddress"] = value;
            }
        }

        [ConfigurationProperty("hostNameComparisonMode", DefaultValue=0), ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper))]
        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return (System.ServiceModel.HostNameComparisonMode) base["hostNameComparisonMode"];
            }
            set
            {
                base["hostNameComparisonMode"] = value;
            }
        }

        [LongValidator(MinValue=0L), ConfigurationProperty("maxBufferPoolSize", DefaultValue=0x80000L)]
        public long MaxBufferPoolSize
        {
            get
            {
                return (long) base["maxBufferPoolSize"];
            }
            set
            {
                base["maxBufferPoolSize"] = value;
            }
        }

        [ConfigurationProperty("maxReceivedMessageSize", DefaultValue=0x10000L), LongValidator(MinValue=1L)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return (long) base["maxReceivedMessageSize"];
            }
            set
            {
                base["maxReceivedMessageSize"] = value;
            }
        }

        [ConfigurationProperty("messageEncoding", DefaultValue=0), ServiceModelEnumValidator(typeof(WSMessageEncodingHelper))]
        public WSMessageEncoding MessageEncoding
        {
            get
            {
                return (WSMessageEncoding) base["messageEncoding"];
            }
            set
            {
                base["messageEncoding"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("bypassProxyOnLocal", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("clientBaseAddress", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transactionFlow", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(long), 0x80000L, null, new LongValidator(0L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(long), 0x10000L, null, new LongValidator(1L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(WSMessageEncoding), WSMessageEncoding.Text, null, new ServiceModelEnumValidator(typeof(WSMessageEncodingHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableSession", typeof(StandardBindingReliableSessionElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(WSDualHttpSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("textEncoding", typeof(Encoding), "utf-8", new EncodingConverter(), null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useDefaultWebProxy", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("proxyAddress", DefaultValue=null)]
        public Uri ProxyAddress
        {
            get
            {
                return (Uri) base["proxyAddress"];
            }
            set
            {
                base["proxyAddress"] = value;
            }
        }

        [ConfigurationProperty("readerQuotas")]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get
            {
                return (XmlDictionaryReaderQuotasElement) base["readerQuotas"];
            }
        }

        [ConfigurationProperty("reliableSession")]
        public StandardBindingReliableSessionElement ReliableSession
        {
            get
            {
                return (StandardBindingReliableSessionElement) base["reliableSession"];
            }
        }

        [ConfigurationProperty("security")]
        public WSDualHttpSecurityElement Security
        {
            get
            {
                return (WSDualHttpSecurityElement) base["security"];
            }
        }

        [TypeConverter(typeof(EncodingConverter)), ConfigurationProperty("textEncoding", DefaultValue="utf-8")]
        public Encoding TextEncoding
        {
            get
            {
                return (Encoding) base["textEncoding"];
            }
            set
            {
                base["textEncoding"] = value;
            }
        }

        [ConfigurationProperty("transactionFlow", DefaultValue=false)]
        public bool TransactionFlow
        {
            get
            {
                return (bool) base["transactionFlow"];
            }
            set
            {
                base["transactionFlow"] = value;
            }
        }

        [ConfigurationProperty("useDefaultWebProxy", DefaultValue=true)]
        public bool UseDefaultWebProxy
        {
            get
            {
                return (bool) base["useDefaultWebProxy"];
            }
            set
            {
                base["useDefaultWebProxy"] = value;
            }
        }
    }
}

