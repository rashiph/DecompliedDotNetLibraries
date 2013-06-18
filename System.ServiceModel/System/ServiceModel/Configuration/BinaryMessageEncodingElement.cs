namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed class BinaryMessageEncodingElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            BinaryMessageEncodingBindingElement element = (BinaryMessageEncodingBindingElement) bindingElement;
            element.MaxSessionSize = this.MaxSessionSize;
            element.MaxReadPoolSize = this.MaxReadPoolSize;
            element.MaxWritePoolSize = this.MaxWritePoolSize;
            this.ReaderQuotas.ApplyConfiguration(element.ReaderQuotas);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            BinaryMessageEncodingElement element = (BinaryMessageEncodingElement) from;
            this.MaxSessionSize = element.MaxSessionSize;
            this.MaxReadPoolSize = element.MaxReadPoolSize;
            this.MaxWritePoolSize = element.MaxWritePoolSize;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            BinaryMessageEncodingBindingElement bindingElement = new BinaryMessageEncodingBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            BinaryMessageEncodingBindingElement element = (BinaryMessageEncodingBindingElement) bindingElement;
            this.MaxSessionSize = element.MaxSessionSize;
            this.MaxReadPoolSize = element.MaxReadPoolSize;
            this.MaxWritePoolSize = element.MaxWritePoolSize;
            this.ReaderQuotas.InitializeFrom(element.ReaderQuotas);
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(BinaryMessageEncodingBindingElement);
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxReadPoolSize", DefaultValue=0x40)]
        public int MaxReadPoolSize
        {
            get
            {
                return (int) base["maxReadPoolSize"];
            }
            set
            {
                base["maxReadPoolSize"] = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("maxSessionSize", DefaultValue=0x800)]
        public int MaxSessionSize
        {
            get
            {
                return (int) base["maxSessionSize"];
            }
            set
            {
                base["maxSessionSize"] = value;
            }
        }

        [ConfigurationProperty("maxWritePoolSize", DefaultValue=0x10), IntegerValidator(MinValue=1)]
        public int MaxWritePoolSize
        {
            get
            {
                return (int) base["maxWritePoolSize"];
            }
            set
            {
                base["maxWritePoolSize"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("maxReadPoolSize", typeof(int), 0x40, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxWritePoolSize", typeof(int), 0x10, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxSessionSize", typeof(int), 0x800, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
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
    }
}

