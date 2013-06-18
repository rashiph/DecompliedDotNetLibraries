namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.PeerResolvers;

    public sealed class PeerTransportBindingElement : TransportBindingElement, IWsdlExportExtension, ITransportPolicyImport, IPolicyExportExtension
    {
        private IPAddress listenIPAddress;
        private PeerSecuritySettings peerSecurity;
        private int port;
        private PeerResolver resolver;
        private bool resolverSet;

        public PeerTransportBindingElement()
        {
            this.listenIPAddress = null;
            this.port = 0;
            if (PeerTransportDefaults.ResolverAvailable)
            {
                this.resolver = PeerTransportDefaults.CreateResolver();
            }
            this.peerSecurity = new PeerSecuritySettings();
        }

        private PeerTransportBindingElement(PeerTransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.listenIPAddress = elementToBeCloned.listenIPAddress;
            this.port = elementToBeCloned.port;
            this.resolverSet = elementToBeCloned.resolverSet;
            this.resolver = elementToBeCloned.resolver;
            this.peerSecurity = new PeerSecuritySettings(elementToBeCloned.Security);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            if (base.ManualAddressing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ManualAddressingNotSupported")));
            }
            return new PeerChannelFactory<TChannel>(this, context, this.GetResolver(context));
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            PeerChannelListenerBase base2 = null;
            PeerResolver peerResolver = this.GetResolver(context);
            if (typeof(TChannel) == typeof(IInputChannel))
            {
                base2 = new PeerInputChannelListener(this, context, peerResolver);
            }
            else
            {
                if (typeof(TChannel) != typeof(IDuplexChannel))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
                }
                base2 = new PeerDuplexChannelListener(this, context, peerResolver);
            }
            return (IChannelListener<TChannel>) base2;
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (!(typeof(TChannel) == typeof(IOutputChannel)))
            {
                return (typeof(TChannel) == typeof(IDuplexChannel));
            }
            return true;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (!(typeof(TChannel) == typeof(IInputChannel)))
            {
                return (typeof(TChannel) == typeof(IDuplexChannel));
            }
            return true;
        }

        public override BindingElement Clone()
        {
            return new PeerTransportBindingElement(this);
        }

        internal void CreateDefaultResolver(PeerResolverSettings settings)
        {
            if (PeerTransportDefaults.ResolverAvailable)
            {
                this.resolver = new PnrpPeerResolver(settings.ReferralPolicy);
            }
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
            if (typeof(T) == typeof(IBindingMulticastCapabilities))
            {
                return (T) new BindingMulticastCapabilities();
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) new SecurityCapabilities(this.Security.SupportsAuthentication, this.Security.SupportsAuthentication, false, this.Security.SupportedProtectionLevel, this.Security.SupportedProtectionLevel);
            }
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T) new BindingDeliveryCapabilitiesHelper();
            }
            return base.GetProperty<T>(context);
        }

        private PeerResolver GetResolver(BindingContext context)
        {
            if (this.resolverSet)
            {
                return this.resolver;
            }
            Collection<PeerCustomResolverBindingElement> collection = context.BindingParameters.FindAll<PeerCustomResolverBindingElement>();
            if (collection.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MultiplePeerCustomResolverBindingElementsInParameters")));
            }
            if (collection.Count == 1)
            {
                context.BindingParameters.Remove<PeerCustomResolverBindingElement>();
                return collection[0].CreatePeerResolver();
            }
            Collection<PeerResolverBindingElement> collection2 = context.BindingParameters.FindAll<PeerResolverBindingElement>();
            if (collection2.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MultiplePeerResolverBindingElementsinParameters")));
            }
            if (collection2.Count == 0)
            {
                if (this.resolver == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerResolverBindingElementRequired", new object[] { context.Binding.Name })));
                }
                return this.resolver;
            }
            if (collection2[0].GetType() == PeerTransportDefaults.ResolverBindingElementType)
            {
                if (!PeerTransportDefaults.ResolverInstalled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerPnrpNotInstalled")));
                }
                if (!PeerTransportDefaults.ResolverAvailable)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerPnrpNotAvailable")));
                }
            }
            context.BindingParameters.Remove<PeerResolverBindingElement>();
            return collection2[0].CreatePeerResolver();
        }

        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.peerSecurity.OnImportPolicy(importer, context);
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
            this.peerSecurity.OnExportPolicy(exporter, context);
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
            TransportBindingElement.ExportWsdlEndpoint(exporter, endpointContext, "http://schemas.microsoft.com/soap/peer", element.MessageVersion.Addressing);
        }

        public IPAddress ListenIPAddress
        {
            get
            {
                return this.listenIPAddress;
            }
            set
            {
                PeerValidateHelper.ValidateListenIPAddress(value);
                this.listenIPAddress = value;
            }
        }

        public override long MaxReceivedMessageSize
        {
            get
            {
                return base.MaxReceivedMessageSize;
            }
            set
            {
                PeerValidateHelper.ValidateMaxMessageSize(value);
                base.MaxReceivedMessageSize = value;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                PeerValidateHelper.ValidatePort(value);
                this.port = value;
            }
        }

        internal PeerResolver Resolver
        {
            get
            {
                return this.resolver;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value.GetType() == PeerTransportDefaults.ResolverType)
                {
                    if (!PeerTransportDefaults.ResolverInstalled)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.ServiceModel.SR.GetString("PeerPnrpNotInstalled"));
                    }
                    if (!PeerTransportDefaults.ResolverAvailable)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.ServiceModel.SR.GetString("PeerPnrpNotAvailable"));
                    }
                }
                this.resolver = value;
                this.resolverSet = true;
            }
        }

        public override string Scheme
        {
            get
            {
                return "net.p2p";
            }
        }

        public PeerSecuritySettings Security
        {
            get
            {
                return this.peerSecurity;
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
                    return false;
                }
            }
        }

        private class BindingMulticastCapabilities : IBindingMulticastCapabilities
        {
            public bool IsMulticast
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

