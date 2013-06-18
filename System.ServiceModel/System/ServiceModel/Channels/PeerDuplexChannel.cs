namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    internal class PeerDuplexChannel : DuplexChannel
    {
        private ChannelManagerBase channelManager;
        private PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> messageDispatcher;
        private PeerNode peerNode;
        private bool released;
        private SecurityProtocol securityProtocol;
        private EndpointAddress to;
        private Uri via;

        public PeerDuplexChannel(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager, EndpointAddress localAddress, Uri via) : base(channelManager, localAddress)
        {
            PeerNodeImplementation.ValidateVia(via);
            if (registration != null)
            {
                peerNode = PeerNodeImplementation.Get(via, registration);
            }
            this.peerNode = new PeerNode(peerNode);
            this.to = localAddress;
            this.via = via;
            this.channelManager = channelManager;
        }

        protected override void AddHeadersTo(Message message)
        {
            base.AddHeadersTo(message);
            if (this.to != null)
            {
                this.to.ApplyTo(message);
            }
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(PeerNode))
            {
                return (T) this.peerNode;
            }
            if (typeof(T) == typeof(PeerNodeImplementation))
            {
                return (T) this.peerNode.InnerNode;
            }
            if (typeof(T) == typeof(IOnlineStatus))
            {
                return (T) this.peerNode;
            }
            if (typeof(T) == typeof(FaultConverter))
            {
                return (T) FaultConverter.GetDefaultFaultConverter(MessageVersion.Soap12WSAddressing10);
            }
            return base.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (base.State < CommunicationState.Closed)
            {
                try
                {
                    this.peerNode.InnerNode.Abort();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.peerNode.InnerNode.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.peerNode.InnerNode.BeginOpen(timeout, callback, state, true);
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.securityProtocol == null)
            {
                lock (base.ThisLock)
                {
                    if (this.securityProtocol == null)
                    {
                        this.securityProtocol = ((IPeerFactory) this.channelManager).SecurityManager.CreateSecurityProtocol<IDuplexChannel>(this.to, helper.RemainingTime());
                    }
                }
            }
            return this.peerNode.InnerNode.BeginSend(this, message, this.via, (ITransportFactorySettings) base.Manager, helper.RemainingTime(), callback, state, this.securityProtocol);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.peerNode.InnerNode.Close(helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.ReleaseNode();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            PeerNodeImplementation.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            PeerNodeImplementation.EndOpen(result);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            PeerNodeImplementation.EndSend(result);
        }

        protected override void OnEnqueueItem(Message message)
        {
            message.Properties.Via = this.via;
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4003d, System.ServiceModel.SR.GetString("TraceCodePeerChannelMessageReceived"), this, message);
            }
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            this.ReleaseNode();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.peerNode.OnOpen();
            this.peerNode.InnerNode.Open(timeout, true);
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            base.EndSend(base.BeginSend(message, timeout, null, null));
        }

        private void ReleaseNode()
        {
            if (!this.released)
            {
                bool flag = false;
                lock (base.ThisLock)
                {
                    if (!this.released)
                    {
                        flag = this.released = true;
                    }
                }
                if (flag && (this.peerNode != null))
                {
                    if (this.messageDispatcher != null)
                    {
                        this.messageDispatcher.Unregister(false);
                    }
                    this.peerNode.InnerNode.Release();
                }
            }
        }

        internal PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> Dispatcher
        {
            get
            {
                return this.messageDispatcher;
            }
            set
            {
                this.messageDispatcher = value;
            }
        }

        public PeerNodeImplementation InnerNode
        {
            get
            {
                return this.peerNode.InnerNode;
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.to;
            }
        }

        public override Uri Via
        {
            get
            {
                return this.via;
            }
        }
    }
}

