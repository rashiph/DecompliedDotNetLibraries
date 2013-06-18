namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    public abstract class WSHttpBindingBaseElement : StandardBindingElement
    {
        private ConfigurationPropertyCollection properties;

        protected WSHttpBindingBaseElement() : this(null)
        {
        }

        protected WSHttpBindingBaseElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            WSHttpBindingBase base2 = (WSHttpBindingBase) binding;
            this.BypassProxyOnLocal = base2.BypassProxyOnLocal;
            this.TransactionFlow = base2.TransactionFlow;
            this.HostNameComparisonMode = base2.HostNameComparisonMode;
            this.MaxBufferPoolSize = base2.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = base2.MaxReceivedMessageSize;
            this.MessageEncoding = base2.MessageEncoding;
            if (base2.ProxyAddress != null)
            {
                this.ProxyAddress = base2.ProxyAddress;
            }
            this.TextEncoding = base2.TextEncoding;
            this.UseDefaultWebProxy = base2.UseDefaultWebProxy;
            this.ReaderQuotas.InitializeFrom(base2.ReaderQuotas);
            this.ReliableSession.InitializeFrom(base2.ReliableSession);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            WSHttpBindingBase base2 = (WSHttpBindingBase) binding;
            base2.BypassProxyOnLocal = this.BypassProxyOnLocal;
            base2.TransactionFlow = this.TransactionFlow;
            base2.HostNameComparisonMode = this.HostNameComparisonMode;
            base2.MaxBufferPoolSize = this.MaxBufferPoolSize;
            base2.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            base2.MessageEncoding = this.MessageEncoding;
            if (this.ProxyAddress != null)
            {
                base2.ProxyAddress = this.ProxyAddress;
            }
            base2.TextEncoding = this.TextEncoding;
            base2.UseDefaultWebProxy = this.UseDefaultWebProxy;
            this.ReaderQuotas.ApplyConfiguration(base2.ReaderQuotas);
            this.ReliableSession.ApplyConfiguration(base2.ReliableSession);
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

        [LongValidator(MinValue=1L), ConfigurationProperty("maxReceivedMessageSize", DefaultValue=0x10000L)]
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
                    properties.Add(new ConfigurationProperty("transactionFlow", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(long), 0x80000L, null, new LongValidator(0L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(long), 0x10000L, null, new LongValidator(1L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(WSMessageEncoding), WSMessageEncoding.Text, null, new ServiceModelEnumValidator(typeof(WSMessageEncodingHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableSession", typeof(StandardBindingOptionalReliableSessionElement), null, null, null, ConfigurationPropertyOptions.None));
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
        public StandardBindingOptionalReliableSessionElement ReliableSession
        {
            get
            {
                return (StandardBindingOptionalReliableSessionElement) base["reliableSession"];
            }
        }

        [ConfigurationProperty("textEncoding", DefaultValue="utf-8"), TypeConverter(typeof(EncodingConverter))]
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

