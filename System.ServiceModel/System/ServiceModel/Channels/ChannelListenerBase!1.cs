namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public abstract class ChannelListenerBase<TChannel> : ChannelListenerBase, IChannelListener<TChannel>, IChannelListener, ICommunicationObject where TChannel: class, IChannel
    {
        protected ChannelListenerBase()
        {
        }

        protected ChannelListenerBase(IDefaultCommunicationTimeouts timeouts) : base(timeouts)
        {
        }

        public TChannel AcceptChannel()
        {
            return this.AcceptChannel(base.InternalReceiveTimeout);
        }

        public TChannel AcceptChannel(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            base.ThrowPending();
            return this.OnAcceptChannel(timeout);
        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(base.InternalReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            base.ThrowPending();
            return this.OnBeginAcceptChannel(timeout, callback, state);
        }

        public TChannel EndAcceptChannel(IAsyncResult result)
        {
            return this.OnEndAcceptChannel(result);
        }

        protected abstract TChannel OnAcceptChannel(TimeSpan timeout);
        protected abstract IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract TChannel OnEndAcceptChannel(IAsyncResult result);
    }
}

