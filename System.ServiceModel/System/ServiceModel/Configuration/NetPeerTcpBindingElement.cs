namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class NetPeerTcpBindingElement : StandardBindingElement
    {
        private ConfigurationPropertyCollection properties;

        public NetPeerTcpBindingElement() : this(null)
        {
        }

        public NetPeerTcpBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetPeerTcpBinding binding2 = (NetPeerTcpBinding) binding;
            this.ListenIPAddress = binding2.ListenIPAddress;
            this.MaxBufferPoolSize = binding2.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = binding2.MaxReceivedMessageSize;
            this.Port = binding2.Port;
            this.Security.InitializeFrom(binding2.Security);
            this.Resolver.InitializeFrom(binding2.Resolver);
            this.ReaderQuotas.InitializeFrom(binding2.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            NetPeerTcpBinding binding2 = (NetPeerTcpBinding) binding;
            binding2.ListenIPAddress = this.ListenIPAddress;
            binding2.MaxBufferPoolSize = this.MaxBufferPoolSize;
            binding2.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            binding2.Port = this.Port;
            binding2.Security = new PeerSecuritySettings();
            this.ReaderQuotas.ApplyConfiguration(binding2.ReaderQuotas);
            this.Resolver.ApplyConfiguration(binding2.Resolver);
            this.Security.ApplyConfiguration(binding2.Security);
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(NetPeerTcpBinding);
            }
        }

        [PeerTransportListenAddressValidator, TypeConverter(typeof(PeerTransportListenAddressConverter)), ConfigurationProperty("listenIPAddress", DefaultValue=null)]
        public IPAddress ListenIPAddress
        {
            get
            {
                return (IPAddress) base["listenIPAddress"];
            }
            set
            {
                base["listenIPAddress"] = value;
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

        [LongValidator(MinValue=0x4000L), ConfigurationProperty("maxReceivedMessageSize", DefaultValue=0x10000L)]
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

        [ConfigurationProperty("port", DefaultValue=0), IntegerValidator(MinValue=0, MaxValue=0xffff)]
        public int Port
        {
            get
            {
                return (int) base["port"];
            }
            set
            {
                base["port"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("listenIPAddress", typeof(IPAddress), null, new PeerTransportListenAddressConverter(), new PeerTransportListenAddressValidator(), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(long), 0x80000L, null, new LongValidator(0L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(long), 0x10000L, null, new LongValidator(0x4000L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("port", typeof(int), 0, null, new IntegerValidator(0, 0xffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("resolver", typeof(PeerResolverElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(PeerSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
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

        [ConfigurationProperty("resolver", DefaultValue=null)]
        public PeerResolverElement Resolver
        {
            get
            {
                return (PeerResolverElement) base["resolver"];
            }
        }

        [ConfigurationProperty("security")]
        public PeerSecurityElement Security
        {
            get
            {
                return (PeerSecurityElement) base["security"];
            }
        }
    }
}

