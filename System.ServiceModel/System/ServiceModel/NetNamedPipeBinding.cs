namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;

    public class NetNamedPipeBinding : Binding, IBindingRuntimePreferences
    {
        private TransactionFlowBindingElement context;
        private BinaryMessageEncodingBindingElement encoding;
        private NamedPipeTransportBindingElement namedPipe;
        private NetNamedPipeSecurity security;

        public NetNamedPipeBinding()
        {
            this.security = new NetNamedPipeSecurity();
            this.Initialize();
        }

        private NetNamedPipeBinding(NetNamedPipeSecurity security) : this()
        {
            this.security = security;
        }

        public NetNamedPipeBinding(NetNamedPipeSecurityMode securityMode) : this()
        {
            this.security.Mode = securityMode;
        }

        public NetNamedPipeBinding(string configurationName) : this()
        {
            this.ApplyConfiguration(configurationName);
        }

        private void ApplyConfiguration(string configurationName)
        {
            NetNamedPipeBindingElement element2 = NetNamedPipeBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "netNamedPipeBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection {
                this.context,
                this.encoding
            };
            WindowsStreamSecurityBindingElement item = this.CreateTransportSecurity();
            if (item != null)
            {
                elements.Add(item);
            }
            elements.Add(this.namedPipe);
            return elements.Clone();
        }

        private WindowsStreamSecurityBindingElement CreateTransportSecurity()
        {
            if (this.security.Mode == NetNamedPipeSecurityMode.Transport)
            {
                return this.security.CreateTransportSecurity();
            }
            return null;
        }

        private static TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            return new TransactionFlowBindingElement(false);
        }

        private void Initialize()
        {
            this.namedPipe = new NamedPipeTransportBindingElement();
            this.encoding = new BinaryMessageEncodingBindingElement();
            this.context = GetDefaultTransactionFlowBindingElement();
        }

        private void InitializeFrom(NamedPipeTransportBindingElement namedPipe, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context)
        {
            this.Initialize();
            this.HostNameComparisonMode = namedPipe.HostNameComparisonMode;
            this.MaxBufferPoolSize = namedPipe.MaxBufferPoolSize;
            this.MaxBufferSize = namedPipe.MaxBufferSize;
            this.MaxConnections = namedPipe.MaxPendingConnections;
            this.MaxReceivedMessageSize = namedPipe.MaxReceivedMessageSize;
            this.TransferMode = namedPipe.TransferMode;
            this.ReaderQuotas = encoding.ReaderQuotas;
            this.TransactionFlow = context.Transactions;
            this.TransactionProtocol = context.TransactionProtocol;
        }

        private bool IsBindingElementsMatch(NamedPipeTransportBindingElement namedPipe, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context)
        {
            if (!this.namedPipe.IsMatch(namedPipe))
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
            return ((this.security.Mode != NetNamedPipeSecurityMode.Transport) || (this.security.Transport.ProtectionLevel != ProtectionLevel.EncryptAndSign));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return (this.TransactionProtocol != NetTcpDefaults.TransactionProtocol);
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            NetNamedPipeSecurity security;
            binding = null;
            if (elements.Count > 4)
            {
                return false;
            }
            TransactionFlowBindingElement context = null;
            BinaryMessageEncodingBindingElement encoding = null;
            WindowsStreamSecurityBindingElement wssbe = null;
            NamedPipeTransportBindingElement namedPipe = null;
            foreach (BindingElement element5 in elements)
            {
                if (element5 is TransactionFlowBindingElement)
                {
                    context = element5 as TransactionFlowBindingElement;
                }
                else if (element5 is BinaryMessageEncodingBindingElement)
                {
                    encoding = element5 as BinaryMessageEncodingBindingElement;
                }
                else if (element5 is WindowsStreamSecurityBindingElement)
                {
                    wssbe = element5 as WindowsStreamSecurityBindingElement;
                }
                else if (element5 is NamedPipeTransportBindingElement)
                {
                    namedPipe = element5 as NamedPipeTransportBindingElement;
                }
                else
                {
                    return false;
                }
            }
            if (namedPipe == null)
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
            if (!TryCreateSecurity(wssbe, out security))
            {
                return false;
            }
            NetNamedPipeBinding binding2 = new NetNamedPipeBinding(security);
            binding2.InitializeFrom(namedPipe, encoding, context);
            if (!binding2.IsBindingElementsMatch(namedPipe, encoding, context))
            {
                return false;
            }
            binding = binding2;
            return true;
        }

        private static bool TryCreateSecurity(WindowsStreamSecurityBindingElement wssbe, out NetNamedPipeSecurity security)
        {
            NetNamedPipeSecurityMode mode = (wssbe == null) ? NetNamedPipeSecurityMode.None : NetNamedPipeSecurityMode.Transport;
            return NetNamedPipeSecurity.TryCreate(wssbe, mode, out security);
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
                return this.namedPipe.HostNameComparisonMode;
            }
            set
            {
                this.namedPipe.HostNameComparisonMode = value;
            }
        }

        [DefaultValue((long) 0x80000L)]
        public long MaxBufferPoolSize
        {
            get
            {
                return this.namedPipe.MaxBufferPoolSize;
            }
            set
            {
                this.namedPipe.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(0x10000)]
        public int MaxBufferSize
        {
            get
            {
                return this.namedPipe.MaxBufferSize;
            }
            set
            {
                this.namedPipe.MaxBufferSize = value;
            }
        }

        [DefaultValue(10)]
        public int MaxConnections
        {
            get
            {
                return this.namedPipe.MaxPendingConnections;
            }
            set
            {
                this.namedPipe.MaxPendingConnections = value;
                this.namedPipe.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = value;
            }
        }

        [DefaultValue((long) 0x10000L)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return this.namedPipe.MaxReceivedMessageSize;
            }
            set
            {
                this.namedPipe.MaxReceivedMessageSize = value;
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

        public override string Scheme
        {
            get
            {
                return this.namedPipe.Scheme;
            }
        }

        public NetNamedPipeSecurity Security
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
                return this.namedPipe.TransferMode;
            }
            set
            {
                this.namedPipe.TransferMode = value;
            }
        }
    }
}

