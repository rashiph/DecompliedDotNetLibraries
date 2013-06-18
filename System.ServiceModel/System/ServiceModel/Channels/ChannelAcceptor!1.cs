namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class ChannelAcceptor<TChannel> : CommunicationObject, IChannelAcceptor<TChannel>, ICommunicationObject where TChannel: class, IChannel
    {
        private ChannelManagerBase channelManager;

        protected ChannelAcceptor(ChannelManagerBase channelManager)
        {
            this.channelManager = channelManager;
        }

        public abstract TChannel AcceptChannel(TimeSpan timeout);
        public abstract IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        public abstract IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);
        public abstract TChannel EndAcceptChannel(IAsyncResult result);
        public abstract bool EndWaitForChannel(IAsyncResult result);
        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        public abstract bool WaitForChannel(TimeSpan timeout);

        protected ChannelManagerBase ChannelManager
        {
            get
            {
                return this.channelManager;
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
    }
}

