namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class PeerChannelFactory<TChannel> : TransportChannelFactory<TChannel>, IPeerFactory, ITransportFactorySettings, IDefaultCommunicationTimeouts
    {
        private IPAddress listenIPAddress;
        private int port;
        private PeerNodeImplementation privatePeerNode;
        private XmlDictionaryReaderQuotas readerQuotas;
        private PeerResolver resolver;
        private ISecurityCapabilities securityCapabilities;
        private PeerSecurityManager securityManager;

        internal PeerChannelFactory(PeerTransportBindingElement bindingElement, BindingContext context, PeerResolver peerResolver) : base(bindingElement, context)
        {
            this.listenIPAddress = bindingElement.ListenIPAddress;
            this.port = bindingElement.Port;
            this.resolver = peerResolver;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            BinaryMessageEncodingBindingElement element = context.Binding.Elements.Find<BinaryMessageEncodingBindingElement>();
            if (element != null)
            {
                element.ReaderQuotas.CopyTo(this.readerQuotas);
            }
            else
            {
                EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            }
            this.securityManager = PeerSecurityManager.Create(bindingElement.Security, context, this.readerQuotas);
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(PeerChannelFactory<TChannel>))
            {
                return (T) this;
            }
            if (typeof(T) == typeof(IPeerFactory))
            {
                return (T) this;
            }
            if (typeof(T) == typeof(PeerNodeImplementation))
            {
                return (T) this.privatePeerNode;
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) this.securityCapabilities;
            }
            return base.GetProperty<T>();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override TChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);
            PeerNodeImplementation peerNode = null;
            PeerNodeImplementation.Registration registration = null;
            if ((this.privatePeerNode != null) && (via.Host == this.privatePeerNode.MeshId))
            {
                peerNode = this.privatePeerNode;
            }
            else
            {
                registration = new PeerNodeImplementation.Registration(via, this);
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel) new PeerOutputChannel(peerNode, registration, this, to, via, base.MessageVersion);
            }
            PeerDuplexChannel inputQueueChannel = new PeerDuplexChannel(peerNode, registration, this, to, via);
            PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter queueHandler = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter(inputQueueChannel);
            PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> dispatcher = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>(queueHandler, inputQueueChannel.InnerNode, this, to, via);
            inputQueueChannel.Dispatcher = dispatcher;
            return (TChannel) inputQueueChannel;
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        public IPAddress ListenIPAddress
        {
            get
            {
                return this.listenIPAddress;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
        }

        public PeerNodeImplementation PrivatePeerNode
        {
            get
            {
                return this.privatePeerNode;
            }
            set
            {
                this.privatePeerNode = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

        public PeerResolver Resolver
        {
            get
            {
                return this.resolver;
            }
        }

        public override string Scheme
        {
            get
            {
                return "net.p2p";
            }
        }

        public PeerSecurityManager SecurityManager
        {
            get
            {
                return this.securityManager;
            }
            set
            {
                this.securityManager = value;
            }
        }
    }
}

