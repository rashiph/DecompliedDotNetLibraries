namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class LayeredChannel<TInnerChannel> : ChannelBase where TInnerChannel: class, IChannel
    {
        private TInnerChannel innerChannel;
        private EventHandler onInnerChannelFaulted;

        protected LayeredChannel(ChannelManagerBase channelManager, TInnerChannel innerChannel) : base(channelManager)
        {
            this.innerChannel = innerChannel;
            this.onInnerChannelFaulted = new EventHandler(this.OnInnerChannelFaulted);
            this.innerChannel.Faulted += this.onInnerChannelFaulted;
        }

        public override T GetProperty<T>() where T: class
        {
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            return this.InnerChannel.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            this.innerChannel.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannel.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannel.BeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.innerChannel.Close(timeout);
        }

        protected override void OnClosing()
        {
            this.innerChannel.Faulted -= this.onInnerChannelFaulted;
            base.OnClosing();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.innerChannel.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannel.EndOpen(result);
        }

        private void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            base.Fault();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannel.Open(timeout);
        }

        protected TInnerChannel InnerChannel
        {
            get
            {
                return this.innerChannel;
            }
        }
    }
}

