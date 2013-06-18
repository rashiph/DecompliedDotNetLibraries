namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Text;
    using System.Xml;

    public abstract class WSHttpBindingBase : Binding, IBindingRuntimePreferences
    {
        private HttpsTransportBindingElement httpsTransport;
        private HttpTransportBindingElement httpTransport;
        private WSMessageEncoding messageEncoding;
        private MtomMessageEncodingBindingElement mtomEncoding;
        private OptionalReliableSession reliableSession;
        private System.ServiceModel.Channels.ReliableSessionBindingElement session;
        private TextMessageEncodingBindingElement textEncoding;
        private System.ServiceModel.Channels.TransactionFlowBindingElement txFlow;

        protected WSHttpBindingBase()
        {
            this.Initialize();
        }

        protected WSHttpBindingBase(bool reliableSessionEnabled) : this()
        {
            this.ReliableSession.Enabled = reliableSessionEnabled;
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection {
                this.txFlow
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
            WSMessageEncodingHelper.SyncUpEncodingBindingElementProperties(this.textEncoding, this.mtomEncoding);
            if (this.MessageEncoding == WSMessageEncoding.Text)
            {
                elements.Add(this.textEncoding);
            }
            else if (this.MessageEncoding == WSMessageEncoding.Mtom)
            {
                elements.Add(this.mtomEncoding);
            }
            elements.Add(this.GetTransport());
            return elements.Clone();
        }

        protected abstract SecurityBindingElement CreateMessageSecurity();
        private static System.ServiceModel.Channels.TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            return new System.ServiceModel.Channels.TransactionFlowBindingElement(false) { TransactionProtocol = TransactionProtocol.WSAtomicTransactionOctober2004 };
        }

        protected abstract TransportBindingElement GetTransport();
        private void Initialize()
        {
            this.httpTransport = new HttpTransportBindingElement();
            this.httpsTransport = new HttpsTransportBindingElement();
            this.messageEncoding = WSMessageEncoding.Text;
            this.txFlow = GetDefaultTransactionFlowBindingElement();
            this.session = new System.ServiceModel.Channels.ReliableSessionBindingElement(true);
            this.textEncoding = new TextMessageEncodingBindingElement();
            this.textEncoding.MessageVersion = MessageVersion.Soap12WSAddressing10;
            this.mtomEncoding = new MtomMessageEncodingBindingElement();
            this.mtomEncoding.MessageVersion = MessageVersion.Soap12WSAddressing10;
            this.reliableSession = new OptionalReliableSession(this.session);
        }

        private void InitializeFrom(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding, System.ServiceModel.Channels.TransactionFlowBindingElement txFlow, System.ServiceModel.Channels.ReliableSessionBindingElement session)
        {
            this.BypassProxyOnLocal = transport.BypassProxyOnLocal;
            this.HostNameComparisonMode = transport.HostNameComparisonMode;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.ProxyAddress = transport.ProxyAddress;
            this.UseDefaultWebProxy = transport.UseDefaultWebProxy;
            if (encoding is TextMessageEncodingBindingElement)
            {
                this.MessageEncoding = WSMessageEncoding.Text;
                TextMessageEncodingBindingElement element = (TextMessageEncodingBindingElement) encoding;
                this.TextEncoding = element.WriteEncoding;
                this.ReaderQuotas = element.ReaderQuotas;
            }
            else if (encoding is MtomMessageEncodingBindingElement)
            {
                this.messageEncoding = WSMessageEncoding.Mtom;
                MtomMessageEncodingBindingElement element2 = (MtomMessageEncodingBindingElement) encoding;
                this.TextEncoding = element2.WriteEncoding;
                this.ReaderQuotas = element2.ReaderQuotas;
            }
            this.TransactionFlow = txFlow.Transactions;
            this.reliableSession.Enabled = session != null;
            if (session != null)
            {
                this.session.InactivityTimeout = session.InactivityTimeout;
                this.session.Ordered = session.Ordered;
            }
        }

        private bool IsBindingElementsMatch(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding, System.ServiceModel.Channels.TransactionFlowBindingElement txFlow, System.ServiceModel.Channels.ReliableSessionBindingElement session)
        {
            if (!this.GetTransport().IsMatch(transport))
            {
                return false;
            }
            if (this.MessageEncoding == WSMessageEncoding.Text)
            {
                if (!this.textEncoding.IsMatch(encoding))
                {
                    return false;
                }
            }
            else if ((this.MessageEncoding == WSMessageEncoding.Mtom) && !this.mtomEncoding.IsMatch(encoding))
            {
                return false;
            }
            if (!this.txFlow.IsMatch(txFlow))
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
        public bool ShouldSerializeTextEncoding()
        {
            return !this.TextEncoding.Equals(TextEncoderDefaults.Encoding);
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 6)
            {
                return false;
            }
            PrivacyNoticeBindingElement privacy = null;
            System.ServiceModel.Channels.TransactionFlowBindingElement tfbe = null;
            System.ServiceModel.Channels.ReliableSessionBindingElement rsbe = null;
            SecurityBindingElement sbe = null;
            MessageEncodingBindingElement encoding = null;
            HttpTransportBindingElement transport = null;
            foreach (BindingElement element7 in elements)
            {
                if (element7 is SecurityBindingElement)
                {
                    sbe = element7 as SecurityBindingElement;
                }
                else if (element7 is TransportBindingElement)
                {
                    transport = element7 as HttpTransportBindingElement;
                }
                else if (element7 is MessageEncodingBindingElement)
                {
                    encoding = element7 as MessageEncodingBindingElement;
                }
                else if (element7 is System.ServiceModel.Channels.TransactionFlowBindingElement)
                {
                    tfbe = element7 as System.ServiceModel.Channels.TransactionFlowBindingElement;
                }
                else if (element7 is System.ServiceModel.Channels.ReliableSessionBindingElement)
                {
                    rsbe = element7 as System.ServiceModel.Channels.ReliableSessionBindingElement;
                }
                else if (element7 is PrivacyNoticeBindingElement)
                {
                    privacy = element7 as PrivacyNoticeBindingElement;
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
            if (((privacy != null) || !WSHttpBinding.TryCreate(sbe, transport, rsbe, tfbe, out binding)) && ((!WSFederationHttpBinding.TryCreate(sbe, transport, privacy, rsbe, tfbe, out binding) && !WS2007HttpBinding.TryCreate(sbe, transport, rsbe, tfbe, out binding)) && !WS2007FederationHttpBinding.TryCreate(sbe, transport, privacy, rsbe, tfbe, out binding)))
            {
                return false;
            }
            if (tfbe == null)
            {
                tfbe = GetDefaultTransactionFlowBindingElement();
                if ((binding is WS2007HttpBinding) || (binding is WS2007FederationHttpBinding))
                {
                    tfbe.TransactionProtocol = TransactionProtocol.WSAtomicTransaction11;
                }
            }
            WSHttpBindingBase base2 = binding as WSHttpBindingBase;
            base2.InitializeFrom(transport, encoding, tfbe, rsbe);
            if (!base2.IsBindingElementsMatch(transport, encoding, tfbe, rsbe))
            {
                return false;
            }
            return true;
        }

        [DefaultValue(false)]
        public bool BypassProxyOnLocal
        {
            get
            {
                return this.httpTransport.BypassProxyOnLocal;
            }
            set
            {
                this.httpTransport.BypassProxyOnLocal = value;
                this.httpsTransport.BypassProxyOnLocal = value;
            }
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
                return this.httpTransport.HostNameComparisonMode;
            }
            set
            {
                this.httpTransport.HostNameComparisonMode = value;
                this.httpsTransport.HostNameComparisonMode = value;
            }
        }

        internal HttpsTransportBindingElement HttpsTransport
        {
            get
            {
                return this.httpsTransport;
            }
        }

        internal HttpTransportBindingElement HttpTransport
        {
            get
            {
                return this.httpTransport;
            }
        }

        [DefaultValue((long) 0x80000L)]
        public long MaxBufferPoolSize
        {
            get
            {
                return this.httpTransport.MaxBufferPoolSize;
            }
            set
            {
                this.httpTransport.MaxBufferPoolSize = value;
                this.httpsTransport.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue((long) 0x10000L)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return this.httpTransport.MaxReceivedMessageSize;
            }
            set
            {
                if (value > 0x7fffffffL)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value.MaxReceivedMessageSize", System.ServiceModel.SR.GetString("MaxReceivedMessageSizeMustBeInIntegerRange")));
                }
                this.httpTransport.MaxReceivedMessageSize = value;
                this.httpsTransport.MaxReceivedMessageSize = value;
                this.mtomEncoding.MaxBufferSize = (int) value;
            }
        }

        [DefaultValue(0)]
        public WSMessageEncoding MessageEncoding
        {
            get
            {
                return this.messageEncoding;
            }
            set
            {
                this.messageEncoding = value;
            }
        }

        [TypeConverter(typeof(UriTypeConverter)), DefaultValue(null)]
        public Uri ProxyAddress
        {
            get
            {
                return this.httpTransport.ProxyAddress;
            }
            set
            {
                this.httpTransport.ProxyAddress = value;
                this.httpsTransport.ProxyAddress = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.textEncoding.ReaderQuotas;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                value.CopyTo(this.textEncoding.ReaderQuotas);
                value.CopyTo(this.mtomEncoding.ReaderQuotas);
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

        internal System.ServiceModel.Channels.ReliableSessionBindingElement ReliableSessionBindingElement
        {
            get
            {
                return this.session;
            }
        }

        public override string Scheme
        {
            get
            {
                return this.GetTransport().Scheme;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get
            {
                return false;
            }
        }

        [TypeConverter(typeof(EncodingConverter))]
        public Encoding TextEncoding
        {
            get
            {
                return this.textEncoding.WriteEncoding;
            }
            set
            {
                this.textEncoding.WriteEncoding = value;
                this.mtomEncoding.WriteEncoding = value;
            }
        }

        [DefaultValue(false)]
        public bool TransactionFlow
        {
            get
            {
                return this.txFlow.Transactions;
            }
            set
            {
                this.txFlow.Transactions = value;
            }
        }

        internal System.ServiceModel.Channels.TransactionFlowBindingElement TransactionFlowBindingElement
        {
            get
            {
                return this.txFlow;
            }
        }

        [DefaultValue(true)]
        public bool UseDefaultWebProxy
        {
            get
            {
                return this.httpTransport.UseDefaultWebProxy;
            }
            set
            {
                this.httpTransport.UseDefaultWebProxy = value;
                this.httpsTransport.UseDefaultWebProxy = value;
            }
        }
    }
}

