namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;

    public class NetTcpBinding : Binding, IBindingRuntimePreferences
    {
        private TransactionFlowBindingElement context;
        private BinaryMessageEncodingBindingElement encoding;
        private OptionalReliableSession reliableSession;
        private NetTcpSecurity security;
        private ReliableSessionBindingElement session;
        private TcpTransportBindingElement transport;

        public NetTcpBinding()
        {
            this.security = new NetTcpSecurity();
            this.Initialize();
        }

        public NetTcpBinding(SecurityMode securityMode) : this()
        {
            this.security.Mode = securityMode;
        }

        public NetTcpBinding(string configurationName) : this()
        {
            this.ApplyConfiguration(configurationName);
        }

        public NetTcpBinding(SecurityMode securityMode, bool reliableSessionEnabled) : this(securityMode)
        {
            this.ReliableSession.Enabled = reliableSessionEnabled;
        }

        private NetTcpBinding(TcpTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context, ReliableSessionBindingElement session, NetTcpSecurity security) : this()
        {
            this.security = security;
            this.ReliableSession.Enabled = session != null;
            this.InitializeFrom(transport, encoding, context, session);
        }

        private void ApplyConfiguration(string configurationName)
        {
            NetTcpBindingElement element2 = NetTcpBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "netTcpBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection {
                this.context
            };
            if (this.reliableSession.Enabled)
            {
                elements.Add(this.session);
            }
            SecurityBindingElement item = this.CreateMessageSecurity();
            if (item != null)
            {
                elements.Add(item);
            }
            elements.Add(this.encoding);
            BindingElement element2 = this.CreateTransportSecurity();
            if (element2 != null)
            {
                elements.Add(element2);
            }
            this.transport.ExtendedProtectionPolicy = this.security.Transport.ExtendedProtectionPolicy;
            elements.Add(this.transport);
            return elements.Clone();
        }

        private SecurityBindingElement CreateMessageSecurity()
        {
            if ((this.security.Mode != SecurityMode.Message) && (this.security.Mode != SecurityMode.TransportWithMessageCredential))
            {
                return null;
            }
            return this.security.CreateMessageSecurity(this.ReliableSession.Enabled);
        }

        private BindingElement CreateTransportSecurity()
        {
            return this.security.CreateTransportSecurity();
        }

        private static TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            return new TransactionFlowBindingElement(false);
        }

        private static UnifiedSecurityMode GetModeFromTransportSecurity(BindingElement transport)
        {
            return NetTcpSecurity.GetModeFromTransportSecurity(transport);
        }

        private void Initialize()
        {
            this.transport = new TcpTransportBindingElement();
            this.encoding = new BinaryMessageEncodingBindingElement();
            this.context = GetDefaultTransactionFlowBindingElement();
            this.session = new ReliableSessionBindingElement();
            this.reliableSession = new OptionalReliableSession(this.session);
        }

        private void InitializeFrom(TcpTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context, ReliableSessionBindingElement session)
        {
            this.HostNameComparisonMode = transport.HostNameComparisonMode;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxBufferSize = transport.MaxBufferSize;
            this.MaxConnections = transport.MaxPendingConnections;
            this.ListenBacklog = transport.ListenBacklog;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.PortSharingEnabled = transport.PortSharingEnabled;
            this.TransferMode = transport.TransferMode;
            this.ReaderQuotas = encoding.ReaderQuotas;
            this.TransactionFlow = context.Transactions;
            this.TransactionProtocol = context.TransactionProtocol;
            if (session != null)
            {
                this.session.InactivityTimeout = session.InactivityTimeout;
                this.session.Ordered = session.Ordered;
            }
        }

        private bool IsBindingElementsMatch(TcpTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context, ReliableSessionBindingElement session)
        {
            if (!this.transport.IsMatch(transport))
            {
                return false;
            }
            if (!this.encoding.IsMatch(encoding))
            {
                return false;
            }
            if (!this.context.IsMatch(context))
            {
                return false;
            }
            if (this.reliableSession.Enabled)
            {
                if (!this.session.IsMatch(session))
                {
                    return false;
                }
            }
            else if (session != null)
            {
                return false;
            }
            return true;
        }

        private static bool SetTransportSecurity(BindingElement transport, SecurityMode mode, TcpTransportSecurity transportSecurity)
        {
            return NetTcpSecurity.SetTransportSecurity(transport, mode, transportSecurity);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return !EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReliableSession()
        {
            if (this.ReliableSession.Ordered && !(this.ReliableSession.InactivityTimeout != ReliableSessionDefaults.InactivityTimeout))
            {
                return this.ReliableSession.Enabled;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.security.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return (this.TransactionProtocol != NetTcpDefaults.TransactionProtocol);
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            NetTcpSecurity security2;
            binding = null;
            if (elements.Count > 6)
            {
                return false;
            }
            TcpTransportBindingElement transport = null;
            BinaryMessageEncodingBindingElement encoding = null;
            TransactionFlowBindingElement context = null;
            ReliableSessionBindingElement session = null;
            SecurityBindingElement sbe = null;
            BindingElement element6 = null;
            foreach (BindingElement element7 in elements)
            {
                if (element7 is SecurityBindingElement)
                {
                    sbe = element7 as SecurityBindingElement;
                }
                else if (element7 is TransportBindingElement)
                {
                    transport = element7 as TcpTransportBindingElement;
                }
                else if (element7 is MessageEncodingBindingElement)
                {
                    encoding = element7 as BinaryMessageEncodingBindingElement;
                }
                else if (element7 is TransactionFlowBindingElement)
                {
                    context = element7 as TransactionFlowBindingElement;
                }
                else if (element7 is ReliableSessionBindingElement)
                {
                    session = element7 as ReliableSessionBindingElement;
                }
                else
                {
                    if (element6 != null)
                    {
                        return false;
                    }
                    element6 = element7;
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
            if (context == null)
            {
                context = GetDefaultTransactionFlowBindingElement();
            }
            TcpTransportSecurity tcpTransportSecurity = new TcpTransportSecurity();
            UnifiedSecurityMode modeFromTransportSecurity = GetModeFromTransportSecurity(element6);
            if (!TryCreateSecurity(sbe, modeFromTransportSecurity, session != null, element6, tcpTransportSecurity, out security2))
            {
                return false;
            }
            if (!SetTransportSecurity(element6, security2.Mode, tcpTransportSecurity))
            {
                return false;
            }
            NetTcpBinding binding2 = new NetTcpBinding(transport, encoding, context, session, security2);
            if (!binding2.IsBindingElementsMatch(transport, encoding, context, session))
            {
                return false;
            }
            binding = binding2;
            return true;
        }

        private static bool TryCreateSecurity(SecurityBindingElement sbe, UnifiedSecurityMode mode, bool isReliableSession, BindingElement transportSecurity, TcpTransportSecurity tcpTransportSecurity, out NetTcpSecurity security)
        {
            if (sbe != null)
            {
                mode &= UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Message;
            }
            else
            {
                mode &= ~(UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Message);
            }
            SecurityMode mode2 = SecurityModeHelper.ToSecurityMode(mode);
            return NetTcpSecurity.TryCreate(sbe, mode2, isReliableSession, transportSecurity, tcpTransportSecurity, out security);
        }

        public System.ServiceModel.EnvelopeVersion EnvelopeVersion
        {
            get
            {
                return System.ServiceModel.EnvelopeVersion.Soap12;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.transport.HostNameComparisonMode;
            }
            set
            {
                this.transport.HostNameComparisonMode = value;
            }
        }

        [DefaultValue(10)]
        public int ListenBacklog
        {
            get
            {
                return this.transport.ListenBacklog;
            }
            set
            {
                this.transport.ListenBacklog = value;
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

        [DefaultValue(0x10000)]
        public int MaxBufferSize
        {
            get
            {
                return this.transport.MaxBufferSize;
            }
            set
            {
                this.transport.MaxBufferSize = value;
            }
        }

        [DefaultValue(10)]
        public int MaxConnections
        {
            get
            {
                return this.transport.MaxPendingConnections;
            }
            set
            {
                this.transport.MaxPendingConnections = value;
                this.transport.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = value;
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

        [DefaultValue(false)]
        public bool PortSharingEnabled
        {
            get
            {
                return this.transport.PortSharingEnabled;
            }
            set
            {
                this.transport.PortSharingEnabled = value;
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

        public OptionalReliableSession ReliableSession
        {
            get
            {
                return this.reliableSession;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.reliableSession.CopySettings(value);
            }
        }

        public override string Scheme
        {
            get
            {
                return this.transport.Scheme;
            }
        }

        public NetTcpSecurity Security
        {
            get
            {
                return this.security;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.security = value;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get
            {
                return false;
            }
        }

        [DefaultValue(false)]
        public bool TransactionFlow
        {
            get
            {
                return this.context.Transactions;
            }
            set
            {
                this.context.Transactions = value;
            }
        }

        public System.ServiceModel.TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.context.TransactionProtocol;
            }
            set
            {
                this.context.TransactionProtocol = value;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.TransferMode TransferMode
        {
            get
            {
                return this.transport.TransferMode;
            }
            set
            {
                this.transport.TransferMode = value;
            }
        }
    }
}

