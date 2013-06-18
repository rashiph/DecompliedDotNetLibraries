namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class PeerInputChannel : InputChannel
    {
        private PeerNode peerNode;
        private bool released;
        private EndpointAddress to;
        private Uri via;

        public PeerInputChannel(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager, EndpointAddress localAddress, Uri via) : base(channelManager, localAddress)
        {
            PeerNodeImplementation.ValidateVia(via);
            if (registration != null)
            {
                peerNode = PeerNodeImplementation.Get(via, registration);
            }
            this.peerNode = new PeerNode(peerNode);
            this.to = localAddress;
            this.via = via;
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
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginCloseNode), new ChainedEndHandler(this.OnEndCloseNode), new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose));
        }

        private IAsyncResult OnBeginCloseNode(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.peerNode.InnerNode.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ChainedBeginHandler(this.OnBeginOpenNode), new ChainedEndHandler(this.OnEndOpenNode));
        }

        private IAsyncResult OnBeginOpenNode(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.peerNode.InnerNode.BeginOpen(timeout, callback, state, true);
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
            ChainedAsyncResult.End(result);
        }

        private void OnEndCloseNode(IAsyncResult result)
        {
            PeerNodeImplementation.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        private void OnEndOpenNode(IAsyncResult result)
        {
            PeerNodeImplementation.EndOpen(result);
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
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.peerNode.OnOpen();
            this.peerNode.InnerNode.Open(helper.RemainingTime(), true);
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
                if (flag)
                {
                    this.peerNode.InnerNode.Release();
                }
            }
        }
    }
}

