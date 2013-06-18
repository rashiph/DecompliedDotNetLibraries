namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class DelegatingChannelListener<TChannel> : LayeredChannelListener<TChannel> where TChannel: class, IChannel
    {
        private IChannelAcceptor<TChannel> channelAcceptor;

        protected DelegatingChannelListener(bool sharedInnerListener) : base(sharedInnerListener)
        {
        }

        protected DelegatingChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts) : base(sharedInnerListener, timeouts)
        {
        }

        protected DelegatingChannelListener(IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener) : base(timeouts, innerChannelListener)
        {
        }

        protected DelegatingChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener) : base(sharedInnerListener, timeouts, innerChannelListener)
        {
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (this.channelAcceptor != null)
            {
                this.channelAcceptor.Abort();
            }
        }

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            return this.channelAcceptor.AcceptChannel(timeout);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelAcceptor.BeginAcceptChannel(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ICommunicationObject[] { this.channelAcceptor });
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ICommunicationObject[] { this.channelAcceptor });
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelAcceptor.BeginWaitForChannel(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnClose(helper.RemainingTime());
            this.channelAcceptor.Close(helper.RemainingTime());
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return this.channelAcceptor.EndAcceptChannel(result);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.channelAcceptor.EndWaitForChannel(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.channelAcceptor.Open(helper.RemainingTime());
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.channelAcceptor.WaitForChannel(timeout);
        }

        public IChannelAcceptor<TChannel> Acceptor
        {
            get
            {
                return this.channelAcceptor;
            }
            set
            {
                this.channelAcceptor = value;
            }
        }
    }
}

