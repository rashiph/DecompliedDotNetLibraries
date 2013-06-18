namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public abstract class MsmqBindingElementBase : TransportBindingElement, ITransactedBindingElement, IWsdlExportExtension, IPolicyExportExtension, ITransportPolicyImport
    {
        private Uri customDeadLetterQueue;
        private System.ServiceModel.DeadLetterQueue deadLetterQueue;
        private bool durable;
        private bool exactlyOnce;
        private int maxRetryCycles;
        private System.ServiceModel.MsmqTransportSecurity msmqTransportSecurity;
        private bool receiveContextEnabled;
        private System.ServiceModel.ReceiveErrorHandling receiveErrorHandling;
        private int receiveRetryCount;
        private TimeSpan retryCycleDelay;
        private TimeSpan timeToLive;
        private bool useMsmqTracing;
        private bool useSourceJournal;

        internal MsmqBindingElementBase()
        {
            this.customDeadLetterQueue = null;
            this.deadLetterQueue = System.ServiceModel.DeadLetterQueue.System;
            this.durable = true;
            this.exactlyOnce = true;
            this.maxRetryCycles = 2;
            this.receiveContextEnabled = true;
            this.receiveErrorHandling = System.ServiceModel.ReceiveErrorHandling.Fault;
            this.receiveRetryCount = 5;
            this.retryCycleDelay = MsmqDefaults.RetryCycleDelay;
            this.timeToLive = MsmqDefaults.TimeToLive;
            this.msmqTransportSecurity = new System.ServiceModel.MsmqTransportSecurity();
            this.useMsmqTracing = false;
            this.useSourceJournal = false;
            this.ReceiveContextSettings = new MsmqReceiveContextSettings();
        }

        internal MsmqBindingElementBase(MsmqBindingElementBase elementToBeCloned) : base(elementToBeCloned)
        {
            this.customDeadLetterQueue = elementToBeCloned.customDeadLetterQueue;
            this.deadLetterQueue = elementToBeCloned.deadLetterQueue;
            this.durable = elementToBeCloned.durable;
            this.exactlyOnce = elementToBeCloned.exactlyOnce;
            this.maxRetryCycles = elementToBeCloned.maxRetryCycles;
            this.msmqTransportSecurity = new System.ServiceModel.MsmqTransportSecurity(elementToBeCloned.MsmqTransportSecurity);
            this.receiveContextEnabled = elementToBeCloned.ReceiveContextEnabled;
            this.receiveErrorHandling = elementToBeCloned.receiveErrorHandling;
            this.receiveRetryCount = elementToBeCloned.receiveRetryCount;
            this.retryCycleDelay = elementToBeCloned.retryCycleDelay;
            this.timeToLive = elementToBeCloned.timeToLive;
            this.useMsmqTracing = elementToBeCloned.useMsmqTracing;
            this.useSourceJournal = elementToBeCloned.useSourceJournal;
            this.ReceiveContextSettings = elementToBeCloned.ReceiveContextSettings;
        }

        private static bool FindAssertion(ICollection<XmlElement> assertions, string name)
        {
            return (PolicyConversionContext.FindAssertion(assertions, name, "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq", true) != null);
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
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return default(T);
            }
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T) new BindingDeliveryCapabilitiesHelper();
            }
            if (typeof(T) == typeof(IReceiveContextSettings))
            {
                if (this.ExactlyOnce && this.ReceiveContextEnabled)
                {
                    return (T) this.ReceiveContextSettings;
                }
                return default(T);
            }
            if (typeof(T) == typeof(ITransactedBindingElement))
            {
                return (T) this;
            }
            return base.GetProperty<T>(context);
        }

        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            ICollection<XmlElement> bindingAssertions = policyContext.GetBindingAssertions();
            if (FindAssertion(bindingAssertions, "MsmqVolatile"))
            {
                this.Durable = false;
            }
            if (FindAssertion(bindingAssertions, "MsmqBestEffort"))
            {
                this.ExactlyOnce = false;
            }
            if (FindAssertion(bindingAssertions, "MsmqSession"))
            {
                policyContext.Contract.SessionMode = SessionMode.Required;
            }
            if (FindAssertion(bindingAssertions, "Authenticated"))
            {
                this.MsmqTransportSecurity.MsmqProtectionLevel = ProtectionLevel.Sign;
                if (FindAssertion(bindingAssertions, "WindowsDomain"))
                {
                    this.MsmqTransportSecurity.MsmqAuthenticationMode = MsmqAuthenticationMode.WindowsDomain;
                }
                else
                {
                    this.MsmqTransportSecurity.MsmqAuthenticationMode = MsmqAuthenticationMode.Certificate;
                }
            }
            else
            {
                this.MsmqTransportSecurity.MsmqProtectionLevel = ProtectionLevel.None;
                this.MsmqTransportSecurity.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
            }
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
            XmlDocument document = new XmlDocument();
            ICollection<XmlElement> bindingAssertions = context.GetBindingAssertions();
            if (!this.Durable)
            {
                bindingAssertions.Add(document.CreateElement("msmq", "MsmqVolatile", "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));
            }
            if (!this.ExactlyOnce)
            {
                bindingAssertions.Add(document.CreateElement("msmq", "MsmqBestEffort", "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));
            }
            if (context.Contract.SessionMode == SessionMode.Required)
            {
                bindingAssertions.Add(document.CreateElement("msmq", "MsmqSession", "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));
            }
            if (this.MsmqTransportSecurity.MsmqProtectionLevel != ProtectionLevel.None)
            {
                bindingAssertions.Add(document.CreateElement("msmq", "Authenticated", "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));
                if (this.MsmqTransportSecurity.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain)
                {
                    bindingAssertions.Add(document.CreateElement("msmq", "WindowsDomain", "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));
                }
            }
            MessageEncodingBindingElement element = this.FindMessageEncodingBindingElement(context.BindingElements, out flag);
            if (flag && (element is IPolicyExportExtension))
            {
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

        internal abstract MsmqUri.IAddressTranslator AddressTranslator { get; }

        public Uri CustomDeadLetterQueue
        {
            get
            {
                return this.customDeadLetterQueue;
            }
            set
            {
                this.customDeadLetterQueue = value;
            }
        }

        public System.ServiceModel.DeadLetterQueue DeadLetterQueue
        {
            get
            {
                return this.deadLetterQueue;
            }
            set
            {
                if (!DeadLetterQueueHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.deadLetterQueue = value;
            }
        }

        public bool Durable
        {
            get
            {
                return this.durable;
            }
            set
            {
                this.durable = value;
            }
        }

        public bool ExactlyOnce
        {
            get
            {
                return this.exactlyOnce;
            }
            set
            {
                this.exactlyOnce = value;
            }
        }

        public int MaxRetryCycles
        {
            get
            {
                return this.maxRetryCycles;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("MsmqNonNegativeArgumentExpected")));
                }
                this.maxRetryCycles = value;
            }
        }

        public System.ServiceModel.MsmqTransportSecurity MsmqTransportSecurity
        {
            get
            {
                return this.msmqTransportSecurity;
            }
            internal set
            {
                this.msmqTransportSecurity = value;
            }
        }

        public bool ReceiveContextEnabled
        {
            get
            {
                return this.receiveContextEnabled;
            }
            set
            {
                this.receiveContextEnabled = value;
            }
        }

        internal IReceiveContextSettings ReceiveContextSettings { get; set; }

        public System.ServiceModel.ReceiveErrorHandling ReceiveErrorHandling
        {
            get
            {
                return this.receiveErrorHandling;
            }
            set
            {
                if (!ReceiveErrorHandlingHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.receiveErrorHandling = value;
            }
        }

        public int ReceiveRetryCount
        {
            get
            {
                return this.receiveRetryCount;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("MsmqNonNegativeArgumentExpected")));
                }
                this.receiveRetryCount = value;
            }
        }

        public TimeSpan RetryCycleDelay
        {
            get
            {
                return this.retryCycleDelay;
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
                this.retryCycleDelay = value;
            }
        }

        public TimeSpan TimeToLive
        {
            get
            {
                return this.timeToLive;
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
                this.timeToLive = value;
            }
        }

        public bool TransactedReceiveEnabled
        {
            get
            {
                return this.exactlyOnce;
            }
        }

        public bool UseMsmqTracing
        {
            get
            {
                return this.useMsmqTracing;
            }
            set
            {
                this.useMsmqTracing = value;
            }
        }

        public bool UseSourceJournal
        {
            get
            {
                return this.useSourceJournal;
            }
            set
            {
                this.useSourceJournal = value;
            }
        }

        public TimeSpan ValidityDuration
        {
            get
            {
                return this.ReceiveContextSettings.ValidityDuration;
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
                ((MsmqReceiveContextSettings) this.ReceiveContextSettings).SetValidityDuration(value);
            }
        }

        internal virtual string WsdlTransportUri
        {
            get
            {
                return null;
            }
        }

        private class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }

            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get
                {
                    return false;
                }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

