namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.PeerResolvers;
    using System.Xml;

    public class NetPeerTcpBinding : Binding, IBindingRuntimePreferences
    {
        private BinaryMessageEncodingBindingElement encoding;
        private PeerSecuritySettings peerSecurity;
        private PeerResolverSettings resolverSettings;
        private PeerTransportBindingElement transport;

        public NetPeerTcpBinding()
        {
            this.Initialize();
        }

        public NetPeerTcpBinding(string configurationName) : this()
        {
            this.ApplyConfiguration(configurationName);
        }

        private void ApplyConfiguration(string configurationName)
        {
            NetPeerTcpBindingElement element2 = NetPeerTcpBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            this.resolverSettings = new PeerResolverSettings();
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "netPeerTcpBinding" })));
            }
            element2.ApplyConfiguration(this);
            this.transport.CreateDefaultResolver(this.Resolver);
        }

        private bool CanUseCustomResolver()
        {
            return ((this.Resolver.Custom.Resolver != null) || (this.Resolver.Custom.IsBindingSpecified && (this.Resolver.Custom.Address != null)));
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            switch (this.Resolver.Mode)
            {
                case PeerResolverMode.Auto:
                    if (!this.CanUseCustomResolver())
                    {
                        if (!PeerTransportDefaults.ResolverAvailable)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerResolverRequired")));
                        }
                        elements.Add(new PnrpPeerResolverBindingElement(this.Resolver.ReferralPolicy));
                        break;
                    }
                    elements.Add(new PeerCustomResolverBindingElement(this.Resolver.Custom));
                    break;

                case PeerResolverMode.Pnrp:
                    if (!PeerTransportDefaults.ResolverAvailable)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerResolverRequired")));
                    }
                    elements.Add(new PnrpPeerResolverBindingElement(this.Resolver.ReferralPolicy));
                    break;

                case PeerResolverMode.Custom:
                    if (!this.CanUseCustomResolver())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerResolverSettingsInvalid")));
                    }
                    elements.Add(new PeerCustomResolverBindingElement(this.Resolver.Custom));
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerResolverRequired")));
            }
            elements.Add(this.encoding);
            elements.Add(this.transport);
            this.transport.Security.Mode = this.Security.Mode;
            this.transport.Security.Transport.CredentialType = this.Security.Transport.CredentialType;
            return elements.Clone();
        }

        private void Initialize()
        {
            this.resolverSettings = new PeerResolverSettings();
            this.transport = new PeerTransportBindingElement();
            this.encoding = new BinaryMessageEncodingBindingElement();
            this.peerSecurity = new PeerSecuritySettings();
        }

        private void InitializeFrom(PeerTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding)
        {
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.ListenIPAddress = transport.ListenIPAddress;
            this.Port = transport.Port;
            this.Security.Mode = transport.Security.Mode;
            this.ReaderQuotas = encoding.ReaderQuotas;
        }

        private bool IsBindingElementsMatch(PeerTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding)
        {
            if (!this.transport.IsMatch(transport))
            {
                return false;
            }
            if (!this.encoding.IsMatch(encoding))
            {
                return false;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return !EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count != 3)
            {
                return false;
            }
            PeerResolverBindingElement element = null;
            PeerTransportBindingElement transport = null;
            BinaryMessageEncodingBindingElement encoding = null;
            foreach (BindingElement element4 in elements)
            {
                if (element4 is TransportBindingElement)
                {
                    transport = element4 as PeerTransportBindingElement;
                }
                else if (element4 is BinaryMessageEncodingBindingElement)
                {
                    encoding = element4 as BinaryMessageEncodingBindingElement;
                }
                else if (element4 is PeerResolverBindingElement)
                {
                    element = element4 as PeerResolverBindingElement;
                }
                else
                {
                    return false;
                }
            }
            if (transport == null)
            {
                return false;
            }
            if (encoding == null)
            {
                return false;
            }
            if (element == null)
            {
                return false;
            }
            NetPeerTcpBinding binding2 = new NetPeerTcpBinding();
            binding2.InitializeFrom(transport, encoding);
            if (!binding2.IsBindingElementsMatch(transport, encoding))
            {
                return false;
            }
            PeerCustomResolverBindingElement element5 = element as PeerCustomResolverBindingElement;
            if (element5 != null)
            {
                binding2.Resolver.Custom.Address = element5.Address;
                binding2.Resolver.Custom.Binding = element5.Binding;
                binding2.Resolver.Custom.Resolver = element5.CreatePeerResolver();
            }
            else if ((element is PnrpPeerResolverBindingElement) && IsPnrpAvailable)
            {
                binding2.Resolver.Mode = PeerResolverMode.Pnrp;
            }
            binding = binding2;
            return true;
        }

        public System.ServiceModel.EnvelopeVersion EnvelopeVersion
        {
            get
            {
                return System.ServiceModel.EnvelopeVersion.Soap12;
            }
        }

        public static bool IsPnrpAvailable
        {
            get
            {
                return PnrpPeerResolver.IsPnrpAvailable;
            }
        }

        [TypeConverter(typeof(PeerTransportListenAddressConverter)), DefaultValue(null)]
        public IPAddress ListenIPAddress
        {
            get
            {
                return this.transport.ListenIPAddress;
            }
            set
            {
                this.transport.ListenIPAddress = value;
            }
        }

        [DefaultValue((long) 0x80000L)]
        public long MaxBufferPoolSize
        {
            get
            {
                return this.transport.MaxBufferPoolSize;
            }
            set
            {
                this.transport.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue((long) 0x10000L)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return this.transport.MaxReceivedMessageSize;
            }
            set
            {
                this.transport.MaxReceivedMessageSize = value;
            }
        }

        [DefaultValue(0)]
        public int Port
        {
            get
            {
                return this.transport.Port;
            }
            set
            {
                this.transport.Port = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.encoding.ReaderQuotas;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                value.CopyTo(this.encoding.ReaderQuotas);
            }
        }

        public PeerResolverSettings Resolver
        {
            get
            {
                return this.resolverSettings;
            }
        }

        public override string Scheme
        {
            get
            {
                return this.transport.Scheme;
            }
        }

        public PeerSecuritySettings Security
        {
            get
            {
                return this.peerSecurity;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.peerSecurity = value;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get
            {
                return false;
            }
        }
    }
}

