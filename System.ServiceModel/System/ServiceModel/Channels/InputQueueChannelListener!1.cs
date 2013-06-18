namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal class InputQueueChannelListener<TChannel> : DelegatingChannelListener<TChannel> where TChannel: class, IChannel
    {
        private IChannelDemuxer channelDemuxer;
        private ChannelDemuxerFilter filter;

        public InputQueueChannelListener(ChannelDemuxerFilter filter, IChannelDemuxer channelDemuxer) : base(true)
        {
            this.filter = filter;
            this.channelDemuxer = channelDemuxer;
            base.Acceptor = new InputQueueChannelAcceptor<TChannel>(this);
        }

        protected override void OnAbort()
        {
            this.channelDemuxer.OnOuterListenerAbort(this.filter);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOuterListenerClose), new ChainedEndHandler(this.OnEndOuterListenerClose), new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose));
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOuterListenerOpen), new ChainedEndHandler(this.OnEndOuterListenerOpen), new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen));
        }

        private IAsyncResult OnBeginOuterListenerClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelDemuxer.OnBeginOuterListenerClose(this.filter, timeout, callback, state);
        }

        private IAsyncResult OnBeginOuterListenerOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelDemuxer.OnBeginOuterListenerOpen(this.filter, this, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.channelDemuxer.OnOuterListenerClose(this.filter, helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        private void OnEndOuterListenerClose(IAsyncResult result)
        {
            this.channelDemuxer.OnEndOuterListenerClose(result);
        }

        private void OnEndOuterListenerOpen(IAsyncResult result)
        {
            this.channelDemuxer.OnEndOuterListenerOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.channelDemuxer.OnOuterListenerOpen(this.filter, this, helper.RemainingTime());
            base.OnOpen(helper.RemainingTime());
        }

        public ChannelDemuxerFilter Filter
        {
            get
            {
                return this.filter;
            }
        }

        public InputQueueChannelAcceptor<TChannel> InputQueueAcceptor
        {
            get
            {
                return (InputQueueChannelAcceptor<TChannel>) base.Acceptor;
            }
        }
    }
}

