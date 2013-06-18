namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Web.Services.Description;
    using System.Xml;

    public abstract class TransportBindingElement : BindingElement
    {
        private bool manualAddressing;
        private long maxBufferPoolSize;
        private long maxReceivedMessageSize;

        protected TransportBindingElement()
        {
            this.manualAddressing = false;
            this.maxBufferPoolSize = 0x80000L;
            this.maxReceivedMessageSize = 0x10000L;
        }

        protected TransportBindingElement(TransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.manualAddressing = elementToBeCloned.manualAddressing;
            this.maxBufferPoolSize = elementToBeCloned.maxBufferPoolSize;
            this.maxReceivedMessageSize = elementToBeCloned.maxReceivedMessageSize;
        }

        internal static IChannelFactory<TChannel> CreateChannelFactory<TChannel>(TransportBindingElement transport)
        {
            System.ServiceModel.Channels.Binding binding = new CustomBinding(new BindingElement[] { transport });
            return binding.BuildChannelFactory<TChannel>(new object[0]);
        }

        internal static IChannelListener CreateChannelListener<TChannel>(TransportBindingElement transport) where TChannel: class, IChannel
        {
            System.ServiceModel.Channels.Binding binding = new CustomBinding(new BindingElement[] { transport });
            return binding.BuildChannelListener<TChannel>(new object[0]);
        }

        internal static void ExportWsdlEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext, string wsdlTransportUri, AddressingVersion addressingVersion)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (endpointContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");
            }
            endpointContext.Endpoint.Binding.CreateBindingElements();
            if (wsdlTransportUri != null)
            {
                SoapBinding orCreateSoapBinding = SoapHelper.GetOrCreateSoapBinding(endpointContext, exporter);
                if (orCreateSoapBinding != null)
                {
                    orCreateSoapBinding.Transport = wsdlTransportUri;
                }
            }
            if (endpointContext.WsdlPort != null)
            {
                WsdlExporter.WSAddressingHelper.AddAddressToWsdlPort(endpointContext.WsdlPort, endpointContext.Endpoint.Address, addressingVersion);
            }
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                ChannelProtectionRequirements protectionRequirements = this.GetProtectionRequirements(context);
                protectionRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
                return (T) protectionRequirements;
            }
            Collection<BindingElement> collection = context.BindingParameters.FindAll<BindingElement>();
            T individualProperty = default(T);
            for (int i = 0; i < collection.Count; i++)
            {
                individualProperty = collection[i].GetIndividualProperty<T>();
                if (individualProperty != new T())
                {
                    return individualProperty;
                }
            }
            if (typeof(T) == typeof(MessageVersion))
            {
                return (T) TransportDefaults.GetDefaultMessageEncoderFactory().MessageVersion;
            }
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return (T) new XmlDictionaryReaderQuotas();
            }
            return default(T);
        }

        private ChannelProtectionRequirements GetProtectionRequirements(AddressingVersion addressingVersion)
        {
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            requirements.IncomingSignatureParts.AddParts(addressingVersion.SignedMessageParts);
            requirements.OutgoingSignatureParts.AddParts(addressingVersion.SignedMessageParts);
            return requirements;
        }

        internal ChannelProtectionRequirements GetProtectionRequirements(BindingContext context)
        {
            AddressingVersion addressingVersion = AddressingVersion.WSAddressing10;
            MessageEncodingBindingElement element = context.Binding.Elements.Find<MessageEncodingBindingElement>();
            if (element != null)
            {
                addressingVersion = element.MessageVersion.Addressing;
            }
            return this.GetProtectionRequirements(addressingVersion);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            TransportBindingElement element = b as TransportBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.maxBufferPoolSize != element.MaxBufferPoolSize)
            {
                return false;
            }
            if (this.maxReceivedMessageSize != element.MaxReceivedMessageSize)
            {
                return false;
            }
            return true;
        }

        [DefaultValue(false)]
        public bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
            set
            {
                this.manualAddressing = value;
            }
        }

        [DefaultValue((long) 0x80000L)]
        public virtual long MaxBufferPoolSize
        {
            get
            {
                return this.maxBufferPoolSize;
            }
            set
            {
                if (value < 0L)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.maxBufferPoolSize = value;
            }
        }

        [DefaultValue((long) 0x10000L)]
        public virtual long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
            set
            {
                if (value <= 0L)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxReceivedMessageSize = value;
            }
        }

        public abstract string Scheme { get; }
    }
}

