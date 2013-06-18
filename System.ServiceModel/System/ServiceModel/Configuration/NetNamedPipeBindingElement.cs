namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class NetNamedPipeBindingElement : StandardBindingElement
    {
        private ConfigurationPropertyCollection properties;

        public NetNamedPipeBindingElement() : this(null)
        {
        }

        public NetNamedPipeBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetNamedPipeBinding binding2 = (NetNamedPipeBinding) binding;
            this.TransactionFlow = binding2.TransactionFlow;
            this.TransferMode = binding2.TransferMode;
            this.TransactionProtocol = binding2.TransactionProtocol;
            this.HostNameComparisonMode = binding2.HostNameComparisonMode;
            this.MaxBufferPoolSize = binding2.MaxBufferPoolSize;
            this.MaxBufferSize = binding2.MaxBufferSize;
            this.MaxConnections = binding2.MaxConnections;
            this.MaxReceivedMessageSize = binding2.MaxReceivedMessageSize;
            this.Security.InitializeFrom(binding2.Security);
            this.ReaderQuotas.InitializeFrom(binding2.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            NetNamedPipeBinding binding2 = (NetNamedPipeBinding) binding;
            binding2.TransactionFlow = this.TransactionFlow;
            binding2.TransferMode = this.TransferMode;
            binding2.TransactionProtocol = this.TransactionProtocol;
            binding2.HostNameComparisonMode = this.HostNameComparisonMode;
            binding2.MaxBufferPoolSize = this.MaxBufferPoolSize;
            if (base.ElementInformation.Properties["maxBufferSize"].ValueOrigin != PropertyValueOrigin.Default)
            {
                binding2.MaxBufferSize = this.MaxBufferSize;
            }
            binding2.MaxConnections = this.MaxConnections;
            binding2.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            this.Security.ApplyConfiguration(binding2.Security);
            this.ReaderQuotas.ApplyConfiguration(binding2.ReaderQuotas);
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(NetNamedPipeBinding);
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

        [ConfigurationProperty("maxBufferSize", DefaultValue=0x10000), IntegerValidator(MinValue=1)]
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

        [ConfigurationProperty("maxConnections", DefaultValue=10), IntegerValidator(MinValue=1)]
        public int MaxConnections
        {
            get
            {
                return (int) base["maxConnections"];
            }
            set
            {
                base["maxConnections"] = value;
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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("transactionFlow", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new ServiceModelEnumValidator(typeof(TransferModeHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transactionProtocol", typeof(System.ServiceModel.TransactionProtocol), "OleTransactions", new TransactionProtocolConverter(), null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(long), 0x80000L, null, new LongValidator(0L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(int), 0x10000, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxConnections", typeof(int), 10, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(long), 0x10000L, null, new LongValidator(1L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(NetNamedPipeSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
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
        public NetNamedPipeSecurityElement Security
        {
            get
            {
                return (NetNamedPipeSecurityElement) base["security"];
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

        [ConfigurationProperty("transactionProtocol", DefaultValue="OleTransactions"), TypeConverter(typeof(TransactionProtocolConverter))]
        public System.ServiceModel.TransactionProtocol TransactionProtocol
        {
            get
            {
                return (System.ServiceModel.TransactionProtocol) base["transactionProtocol"];
            }
            set
            {
                base["transactionProtocol"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(TransferModeHelper)), ConfigurationProperty("transferMode", DefaultValue=0)]
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
    }
}

