namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public abstract class TransportElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        protected TransportElement()
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            TransportBindingElement element = (TransportBindingElement) bindingElement;
            element.ManualAddressing = this.ManualAddressing;
            element.MaxBufferPoolSize = this.MaxBufferPoolSize;
            element.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            TransportElement element = (TransportElement) from;
            this.ManualAddressing = element.ManualAddressing;
            this.MaxBufferPoolSize = element.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = element.MaxReceivedMessageSize;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            TransportBindingElement bindingElement = this.CreateDefaultBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected abstract TransportBindingElement CreateDefaultBindingElement();
        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            TransportBindingElement element = (TransportBindingElement) bindingElement;
            this.ManualAddressing = element.ManualAddressing;
            this.MaxBufferPoolSize = element.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = element.MaxReceivedMessageSize;
        }

        [ConfigurationProperty("manualAddressing", DefaultValue=false)]
        public bool ManualAddressing
        {
            get
            {
                return (bool) base["manualAddressing"];
            }
            set
            {
                base["manualAddressing"] = value;
            }
        }

        [ConfigurationProperty("maxBufferPoolSize", DefaultValue=0x80000L), LongValidator(MinValue=1L)]
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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("manualAddressing", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(long), 0x80000L, null, new LongValidator(1L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(long), 0x10000L, null, new LongValidator(1L, 0x7fffffffffffffffL, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

