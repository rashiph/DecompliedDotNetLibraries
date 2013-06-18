namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Xml;

    public sealed class MtomMessageEncodingBindingElement : MessageEncodingBindingElement, IWsdlExportExtension, IPolicyExportExtension
    {
        private int maxBufferSize;
        private int maxReadPoolSize;
        private int maxWritePoolSize;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private XmlDictionaryReaderQuotas readerQuotas;
        private Encoding writeEncoding;

        public MtomMessageEncodingBindingElement() : this(System.ServiceModel.Channels.MessageVersion.Default, TextEncoderDefaults.Encoding)
        {
        }

        private MtomMessageEncodingBindingElement(MtomMessageEncodingBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.maxReadPoolSize = elementToBeCloned.maxReadPoolSize;
            this.maxWritePoolSize = elementToBeCloned.maxWritePoolSize;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            elementToBeCloned.readerQuotas.CopyTo(this.readerQuotas);
            this.maxBufferSize = elementToBeCloned.maxBufferSize;
            this.writeEncoding = elementToBeCloned.writeEncoding;
            this.messageVersion = elementToBeCloned.messageVersion;
        }

        public MtomMessageEncodingBindingElement(System.ServiceModel.Channels.MessageVersion messageVersion, Encoding writeEncoding)
        {
            if (messageVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (messageVersion == System.ServiceModel.Channels.MessageVersion.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MtomEncoderBadMessageVersion", new object[] { messageVersion.ToString() }), "messageVersion"));
            }
            if (writeEncoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");
            }
            TextEncoderDefaults.ValidateEncoding(writeEncoding);
            this.maxReadPoolSize = 0x40;
            this.maxWritePoolSize = 0x10;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            this.maxBufferSize = 0x10000;
            this.messageVersion = messageVersion;
            this.writeEncoding = writeEncoding;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return base.InternalBuildChannelFactory<TChannel>(context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            return base.InternalBuildChannelListener<TChannel>(context);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return base.InternalCanBuildChannelFactory<TChannel>(context);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            return base.InternalCanBuildChannelListener<TChannel>(context);
        }

        internal override bool CheckEncodingVersion(EnvelopeVersion version)
        {
            return (this.messageVersion.Envelope == version);
        }

        public override BindingElement Clone()
        {
            return new MtomMessageEncodingBindingElement(this);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new MtomMessageEncoderFactory(this.MessageVersion, this.WriteEncoding, this.MaxReadPoolSize, this.MaxWritePoolSize, this.MaxBufferSize, this.ReaderQuotas);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return (T) this.readerQuotas;
            }
            return base.GetProperty<T>(context);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
            {
                return false;
            }
            MtomMessageEncodingBindingElement element = b as MtomMessageEncodingBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.maxReadPoolSize != element.MaxReadPoolSize)
            {
                return false;
            }
            if (this.maxWritePoolSize != element.MaxWritePoolSize)
            {
                return false;
            }
            if (this.readerQuotas.MaxStringContentLength != element.ReaderQuotas.MaxStringContentLength)
            {
                return false;
            }
            if (this.readerQuotas.MaxArrayLength != element.ReaderQuotas.MaxArrayLength)
            {
                return false;
            }
            if (this.readerQuotas.MaxBytesPerRead != element.ReaderQuotas.MaxBytesPerRead)
            {
                return false;
            }
            if (this.readerQuotas.MaxDepth != element.ReaderQuotas.MaxDepth)
            {
                return false;
            }
            if (this.readerQuotas.MaxNameTableCharCount != element.ReaderQuotas.MaxNameTableCharCount)
            {
                return false;
            }
            if (this.maxBufferSize != element.MaxBufferSize)
            {
                return false;
            }
            if (this.WriteEncoding.EncodingName != element.WriteEncoding.EncodingName)
            {
                return false;
            }
            if (!this.MessageVersion.IsMatch(element.MessageVersion))
            {
                return false;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessageVersion()
        {
            return !this.messageVersion.IsMatch(System.ServiceModel.Channels.MessageVersion.Default);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return !EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeWriteEncoding()
        {
            return (this.WriteEncoding != TextEncoderDefaults.Encoding);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            if (policyContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
            }
            XmlDocument document = new XmlDocument();
            policyContext.GetBindingAssertions().Add(document.CreateElement("wsoma", "OptimizedMimeSerialization", "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization"));
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            SoapHelper.SetSoapVersion(context, exporter, this.messageVersion.Envelope);
        }

        [DefaultValue(0x10000)]
        public int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxBufferSize = value;
            }
        }

        [DefaultValue(0x40)]
        public int MaxReadPoolSize
        {
            get
            {
                return this.maxReadPoolSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxReadPoolSize = value;
            }
        }

        [DefaultValue(0x10)]
        public int MaxWritePoolSize
        {
            get
            {
                return this.maxWritePoolSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxWritePoolSize = value;
            }
        }

        public override System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == System.ServiceModel.Channels.MessageVersion.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MtomEncoderBadMessageVersion", new object[] { value.ToString() }), "value"));
                }
                this.messageVersion = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

        [TypeConverter(typeof(EncodingConverter))]
        public Encoding WriteEncoding
        {
            get
            {
                return this.writeEncoding;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                TextEncoderDefaults.ValidateEncoding(value);
                this.writeEncoding = value;
            }
        }
    }
}

