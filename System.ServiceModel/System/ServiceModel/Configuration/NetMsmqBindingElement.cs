namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class NetMsmqBindingElement : System.ServiceModel.Configuration.MsmqBindingElementBase
    {
        private ConfigurationPropertyCollection properties;

        public NetMsmqBindingElement() : this(null)
        {
        }

        public NetMsmqBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetMsmqBinding binding2 = (NetMsmqBinding) binding;
            this.MaxBufferPoolSize = binding2.MaxBufferPoolSize;
            this.QueueTransferProtocol = binding2.QueueTransferProtocol;
            this.UseActiveDirectory = binding2.UseActiveDirectory;
            this.Security.InitializeFrom(binding2.Security);
            this.ReaderQuotas.InitializeFrom(binding2.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            NetMsmqBinding binding2 = (NetMsmqBinding) binding;
            binding2.MaxBufferPoolSize = this.MaxBufferPoolSize;
            binding2.QueueTransferProtocol = this.QueueTransferProtocol;
            binding2.UseActiveDirectory = this.UseActiveDirectory;
            this.Security.ApplyConfiguration(binding2.Security);
            this.ReaderQuotas.ApplyConfiguration(binding2.ReaderQuotas);
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(NetMsmqBinding);
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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("queueTransferProtocol", typeof(System.ServiceModel.QueueTransferProtocol), System.ServiceModel.QueueTransferProtocol.Native, null, new ServiceModelEnumValidator(typeof(QueueTransferProtocolHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(long), 0x80000L, null, new LongValidator(0L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(NetMsmqSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useActiveDirectory", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("queueTransferProtocol", DefaultValue=0), ServiceModelEnumValidator(typeof(QueueTransferProtocolHelper))]
        public System.ServiceModel.QueueTransferProtocol QueueTransferProtocol
        {
            get
            {
                return (System.ServiceModel.QueueTransferProtocol) base["queueTransferProtocol"];
            }
            set
            {
                base["queueTransferProtocol"] = value;
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
        public NetMsmqSecurityElement Security
        {
            get
            {
                return (NetMsmqSecurityElement) base["security"];
            }
        }

        [ConfigurationProperty("useActiveDirectory", DefaultValue=false)]
        public bool UseActiveDirectory
        {
            get
            {
                return (bool) base["useActiveDirectory"];
            }
            set
            {
                base["useActiveDirectory"] = value;
            }
        }
    }
}

