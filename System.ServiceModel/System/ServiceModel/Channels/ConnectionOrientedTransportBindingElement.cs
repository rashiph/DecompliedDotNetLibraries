namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public abstract class ConnectionOrientedTransportBindingElement : TransportBindingElement, IWsdlExportExtension, IPolicyExportExtension, ITransportPolicyImport
    {
        private TimeSpan channelInitializationTimeout;
        private int connectionBufferSize;
        private bool exposeConnectionProperty;
        private System.ServiceModel.HostNameComparisonMode hostNameComparisonMode;
        private bool inheritBaseAddressSettings;
        private int maxBufferSize;
        private bool maxBufferSizeInitialized;
        private TimeSpan maxOutputDelay;
        private int maxPendingAccepts;
        private int maxPendingConnections;
        private System.ServiceModel.TransferMode transferMode;

        internal ConnectionOrientedTransportBindingElement()
        {
            this.connectionBufferSize = 0x2000;
            this.hostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
            this.channelInitializationTimeout = ConnectionOrientedTransportDefaults.ChannelInitializationTimeout;
            this.maxBufferSize = 0x10000;
            this.maxPendingConnections = 10;
            this.maxOutputDelay = ConnectionOrientedTransportDefaults.MaxOutputDelay;
            this.maxPendingAccepts = 1;
            this.transferMode = System.ServiceModel.TransferMode.Buffered;
        }

        internal ConnectionOrientedTransportBindingElement(ConnectionOrientedTransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.connectionBufferSize = elementToBeCloned.connectionBufferSize;
            this.exposeConnectionProperty = elementToBeCloned.exposeConnectionProperty;
            this.hostNameComparisonMode = elementToBeCloned.hostNameComparisonMode;
            this.inheritBaseAddressSettings = elementToBeCloned.InheritBaseAddressSettings;
            this.channelInitializationTimeout = elementToBeCloned.ChannelInitializationTimeout;
            this.maxBufferSize = elementToBeCloned.maxBufferSize;
            this.maxBufferSizeInitialized = elementToBeCloned.maxBufferSizeInitialized;
            this.maxPendingConnections = elementToBeCloned.maxPendingConnections;
            this.maxOutputDelay = elementToBeCloned.maxOutputDelay;
            this.maxPendingAccepts = elementToBeCloned.maxPendingAccepts;
            this.transferMode = elementToBeCloned.transferMode;
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (this.TransferMode == System.ServiceModel.TransferMode.Buffered)
            {
                return (typeof(TChannel) == typeof(IDuplexSessionChannel));
            }
            return (typeof(TChannel) == typeof(IRequestChannel));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (this.TransferMode == System.ServiceModel.TransferMode.Buffered)
            {
                return (typeof(TChannel) == typeof(IDuplexSessionChannel));
            }
            return (typeof(TChannel) == typeof(IReplyChannel));
        }

        private MessageEncodingBindingElement FindMessageEncodingBindingElement(BindingElementCollection bindingElements, out bool createdNew)
        {
            createdNew = false;
            MessageEncodingBindingElement element = bindingElements.Find<MessageEncodingBindingElement>();
            if (element == null)
            {
                createdNew = true;
                element = new BinaryMessageEncodingBindingElement();
            }
            return element;
        }

        private MessageEncodingBindingElement FindMessageEncodingBindingElement(WsdlEndpointConversionContext endpointContext, out bool createdNew)
        {
            BindingElementCollection bindingElements = endpointContext.Endpoint.Binding.CreateBindingElements();
            return this.FindMessageEncodingBindingElement(bindingElements, out createdNew);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(System.ServiceModel.TransferMode))
            {
                return (T) this.TransferMode;
            }
            return base.GetProperty<T>(context);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
            {
                return false;
            }
            ConnectionOrientedTransportBindingElement element = b as ConnectionOrientedTransportBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.connectionBufferSize != element.connectionBufferSize)
            {
                return false;
            }
            if (this.hostNameComparisonMode != element.hostNameComparisonMode)
            {
                return false;
            }
            if (this.inheritBaseAddressSettings != element.inheritBaseAddressSettings)
            {
                return false;
            }
            if (this.channelInitializationTimeout != element.channelInitializationTimeout)
            {
                return false;
            }
            if (this.maxBufferSize != element.maxBufferSize)
            {
                return false;
            }
            if (this.maxPendingConnections != element.maxPendingConnections)
            {
                return false;
            }
            if (this.maxOutputDelay != element.maxOutputDelay)
            {
                return false;
            }
            if (this.maxPendingAccepts != element.maxPendingAccepts)
            {
                return false;
            }
            if (this.transferMode != element.transferMode)
            {
                return false;
            }
            return true;
        }

        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            if (PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "Streamed", "http://schemas.microsoft.com/ws/2006/05/framing/policy", true) != null)
            {
                this.TransferMode = System.ServiceModel.TransferMode.Streamed;
            }
            WindowsStreamSecurityBindingElement.ImportPolicy(importer, policyContext);
            SslStreamSecurityBindingElement.ImportPolicy(importer, policyContext);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            bool flag;
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            ICollection<XmlElement> bindingAssertions = context.GetBindingAssertions();
            if (TransferModeHelper.IsRequestStreamed(this.TransferMode) || TransferModeHelper.IsResponseStreamed(this.TransferMode))
            {
                bindingAssertions.Add(new XmlDocument().CreateElement("msf", "Streamed", "http://schemas.microsoft.com/ws/2006/05/framing/policy"));
            }
            MessageEncodingBindingElement element = this.FindMessageEncodingBindingElement(context.BindingElements, out flag);
            if (flag && (element is IPolicyExportExtension))
            {
                element = new BinaryMessageEncodingBindingElement();
                ((IPolicyExportExtension) element).ExportPolicy(exporter, context);
            }
            WsdlExporter.WSAddressingHelper.AddWSAddressingAssertion(exporter, context, element.MessageVersion.Addressing);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            bool flag;
            MessageEncodingBindingElement element = this.FindMessageEncodingBindingElement(endpointContext, out flag);
            TransportBindingElement.ExportWsdlEndpoint(exporter, endpointContext, this.WsdlTransportUri, element.MessageVersion.Addressing);
        }

        [DefaultValue(typeof(TimeSpan), "00:00:05")]
        public TimeSpan ChannelInitializationTimeout
        {
            get
            {
                return this.channelInitializationTimeout;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.channelInitializationTimeout = value;
            }
        }

        [DefaultValue(0x2000)]
        public int ConnectionBufferSize
        {
            get
            {
                return this.connectionBufferSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.connectionBufferSize = value;
            }
        }

        internal bool ExposeConnectionProperty
        {
            get
            {
                return this.exposeConnectionProperty;
            }
            set
            {
                this.exposeConnectionProperty = value;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
            set
            {
                HostNameComparisonModeHelper.Validate(value);
                this.hostNameComparisonMode = value;
            }
        }

        internal bool InheritBaseAddressSettings
        {
            get
            {
                return this.inheritBaseAddressSettings;
            }
            set
            {
                this.inheritBaseAddressSettings = value;
            }
        }

        [DefaultValue(0x10000)]
        public int MaxBufferSize
        {
            get
            {
                if (this.maxBufferSizeInitialized || (this.TransferMode != System.ServiceModel.TransferMode.Buffered))
                {
                    return this.maxBufferSize;
                }
                long maxReceivedMessageSize = this.MaxReceivedMessageSize;
                if (maxReceivedMessageSize > 0x7fffffffL)
                {
                    return 0x7fffffff;
                }
                return (int) maxReceivedMessageSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxBufferSizeInitialized = true;
                this.maxBufferSize = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), "00:00:00.2")]
        public TimeSpan MaxOutputDelay
        {
            get
            {
                return this.maxOutputDelay;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.maxOutputDelay = value;
            }
        }

        [DefaultValue(1)]
        public int MaxPendingAccepts
        {
            get
            {
                return this.maxPendingAccepts;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxPendingAccepts = value;
            }
        }

        [DefaultValue(10)]
        public int MaxPendingConnections
        {
            get
            {
                return this.maxPendingConnections;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxPendingConnections = value;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.TransferMode TransferMode
        {
            get
            {
                return this.transferMode;
            }
            set
            {
                TransferModeHelper.Validate(value);
                this.transferMode = value;
            }
        }

        internal abstract string WsdlTransportUri { get; }
    }
}

