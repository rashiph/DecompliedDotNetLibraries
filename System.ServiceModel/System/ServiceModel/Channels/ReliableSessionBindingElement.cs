namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class ReliableSessionBindingElement : BindingElement, IPolicyExportExtension
    {
        private TimeSpan acknowledgementInterval;
        private static MessagePartSpecification bodyOnly;
        private bool flowControlEnabled;
        private TimeSpan inactivityTimeout;
        private InternalDuplexBindingElement internalDuplexBindingElement;
        private int maxPendingChannels;
        private int maxRetryCount;
        private int maxTransferWindowSize;
        private bool ordered;
        private System.ServiceModel.ReliableMessagingVersion reliableMessagingVersion;

        public ReliableSessionBindingElement()
        {
            this.acknowledgementInterval = ReliableSessionDefaults.AcknowledgementInterval;
            this.flowControlEnabled = true;
            this.inactivityTimeout = ReliableSessionDefaults.InactivityTimeout;
            this.maxPendingChannels = 4;
            this.maxRetryCount = 8;
            this.maxTransferWindowSize = 8;
            this.ordered = true;
            this.reliableMessagingVersion = System.ServiceModel.ReliableMessagingVersion.Default;
        }

        public ReliableSessionBindingElement(bool ordered)
        {
            this.acknowledgementInterval = ReliableSessionDefaults.AcknowledgementInterval;
            this.flowControlEnabled = true;
            this.inactivityTimeout = ReliableSessionDefaults.InactivityTimeout;
            this.maxPendingChannels = 4;
            this.maxRetryCount = 8;
            this.maxTransferWindowSize = 8;
            this.ordered = true;
            this.reliableMessagingVersion = System.ServiceModel.ReliableMessagingVersion.Default;
            this.ordered = ordered;
        }

        internal ReliableSessionBindingElement(ReliableSessionBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.acknowledgementInterval = ReliableSessionDefaults.AcknowledgementInterval;
            this.flowControlEnabled = true;
            this.inactivityTimeout = ReliableSessionDefaults.InactivityTimeout;
            this.maxPendingChannels = 4;
            this.maxRetryCount = 8;
            this.maxTransferWindowSize = 8;
            this.ordered = true;
            this.reliableMessagingVersion = System.ServiceModel.ReliableMessagingVersion.Default;
            this.AcknowledgementInterval = elementToBeCloned.AcknowledgementInterval;
            this.FlowControlEnabled = elementToBeCloned.FlowControlEnabled;
            this.InactivityTimeout = elementToBeCloned.InactivityTimeout;
            this.MaxPendingChannels = elementToBeCloned.MaxPendingChannels;
            this.MaxRetryCount = elementToBeCloned.MaxRetryCount;
            this.MaxTransferWindowSize = elementToBeCloned.MaxTransferWindowSize;
            this.Ordered = elementToBeCloned.Ordered;
            this.ReliableMessagingVersion = elementToBeCloned.ReliableMessagingVersion;
            this.internalDuplexBindingElement = elementToBeCloned.internalDuplexBindingElement;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.VerifyTransportMode(context);
            this.SetSecuritySettings(context);
            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref this.internalDuplexBindingElement);
            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                if (context.CanBuildInnerChannelFactory<IRequestSessionChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IRequestSessionChannel>(this, context.BuildInnerChannelFactory<IRequestSessionChannel>(), context.Binding);
                }
                if (context.CanBuildInnerChannelFactory<IRequestChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IRequestChannel>(this, context.BuildInnerChannelFactory<IRequestChannel>(), context.Binding);
                }
                if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IDuplexSessionChannel>(this, context.BuildInnerChannelFactory<IDuplexSessionChannel>(), context.Binding);
                }
                if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IDuplexChannel>(this, context.BuildInnerChannelFactory<IDuplexChannel>(), context.Binding);
                }
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IDuplexSessionChannel>(this, context.BuildInnerChannelFactory<IDuplexSessionChannel>(), context.Binding);
                }
                if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IDuplexChannel>(this, context.BuildInnerChannelFactory<IDuplexChannel>(), context.Binding);
                }
            }
            else if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                if (context.CanBuildInnerChannelFactory<IRequestSessionChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IRequestSessionChannel>(this, context.BuildInnerChannelFactory<IRequestSessionChannel>(), context.Binding);
                }
                if (context.CanBuildInnerChannelFactory<IRequestChannel>())
                {
                    return (IChannelFactory<TChannel>) new ReliableChannelFactory<TChannel, IRequestChannel>(this, context.BuildInnerChannelFactory<IRequestChannel>(), context.Binding);
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.VerifyTransportMode(context);
            this.SetSecuritySettings(context);
            IMessageFilterTable<EndpointAddress> table = context.BindingParameters.Find<IMessageFilterTable<EndpointAddress>>();
            InternalDuplexBindingElement.AddDuplexListenerSupport(context, ref this.internalDuplexBindingElement);
            if (typeof(TChannel) == typeof(IInputSessionChannel))
            {
                ReliableChannelListenerBase<IInputSessionChannel> base2 = null;
                if (context.CanBuildInnerChannelListener<IReplySessionChannel>())
                {
                    base2 = new ReliableInputListenerOverReplySession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IReplyChannel>())
                {
                    base2 = new ReliableInputListenerOverReply(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
                {
                    base2 = new ReliableInputListenerOverDuplexSession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IDuplexChannel>())
                {
                    base2 = new ReliableInputListenerOverDuplex(this, context);
                }
                if (base2 != null)
                {
                    base2.LocalAddresses = table;
                    return (IChannelListener<TChannel>) base2;
                }
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                ReliableChannelListenerBase<IDuplexSessionChannel> base3 = null;
                if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
                {
                    base3 = new ReliableDuplexListenerOverDuplexSession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IDuplexChannel>())
                {
                    base3 = new ReliableDuplexListenerOverDuplex(this, context);
                }
                if (base3 != null)
                {
                    base3.LocalAddresses = table;
                    return (IChannelListener<TChannel>) base3;
                }
            }
            else if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                ReliableChannelListenerBase<IReplySessionChannel> base4 = null;
                if (context.CanBuildInnerChannelListener<IReplySessionChannel>())
                {
                    base4 = new ReliableReplyListenerOverReplySession(this, context);
                }
                else if (context.CanBuildInnerChannelListener<IReplyChannel>())
                {
                    base4 = new ReliableReplyListenerOverReply(this, context);
                }
                if (base4 != null)
                {
                    base4.LocalAddresses = table;
                    return (IChannelListener<TChannel>) base4;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref this.internalDuplexBindingElement);
            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                if ((!context.CanBuildInnerChannelFactory<IRequestSessionChannel>() && !context.CanBuildInnerChannelFactory<IRequestChannel>()) && !context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
                {
                    return context.CanBuildInnerChannelFactory<IDuplexChannel>();
                }
                return true;
            }
            if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                if (!context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
                {
                    return context.CanBuildInnerChannelFactory<IDuplexChannel>();
                }
                return true;
            }
            if (!(typeof(TChannel) == typeof(IRequestSessionChannel)))
            {
                return false;
            }
            if (!context.CanBuildInnerChannelFactory<IRequestSessionChannel>())
            {
                return context.CanBuildInnerChannelFactory<IRequestChannel>();
            }
            return true;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            InternalDuplexBindingElement.AddDuplexListenerSupport(context, ref this.internalDuplexBindingElement);
            if (typeof(TChannel) == typeof(IInputSessionChannel))
            {
                if ((!context.CanBuildInnerChannelListener<IReplySessionChannel>() && !context.CanBuildInnerChannelListener<IReplyChannel>()) && !context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
                {
                    return context.CanBuildInnerChannelListener<IDuplexChannel>();
                }
                return true;
            }
            if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                if (!context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
                {
                    return context.CanBuildInnerChannelListener<IDuplexChannel>();
                }
                return true;
            }
            if (!(typeof(TChannel) == typeof(IReplySessionChannel)))
            {
                return false;
            }
            if (!context.CanBuildInnerChannelListener<IReplySessionChannel>())
            {
                return context.CanBuildInnerChannelListener<IReplyChannel>();
            }
            return true;
        }

        public override BindingElement Clone()
        {
            return new ReliableSessionBindingElement(this);
        }

        private static XmlElement CreatePolicyElement(PolicyVersion policyVersion, XmlDocument doc)
        {
            string localName = "Policy";
            string namespaceURI = policyVersion.Namespace;
            string prefix = "wsp";
            return doc.CreateElement(prefix, localName, namespaceURI);
        }

        private XmlElement CreateReliabilityAssertion(PolicyVersion policyVersion, BindingElementCollection bindingElements)
        {
            string str;
            string str2;
            string str3;
            string str4;
            XmlDocument doc = new XmlDocument();
            XmlElement childElement = null;
            if (this.ReliableMessagingVersion == System.ServiceModel.ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                str = "wsrm";
                str2 = "http://schemas.xmlsoap.org/ws/2005/02/rm/policy";
                str3 = str;
                str4 = str2;
            }
            else
            {
                str = "wsrmp";
                str2 = "http://docs.oasis-open.org/ws-rx/wsrmp/200702";
                str3 = "netrmp";
                str4 = "http://schemas.microsoft.com/ws-rx/wsrmp/200702";
            }
            XmlElement element2 = doc.CreateElement(str, "RMAssertion", str2);
            if (this.ReliableMessagingVersion == System.ServiceModel.ReliableMessagingVersion.WSReliableMessaging11)
            {
                XmlElement newChild = CreatePolicyElement(policyVersion, doc);
                if (IsSecureConversationEnabled(bindingElements))
                {
                    XmlElement element4 = doc.CreateElement(str, "SequenceSTR", str2);
                    newChild.AppendChild(element4);
                }
                XmlElement element5 = doc.CreateElement(str, "DeliveryAssurance", str2);
                XmlElement element6 = CreatePolicyElement(policyVersion, doc);
                XmlElement element7 = doc.CreateElement(str, "ExactlyOnce", str2);
                element6.AppendChild(element7);
                if (this.ordered)
                {
                    XmlElement element8 = doc.CreateElement(str, "InOrder", str2);
                    element6.AppendChild(element8);
                }
                element5.AppendChild(element6);
                newChild.AppendChild(element5);
                element2.AppendChild(newChild);
            }
            childElement = doc.CreateElement(str3, "InactivityTimeout", str4);
            WriteMillisecondsAttribute(childElement, this.InactivityTimeout);
            element2.AppendChild(childElement);
            childElement = doc.CreateElement(str3, "AcknowledgementInterval", str4);
            WriteMillisecondsAttribute(childElement, this.AcknowledgementInterval);
            element2.AppendChild(childElement);
            return element2;
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                ChannelProtectionRequirements protectionRequirements = this.GetProtectionRequirements();
                protectionRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
                return (T) protectionRequirements;
            }
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T) new BindingDeliveryCapabilitiesHelper(this, context.GetInnerProperty<IBindingDeliveryCapabilities>());
            }
            return context.GetInnerProperty<T>();
        }

        private ChannelProtectionRequirements GetProtectionRequirements()
        {
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            MessagePartSpecification signedReliabilityMessageParts = WsrmIndex.GetSignedReliabilityMessageParts(this.reliableMessagingVersion);
            requirements.IncomingSignatureParts.AddParts(signedReliabilityMessageParts);
            requirements.OutgoingSignatureParts.AddParts(signedReliabilityMessageParts);
            if (this.reliableMessagingVersion == System.ServiceModel.ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                ScopedMessagePartSpecification signaturePart = requirements.IncomingSignatureParts;
                ScopedMessagePartSpecification encryptionPart = requirements.IncomingEncryptionParts;
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/AckRequested");
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequence");
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/SequenceAcknowledgement");
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage");
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/TerminateSequence");
                signaturePart = requirements.OutgoingSignatureParts;
                encryptionPart = requirements.OutgoingEncryptionParts;
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequenceResponse");
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/SequenceAcknowledgement");
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage");
                ProtectProtocolMessage(signaturePart, encryptionPart, "http://schemas.xmlsoap.org/ws/2005/02/rm/TerminateSequence");
                return requirements;
            }
            if (this.reliableMessagingVersion != System.ServiceModel.ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
            ScopedMessagePartSpecification incomingSignatureParts = requirements.IncomingSignatureParts;
            ScopedMessagePartSpecification incomingEncryptionParts = requirements.IncomingEncryptionParts;
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/AckRequested");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequence");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequence");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/fault");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/SequenceAcknowledgement");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequence");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequenceResponse");
            incomingSignatureParts = requirements.OutgoingSignatureParts;
            incomingEncryptionParts = requirements.OutgoingEncryptionParts;
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/AckRequested");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequence");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequenceResponse");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/fault");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/SequenceAcknowledgement");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequence");
            ProtectProtocolMessage(incomingSignatureParts, incomingEncryptionParts, "http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequenceResponse");
            return requirements;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            ReliableSessionBindingElement element = b as ReliableSessionBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.acknowledgementInterval != element.acknowledgementInterval)
            {
                return false;
            }
            if (this.flowControlEnabled != element.flowControlEnabled)
            {
                return false;
            }
            if (this.inactivityTimeout != element.inactivityTimeout)
            {
                return false;
            }
            if (this.maxPendingChannels != element.maxPendingChannels)
            {
                return false;
            }
            if (this.maxRetryCount != element.maxRetryCount)
            {
                return false;
            }
            if (this.maxTransferWindowSize != element.maxTransferWindowSize)
            {
                return false;
            }
            if (this.ordered != element.ordered)
            {
                return false;
            }
            if (this.reliableMessagingVersion != element.reliableMessagingVersion)
            {
                return false;
            }
            return true;
        }

        private static bool IsSecureConversationEnabled(BindingElementCollection bindingElements)
        {
            bool flag = false;
            for (int i = 0; i < bindingElements.Count; i++)
            {
                if (!flag)
                {
                    ReliableSessionBindingElement element = bindingElements[i] as ReliableSessionBindingElement;
                    flag = element != null;
                }
                else
                {
                    SecurityBindingElement element3;
                    SecurityBindingElement sbe = bindingElements[i] as SecurityBindingElement;
                    if (sbe == null)
                    {
                        break;
                    }
                    if (!SecurityBindingElement.IsSecureConversationBinding(sbe, true, out element3))
                    {
                        return SecurityBindingElement.IsSecureConversationBinding(sbe, false, out element3);
                    }
                    return true;
                }
            }
            return false;
        }

        private static void ProtectProtocolMessage(ScopedMessagePartSpecification signaturePart, ScopedMessagePartSpecification encryptionPart, string action)
        {
            signaturePart.AddParts(BodyOnly, action);
            encryptionPart.AddParts(MessagePartSpecification.NoParts, action);
        }

        private void SetSecuritySettings(BindingContext context)
        {
            SecurityBindingElement element = context.RemainingBindingElements.Find<SecurityBindingElement>();
            if (element != null)
            {
                element.LocalServiceSettings.ReconnectTransportOnFailure = true;
            }
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context.BindingElements != null)
            {
                BindingElementCollection bindingElements = context.BindingElements;
                ReliableSessionBindingElement element = bindingElements.Find<ReliableSessionBindingElement>();
                if (element != null)
                {
                    XmlElement item = element.CreateReliabilityAssertion(exporter.PolicyVersion, bindingElements);
                    context.GetBindingAssertions().Add(item);
                }
            }
        }

        private void VerifyTransportMode(BindingContext context)
        {
            TransferMode transferMode;
            TransportBindingElement element = context.RemainingBindingElements.Find<TransportBindingElement>();
            if ((element != null) && element.ManualAddressing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ManualAddressingNotSupported")));
            }
            ConnectionOrientedTransportBindingElement element2 = element as ConnectionOrientedTransportBindingElement;
            HttpTransportBindingElement element3 = element as HttpTransportBindingElement;
            if (element2 != null)
            {
                transferMode = element2.TransferMode;
            }
            else if (element3 != null)
            {
                transferMode = element3.TransferMode;
            }
            else
            {
                transferMode = TransferMode.Buffered;
            }
            if (transferMode != TransferMode.Buffered)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransferModeNotSupported", new object[] { transferMode, base.GetType().Name })));
            }
        }

        private static void WriteMillisecondsAttribute(XmlElement childElement, TimeSpan timeSpan)
        {
            ulong num = Convert.ToUInt64(timeSpan.TotalMilliseconds);
            childElement.SetAttribute("Milliseconds", XmlConvert.ToString(num));
        }

        [DefaultValue(typeof(TimeSpan), "00:00:00.2")]
        public TimeSpan AcknowledgementInterval
        {
            get
            {
                return this.acknowledgementInterval;
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
                this.acknowledgementInterval = value;
            }
        }

        private static MessagePartSpecification BodyOnly
        {
            get
            {
                if (bodyOnly == null)
                {
                    MessagePartSpecification specification = new MessagePartSpecification(true);
                    specification.MakeReadOnly();
                    bodyOnly = specification;
                }
                return bodyOnly;
            }
        }

        [DefaultValue(true)]
        public bool FlowControlEnabled
        {
            get
            {
                return this.flowControlEnabled;
            }
            set
            {
                this.flowControlEnabled = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), "00:10:00")]
        public TimeSpan InactivityTimeout
        {
            get
            {
                return this.inactivityTimeout;
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
                this.inactivityTimeout = value;
            }
        }

        [DefaultValue(4)]
        public int MaxPendingChannels
        {
            get
            {
                return this.maxPendingChannels;
            }
            set
            {
                if ((value <= 0) || (value > 0x4000))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, 0x4000 })));
                }
                this.maxPendingChannels = value;
            }
        }

        [DefaultValue(8)]
        public int MaxRetryCount
        {
            get
            {
                return this.maxRetryCount;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxRetryCount = value;
            }
        }

        [DefaultValue(8)]
        public int MaxTransferWindowSize
        {
            get
            {
                return this.maxTransferWindowSize;
            }
            set
            {
                if ((value <= 0) || (value > 0x1000))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, 0x1000 })));
                }
                this.maxTransferWindowSize = value;
            }
        }

        [DefaultValue(true)]
        public bool Ordered
        {
            get
            {
                return this.ordered;
            }
            set
            {
                this.ordered = value;
            }
        }

        [DefaultValue(typeof(System.ServiceModel.ReliableMessagingVersion), "WSReliableMessagingFebruary2005")]
        public System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion
        {
            get
            {
                return this.reliableMessagingVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (!System.ServiceModel.ReliableMessagingVersion.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.reliableMessagingVersion = value;
            }
        }

        private class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            private ReliableSessionBindingElement element;
            private IBindingDeliveryCapabilities inner;

            internal BindingDeliveryCapabilitiesHelper(ReliableSessionBindingElement element, IBindingDeliveryCapabilities inner)
            {
                this.element = element;
                this.inner = inner;
            }

            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get
                {
                    return this.element.Ordered;
                }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get
                {
                    if (this.inner == null)
                    {
                        return false;
                    }
                    return this.inner.QueuedDelivery;
                }
            }
        }
    }
}

