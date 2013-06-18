namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class TcpChannelListener<TChannel, TChannelAcceptor> : TcpChannelListener, IChannelListener<TChannel>, IChannelListener, ICommunicationObject where TChannel: class, IChannel where TChannelAcceptor: ChannelAcceptor<TChannel>
    {
        protected TcpChannelListener(TcpTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
        }

        public TChannel AcceptChannel()
        {
            return this.AcceptChannel(this.DefaultReceiveTimeout);
        }

        public TChannel AcceptChannel(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            return this.ChannelAcceptor.AcceptChannel(timeout);
        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            return this.ChannelAcceptor.BeginAcceptChannel(timeout, callback, state);
        }

        public TChannel EndAcceptChannel(IAsyncResult result)
        {
            base.ThrowPending();
            return this.ChannelAcceptor.EndAcceptChannel(result);
        }

        protected override void OnAbort()
        {
            this.ChannelAcceptor.Abort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ICommunicationObject[] { this.ChannelAcceptor });
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ICommunicationObject[] { this.ChannelAcceptor });
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.ChannelAcceptor.BeginWaitForChannel(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.ChannelAcceptor.Close(helper.RemainingTime());
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

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.ChannelAcceptor.EndWaitForChannel(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.ChannelAcceptor.Open(helper.RemainingTime());
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.ChannelAcceptor.WaitForChannel(timeout);
        }

        protected abstract TChannelAcceptor ChannelAcceptor { get; }
    }
}

