namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class PeerMessageDispatcher<ChannelInterfaceType, TChannel> : CommunicationObject where ChannelInterfaceType: class, IChannel where TChannel: InputQueueChannel<Message>
    {
        private ChannelManagerBase channelManager;
        private PeerNodeImplementation peerNode;
        private PeerMessageQueueAdapter<ChannelInterfaceType, TChannel> queueHandler;
        private PeerQuotaHelper quotaHelper;
        private bool registered;
        private System.ServiceModel.Security.SecurityProtocol securityProtocol;
        private EndpointAddress to;
        private Uri via;

        public PeerMessageDispatcher(PeerMessageQueueAdapter<ChannelInterfaceType, TChannel> queueHandler, PeerNodeImplementation peerNode, ChannelManagerBase channelManager, EndpointAddress to, Uri via)
        {
            this.quotaHelper = new PeerQuotaHelper(0x7fffffff);
            PeerNodeImplementation.ValidateVia(via);
            this.queueHandler = queueHandler;
            this.peerNode = peerNode;
            this.to = to;
            this.via = via;
            this.channelManager = channelManager;
            EndpointAddress address = null;
            this.securityProtocol = ((IPeerFactory) channelManager).SecurityManager.CreateSecurityProtocol<ChannelInterfaceType>(to, ServiceDefaults.SendTimeout);
            if (typeof(IDuplexChannel).IsAssignableFrom(typeof(ChannelInterfaceType)))
            {
                address = to;
            }
            PeerMessageFilter[] filters = new PeerMessageFilter[] { new PeerMessageFilter(via, address) };
            peerNode.RegisterMessageFilter(this, this.via, filters, (ITransportFactorySettings) this.channelManager, new PeerNodeImplementation.MessageAvailableCallback(this.OnMessageAvailable), this.securityProtocol);
            this.registered = true;
        }

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnClose(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.Unregister(true);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public void OnMessageAvailable(Message message)
        {
            this.quotaHelper.ReadyToEnqueueItem();
            this.queueHandler.EnqueueAndDispatch(message, new Action(this.quotaHelper.ItemDequeued));
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        internal void Unregister()
        {
            this.Unregister(false);
        }

        internal void Unregister(bool release)
        {
            PeerNodeImplementation peerNode = this.peerNode;
            if (peerNode != null)
            {
                if (this.registered)
                {
                    peerNode.UnregisterMessageFilter(this, this.via);
                    this.registered = false;
                }
                if (release)
                {
                    peerNode.Release();
                }
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.channelManager.InternalCloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return this.channelManager.InternalOpenTimeout;
            }
        }

        public System.ServiceModel.Security.SecurityProtocol SecurityProtocol
        {
            get
            {
                return this.securityProtocol;
            }
        }

        public class PeerMessageQueueAdapter
        {
            private InputQueueChannel<Message> inputQueueChannel;
            private SingletonChannelAcceptor<ChannelInterfaceType, TChannel, Message> singletonChannelAcceptor;

            public PeerMessageQueueAdapter(InputQueueChannel<Message> inputQueueChannel)
            {
                this.inputQueueChannel = inputQueueChannel;
            }

            public PeerMessageQueueAdapter(SingletonChannelAcceptor<ChannelInterfaceType, TChannel, Message> singletonChannelAcceptor)
            {
                this.singletonChannelAcceptor = singletonChannelAcceptor;
            }

            public void EnqueueAndDispatch(Message message, Action callback)
            {
                if (this.singletonChannelAcceptor != null)
                {
                    this.singletonChannelAcceptor.Enqueue(message, callback);
                }
                else if (this.inputQueueChannel != null)
                {
                    this.inputQueueChannel.EnqueueAndDispatch(message, callback);
                }
            }
        }
    }
}

