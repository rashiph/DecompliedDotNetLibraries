namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class LayeredChannelAcceptor<TChannel, TInnerChannel> : ChannelAcceptor<TChannel> where TChannel: class, IChannel where TInnerChannel: class, IChannel
    {
        private IChannelListener<TInnerChannel> innerListener;

        protected LayeredChannelAcceptor(ChannelManagerBase channelManager, IChannelListener<TInnerChannel> innerListener) : base(channelManager)
        {
            this.innerListener = innerListener;
        }

        public override TChannel AcceptChannel(TimeSpan timeout)
        {
            TInnerChannel innerChannel = this.innerListener.AcceptChannel(timeout);
            if (innerChannel == null)
            {
                return default(TChannel);
            }
            return this.OnAcceptChannel(innerChannel);
        }

        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerListener.BeginAcceptChannel(timeout, callback, state);
        }

        public override IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerListener.BeginWaitForChannel(timeout, callback, state);
        }

        public override TChannel EndAcceptChannel(IAsyncResult result)
        {
            TInnerChannel innerChannel = this.innerListener.EndAcceptChannel(result);
            if (innerChannel == null)
            {
                return default(TChannel);
            }
            return this.OnAcceptChannel(innerChannel);
        }

        public override bool EndWaitForChannel(IAsyncResult result)
        {
            return this.innerListener.EndWaitForChannel(result);
        }

        protected abstract TChannel OnAcceptChannel(TInnerChannel innerChannel);
        public override bool WaitForChannel(TimeSpan timeout)
        {
            return this.innerListener.WaitForChannel(timeout);
        }
    }
}

