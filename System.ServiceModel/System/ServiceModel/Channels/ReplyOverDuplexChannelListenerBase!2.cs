namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class ReplyOverDuplexChannelListenerBase<TOuterChannel, TInnerChannel> : LayeredChannelListener<TOuterChannel> where TOuterChannel: class, IReplyChannel where TInnerChannel: class, IDuplexChannel
    {
        private IChannelListener<TInnerChannel> innerChannelListener;

        public ReplyOverDuplexChannelListenerBase(BindingContext context) : base(context.Binding, context.BuildInnerChannelListener<TInnerChannel>())
        {
        }

        protected abstract TOuterChannel CreateWrappedChannel(ChannelManagerBase channelManager, TInnerChannel innerChannel);
        protected override TOuterChannel OnAcceptChannel(TimeSpan timeout)
        {
            TInnerChannel innerChannel = this.innerChannelListener.AcceptChannel(timeout);
            return this.WrapInnerChannel(innerChannel);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginAcceptChannel(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override TOuterChannel OnEndAcceptChannel(IAsyncResult result)
        {
            TInnerChannel innerChannel = this.innerChannelListener.EndAcceptChannel(result);
            return this.WrapInnerChannel(innerChannel);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.innerChannelListener.EndWaitForChannel(result);
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<TInnerChannel>) this.InnerChannelListener;
            base.OnOpening();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.innerChannelListener.WaitForChannel(timeout);
        }

        private TOuterChannel WrapInnerChannel(TInnerChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return default(TOuterChannel);
            }
            return this.CreateWrappedChannel(this, innerChannel);
        }
    }
}

