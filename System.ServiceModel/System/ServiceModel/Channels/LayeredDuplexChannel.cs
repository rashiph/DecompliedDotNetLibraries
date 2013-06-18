namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class LayeredDuplexChannel : LayeredInputChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject
    {
        private IOutputChannel innerOutputChannel;
        private EndpointAddress localAddress;
        private EventHandler onInnerOutputChannelFaulted;

        public LayeredDuplexChannel(ChannelManagerBase channelManager, IInputChannel innerInputChannel, EndpointAddress localAddress, IOutputChannel innerOutputChannel) : base(channelManager, innerInputChannel)
        {
            this.localAddress = localAddress;
            this.innerOutputChannel = innerOutputChannel;
            this.onInnerOutputChannelFaulted = new EventHandler(this.OnInnerOutputChannelFaulted);
            this.innerOutputChannel.Faulted += this.onInnerOutputChannelFaulted;
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerOutputChannel.BeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.innerOutputChannel.EndSend(result);
        }

        protected override void OnAbort()
        {
            this.innerOutputChannel.Abort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ICommunicationObject[] { this.innerOutputChannel });
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ICommunicationObject[] { this.innerOutputChannel });
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.innerOutputChannel.Close(helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnClosing()
        {
            this.innerOutputChannel.Faulted -= this.onInnerOutputChannelFaulted;
            base.OnClosing();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        private void OnInnerOutputChannelFaulted(object sender, EventArgs e)
        {
            base.Fault();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.innerOutputChannel.Open(helper.RemainingTime());
        }

        public void Send(Message message)
        {
            this.Send(message, base.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.innerOutputChannel.Send(message, timeout);
        }

        public override EndpointAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.innerOutputChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return this.innerOutputChannel.Via;
            }
        }
    }
}

