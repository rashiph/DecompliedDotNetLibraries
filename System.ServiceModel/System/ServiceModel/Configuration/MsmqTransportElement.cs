namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class MsmqTransportElement : MsmqElementBase
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            MsmqTransportBindingElement element = bindingElement as MsmqTransportBindingElement;
            element.MaxPoolSize = this.MaxPoolSize;
            element.QueueTransferProtocol = this.QueueTransferProtocol;
            element.UseActiveDirectory = this.UseActiveDirectory;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            MsmqTransportElement element = from as MsmqTransportElement;
            if (element != null)
            {
                this.MaxPoolSize = element.MaxPoolSize;
                this.QueueTransferProtocol = element.QueueTransferProtocol;
                this.UseActiveDirectory = element.UseActiveDirectory;
            }
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new MsmqTransportBindingElement();
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            MsmqTransportBindingElement element = bindingElement as MsmqTransportBindingElement;
            this.MaxPoolSize = element.MaxPoolSize;
            this.QueueTransferProtocol = element.QueueTransferProtocol;
            this.UseActiveDirectory = element.UseActiveDirectory;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(MsmqTransportBindingElement);
            }
        }

        [ConfigurationProperty("maxPoolSize", DefaultValue=8), IntegerValidator(MinValue=0)]
        public int MaxPoolSize
        {
            get
            {
                return (int) base["maxPoolSize"];
            }
            set
            {
                base["maxPoolSize"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("maxPoolSize", typeof(int), 8, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("queueTransferProtocol", typeof(System.ServiceModel.QueueTransferProtocol), System.ServiceModel.QueueTransferProtocol.Native, null, new ServiceModelEnumValidator(typeof(QueueTransferProtocolHelper)), ConfigurationPropertyOptions.None));
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

