namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    [ServiceBehavior(ConcurrencyMode=ConcurrencyMode.Multiple, InstanceContextMode=InstanceContextMode.Single, UseSynchronizationContext=false)]
    internal class PeerService : IPeerService, IPeerServiceContract, IServiceBehavior, IChannelInitializer
    {
        private System.ServiceModel.Channels.Binding binding;
        private PeerNodeConfig config;
        private IPeerConnectorContract connector;
        private IPeerFlooderContract<Message, UtilityInfo> flooder;
        private GetNeighborCallback getNeighborCallback;
        private IPeerNodeMessageHandling messageHandler;
        private ChannelCallback newChannelCallback;
        private ServiceHost serviceHost;

        public PeerService(PeerNodeConfig config, ChannelCallback channelCallback, GetNeighborCallback getNeighborCallback, Dictionary<System.Type, object> services) : this(config, channelCallback, getNeighborCallback, services, null)
        {
        }

        public PeerService(PeerNodeConfig config, ChannelCallback channelCallback, GetNeighborCallback getNeighborCallback, Dictionary<System.Type, object> services, IPeerNodeMessageHandling messageHandler)
        {
            this.config = config;
            this.newChannelCallback = channelCallback;
            this.getNeighborCallback = getNeighborCallback;
            this.messageHandler = messageHandler;
            if (services != null)
            {
                object obj2 = null;
                services.TryGetValue(typeof(IPeerConnectorContract), out obj2);
                this.connector = obj2 as IPeerConnectorContract;
                obj2 = null;
                services.TryGetValue(typeof(IPeerFlooderContract<Message, UtilityInfo>), out obj2);
                this.flooder = obj2 as IPeerFlooderContract<Message, UtilityInfo>;
            }
            this.serviceHost = new ServiceHost(this, new Uri[0]);
            ServiceThrottlingBehavior item = new ServiceThrottlingBehavior {
                MaxConcurrentCalls = this.config.MaxPendingIncomingCalls,
                MaxConcurrentSessions = this.config.MaxConcurrentSessions
            };
            this.serviceHost.Description.Behaviors.Add(item);
        }

        public void Abort()
        {
            this.serviceHost.Abort();
        }

        private void CreateBinding()
        {
            Collection<BindingElement> bindingElementsInTopDownChannelStackOrder = new Collection<BindingElement>();
            BindingElement securityBindingElement = this.config.SecurityManager.GetSecurityBindingElement();
            if (securityBindingElement != null)
            {
                bindingElementsInTopDownChannelStackOrder.Add(securityBindingElement);
            }
            TcpTransportBindingElement item = new TcpTransportBindingElement {
                MaxReceivedMessageSize = this.config.MaxReceivedMessageSize,
                MaxBufferPoolSize = this.config.MaxBufferPoolSize,
                TeredoEnabled = true
            };
            MessageEncodingBindingElement encodingBindingElement = null;
            if (this.messageHandler != null)
            {
                encodingBindingElement = this.messageHandler.EncodingBindingElement;
            }
            if (encodingBindingElement == null)
            {
                BinaryMessageEncodingBindingElement element4 = new BinaryMessageEncodingBindingElement();
                this.config.ReaderQuotas.CopyTo(element4.ReaderQuotas);
                bindingElementsInTopDownChannelStackOrder.Add(element4);
            }
            else
            {
                bindingElementsInTopDownChannelStackOrder.Add(encodingBindingElement);
            }
            bindingElementsInTopDownChannelStackOrder.Add(item);
            this.binding = new CustomBinding(bindingElementsInTopDownChannelStackOrder);
            this.binding.ReceiveTimeout = TimeSpan.MaxValue;
        }

        public EndpointAddress GetListenAddress()
        {
            IChannelListener listener = this.serviceHost.ChannelDispatchers[0].Listener;
            return new EndpointAddress(listener.Uri, listener.GetProperty<EndpointIdentity>(), new AddressHeader[0]);
        }

        private IPeerNeighbor GetNeighbor()
        {
            IPeerNeighbor neighbor = this.getNeighborCallback(OperationContext.Current.GetCallbackChannel<IPeerProxy>());
            if ((neighbor == null) || (neighbor.State == PeerNeighborState.Closed))
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x40036, System.ServiceModel.SR.GetString("TraceCodePeerNeighborNotFound"), new PeerNodeTraceRecord(this.config.NodeId), OperationContext.Current.IncomingMessage);
                }
                return null;
            }
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                PeerNeighborState state = neighbor.State;
                PeerNodeAddress listenAddress = null;
                IPAddress connectIPAddress = null;
                if ((state >= PeerNeighborState.Opened) && (state <= PeerNeighborState.Connected))
                {
                    listenAddress = this.config.GetListenAddress(true);
                    connectIPAddress = this.config.ListenIPAddress;
                }
                PeerNeighborTraceRecord extendedData = new PeerNeighborTraceRecord(neighbor.NodeId, this.config.NodeId, listenAddress, connectIPAddress, neighbor.GetHashCode(), neighbor.IsInitiator, state.ToString(), null, null, OperationContext.Current.IncomingMessage.Headers.Action);
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x4003a, System.ServiceModel.SR.GetString("TraceCodePeerNeighborMessageReceived"), extendedData, this, null);
            }
            return neighbor;
        }

        public void Open(TimeSpan timeout)
        {
            this.CreateBinding();
            this.serviceHost.Description.Endpoints.Clear();
            ServiceEndpoint endpoint = this.serviceHost.AddServiceEndpoint(typeof(IPeerService), this.binding, this.config.GetMeshUri());
            endpoint.ListenUri = this.config.GetSelfUri();
            endpoint.ListenUriMode = (this.config.Port > 0) ? ListenUriMode.Explicit : ListenUriMode.Unique;
            this.config.SecurityManager.ApplyServiceSecurity(this.serviceHost.Description);
            this.serviceHost.Open(timeout);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40050, System.ServiceModel.SR.GetString("TraceCodePeerServiceOpened", new object[] { this.GetListenAddress() }), this);
            }
        }

        IAsyncResult IPeerServiceContract.BeginFloodMessage(Message floodedInfo, AsyncCallback callback, object state)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor != null)
            {
                return this.flooder.OnFloodedMessage(neighbor, floodedInfo, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        void IPeerServiceContract.Connect(ConnectInfo connectInfo)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor != null)
            {
                this.connector.Connect(neighbor, connectInfo);
            }
        }

        void IPeerServiceContract.Disconnect(DisconnectInfo disconnectInfo)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor != null)
            {
                this.connector.Disconnect(neighbor, disconnectInfo);
            }
        }

        void IPeerServiceContract.EndFloodMessage(IAsyncResult result)
        {
            this.flooder.EndFloodMessage(result);
        }

        void IPeerServiceContract.Fault(Message message)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(typeof(IPeerNeighbor).ToString()));
            }
            neighbor.Abort(PeerCloseReason.Faulted, PeerCloseInitiator.RemoteNode);
        }

        void IPeerServiceContract.LinkUtility(UtilityInfo utilityInfo)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor != null)
            {
                this.flooder.ProcessLinkUtility(neighbor, utilityInfo);
            }
        }

        void IPeerServiceContract.Ping(Message message)
        {
        }

        Message IPeerServiceContract.ProcessRequestSecurityToken(Message message)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(typeof(IPeerNeighbor).ToString()));
            }
            Message message2 = this.config.SecurityManager.ProcessRequest(neighbor, message);
            if (message2 == null)
            {
                OperationContext current = OperationContext.Current;
                current.RequestContext.Close();
                current.RequestContext = null;
            }
            return message2;
        }

        void IPeerServiceContract.Refuse(RefuseInfo refuseInfo)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor != null)
            {
                this.connector.Refuse(neighbor, refuseInfo);
            }
        }

        void IPeerServiceContract.Welcome(WelcomeInfo welcomeInfo)
        {
            IPeerNeighbor neighbor = this.GetNeighbor();
            if (neighbor != null)
            {
                this.connector.Welcome(neighbor, welcomeInfo);
            }
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHost, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHost)
        {
            for (int i = 0; i < serviceHost.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher dispatcher = serviceHost.ChannelDispatchers[i] as ChannelDispatcher;
                if (dispatcher != null)
                {
                    bool flag = false;
                    foreach (EndpointDispatcher dispatcher2 in dispatcher.Endpoints)
                    {
                        if (!dispatcher2.IsSystemEndpoint)
                        {
                            if (!flag)
                            {
                                dispatcher.ChannelInitializers.Add(this);
                                flag = true;
                            }
                            dispatcher2.DispatchRuntime.OperationSelector = new OperationSelector(this.messageHandler);
                        }
                    }
                }
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHost)
        {
        }

        void IChannelInitializer.Initialize(IClientChannel channel)
        {
            this.newChannelCallback(channel);
        }

        public System.ServiceModel.Channels.Binding Binding
        {
            get
            {
                return this.binding;
            }
        }

        public delegate bool ChannelCallback(IClientChannel channel);

        public delegate IPeerNeighbor GetNeighborCallback(IPeerProxy channel);
    }
}

