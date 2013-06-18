namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    public class BasicHttpBindingElement : StandardBindingElement
    {
        private ConfigurationPropertyCollection properties;

        public BasicHttpBindingElement() : this(null)
        {
        }

        public BasicHttpBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            BasicHttpBinding binding2 = (BasicHttpBinding) binding;
            this.BypassProxyOnLocal = binding2.BypassProxyOnLocal;
            this.HostNameComparisonMode = binding2.HostNameComparisonMode;
            this.MaxBufferSize = binding2.MaxBufferSize;
            this.MaxBufferPoolSize = binding2.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = binding2.MaxReceivedMessageSize;
            this.MessageEncoding = binding2.MessageEncoding;
            if (binding2.ProxyAddress != null)
            {
                this.ProxyAddress = binding2.ProxyAddress;
            }
            this.TextEncoding = binding2.TextEncoding;
            this.TransferMode = binding2.TransferMode;
            this.UseDefaultWebProxy = binding2.UseDefaultWebProxy;
            this.AllowCookies = binding2.AllowCookies;
            this.Security.InitializeFrom(binding2.Security);
            this.ReaderQuotas.InitializeFrom(binding2.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            BasicHttpBinding binding2 = (BasicHttpBinding) binding;
            binding2.BypassProxyOnLocal = this.BypassProxyOnLocal;
            binding2.HostNameComparisonMode = this.HostNameComparisonMode;
            binding2.MaxBufferPoolSize = this.MaxBufferPoolSize;
            binding2.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            binding2.MessageEncoding = this.MessageEncoding;
            binding2.TextEncoding = this.TextEncoding;
            binding2.TransferMode = this.TransferMode;
            binding2.UseDefaultWebProxy = this.UseDefaultWebProxy;
            binding2.AllowCookies = this.AllowCookies;
            if (this.ProxyAddress != null)
            {
                binding2.ProxyAddress = this.ProxyAddress;
            }
            if (base.ElementInformation.Properties["maxBufferSize"].ValueOrigin != PropertyValueOrigin.Default)
            {
                binding2.MaxBufferSize = this.MaxBufferSize;
            }
            this.Security.ApplyConfiguration(binding2.Security);
            this.ReaderQuotas.ApplyConfiguration(binding2.ReaderQuotas);
        }

        [ConfigurationProperty("allowCookies", DefaultValue=false)]
        public bool AllowCookies
        {
            get
            {
                return (bool) base["allowCookies"];
            }
            set
            {
                base["allowCookies"] = value;
            }
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(BasicHttpBinding);
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

        [ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper)), ConfigurationProperty("hostNameComparisonMode", DefaultValue=0)]
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

        [ConfigurationProperty("maxBufferPoolSize", DefaultValue=0x80000L), LongValidator(MinValue=0L)]
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

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxBufferSize", DefaultValue=0x10000)]
        public int MaxBufferSize
        {
            get
            {
                return (int) base["maxBufferSize"];
            }
            set
            {
                base["maxBufferSize"] = value;
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

        [ServiceModelEnumValidator(typeof(WSMessageEncodingHelper)), ConfigurationProperty("messageEncoding", DefaultValue=0)]
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
                    properties.Add(new ConfigurationProperty("allowCookies", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bypassProxyOnLocal", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(int), 0x10000, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(long), 0x80000L, null, new LongValidator(0L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(long), 0x10000L, null, new LongValidator(1L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(WSMessageEncoding), WSMessageEncoding.Text, null, new ServiceModelEnumValidator(typeof(WSMessageEncodingHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(BasicHttpSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("textEncoding", typeof(Encoding), "utf-8", new EncodingConverter(), null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new ServiceModelEnumValidator(typeof(TransferModeHelper)), ConfigurationPropertyOptions.None));
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

        [ConfigurationProperty("security")]
        public BasicHttpSecurityElement Security
        {
            get
            {
                return (BasicHttpSecurityElement) base["security"];
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

        [ConfigurationProperty("transferMode", DefaultValue=0), ServiceModelEnumValidator(typeof(TransferModeHelper))]
        public System.ServiceModel.TransferMode TransferMode
        {
            get
            {
                return (System.ServiceModel.TransferMode) base["transferMode"];
            }
            set
            {
                base["transferMode"] = value;
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

