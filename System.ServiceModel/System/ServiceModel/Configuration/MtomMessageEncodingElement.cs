namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.Text;

    public sealed class MtomMessageEncodingElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            MtomMessageEncodingBindingElement element = (MtomMessageEncodingBindingElement) bindingElement;
            element.MessageVersion = this.MessageVersion;
            element.WriteEncoding = this.WriteEncoding;
            element.MaxReadPoolSize = this.MaxReadPoolSize;
            element.MaxWritePoolSize = this.MaxWritePoolSize;
            this.ReaderQuotas.ApplyConfiguration(element.ReaderQuotas);
            element.MaxBufferSize = this.MaxBufferSize;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            MtomMessageEncodingElement element = (MtomMessageEncodingElement) from;
            this.MessageVersion = element.MessageVersion;
            this.WriteEncoding = element.WriteEncoding;
            this.MaxReadPoolSize = element.MaxReadPoolSize;
            this.MaxWritePoolSize = element.MaxWritePoolSize;
            this.MaxBufferSize = element.MaxBufferSize;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            MtomMessageEncodingBindingElement bindingElement = new MtomMessageEncodingBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            MtomMessageEncodingBindingElement element = (MtomMessageEncodingBindingElement) bindingElement;
            this.MessageVersion = element.MessageVersion;
            this.WriteEncoding = element.WriteEncoding;
            this.MaxReadPoolSize = element.MaxReadPoolSize;
            this.MaxWritePoolSize = element.MaxWritePoolSize;
            this.ReaderQuotas.InitializeFrom(element.ReaderQuotas);
            this.MaxBufferSize = element.MaxBufferSize;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(MtomMessageEncodingBindingElement);
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

        [ConfigurationProperty("maxReadPoolSize", DefaultValue=0x40), IntegerValidator(MinValue=1)]
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

        [ConfigurationProperty("messageVersion", DefaultValue="Soap12WSAddressing10"), TypeConverter(typeof(MessageVersionConverter))]
        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return (System.ServiceModel.Channels.MessageVersion) base["messageVersion"];
            }
            set
            {
                base["messageVersion"] = value;
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
                    propertys.Add(new ConfigurationProperty("messageVersion", typeof(System.ServiceModel.Channels.MessageVersion), "Soap12WSAddressing10", new MessageVersionConverter(), null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("readerQuotas", typeof(XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxBufferSize", typeof(int), 0x10000, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("writeEncoding", typeof(Encoding), "utf-8", new EncodingConverter(), null, ConfigurationPropertyOptions.None));
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

        [TypeConverter(typeof(EncodingConverter)), ConfigurationProperty("writeEncoding", DefaultValue="utf-8")]
        public Encoding WriteEncoding
        {
            get
            {
                return (Encoding) base["writeEncoding"];
            }
            set
            {
                base["writeEncoding"] = value;
            }
        }
    }
}

