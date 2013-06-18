namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class PeerOutputChannel : TransportOutputChannel
    {
        private ChannelManagerBase channelManager;
        private PeerNode peerNode;
        private bool released;
        private SecurityProtocol securityProtocol;
        private EndpointAddress to;
        private Uri via;

        public PeerOutputChannel(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager, EndpointAddress localAddress, Uri via, MessageVersion messageVersion) : base(channelManager, localAddress, via, false, messageVersion)
        {
            PeerNodeImplementation.ValidateVia(via);
            if (registration != null)
            {
                peerNode = PeerNodeImplementation.Get(via, registration);
            }
            this.peerNode = new PeerNode(peerNode);
            this.via = via;
            this.channelManager = channelManager;
            this.to = localAddress;
        }

        protected override void AddHeadersTo(Message message)
        {
            this.RemoteAddress.ApplyTo(message);
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
                        this.securityProtocol = ((IPeerFactory) this.channelManager).SecurityManager.CreateSecurityProtocol<IOutputChannel>(this.to, helper.RemainingTime());
                    }
                }
            }
            return this.peerNode.InnerNode.BeginSend(this, message, this.via, (ITransportFactorySettings) base.Manager, helper.RemainingTime(), callback, state, this.securityProtocol);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.peerNode.InnerNode.Close(timeout);
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
                    this.peerNode.InnerNode.Release();
                }
            }
        }
    }
}

