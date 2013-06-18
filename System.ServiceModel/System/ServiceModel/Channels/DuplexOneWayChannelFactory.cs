namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class DuplexOneWayChannelFactory : LayeredChannelFactory<IOutputChannel>
    {
        private IChannelFactory<IDuplexChannel> innnerFactory;
        private bool packetRoutable;

        public DuplexOneWayChannelFactory(OneWayBindingElement bindingElement, BindingContext context) : base(context.Binding, context.BuildInnerChannelFactory<IDuplexChannel>())
        {
            this.innnerFactory = (IChannelFactory<IDuplexChannel>) base.InnerChannelFactory;
            this.packetRoutable = bindingElement.PacketRoutable;
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new DuplexOutputChannel(this, this.innnerFactory.CreateChannel(address, via));
        }

        private class DuplexOutputChannel : OutputChannel
        {
            private IDuplexChannel innerChannel;
            private bool packetRoutable;

            public DuplexOutputChannel(DuplexOneWayChannelFactory factory, IDuplexChannel innerChannel) : base(factory)
            {
                this.packetRoutable = factory.packetRoutable;
                this.innerChannel = innerChannel;
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

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.StampMessage(message);
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.innerChannel.Close(timeout);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                this.innerChannel.EndClose(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.innerChannel.EndOpen(result);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerChannel.Open(timeout);
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                this.StampMessage(message);
                this.innerChannel.Send(message, timeout);
            }

            private void StampMessage(Message message)
            {
                if (this.packetRoutable)
                {
                    PacketRoutableHeader.AddHeadersTo(message, null);
                }
            }

            public override EndpointAddress RemoteAddress
            {
                get
                {
                    return this.innerChannel.RemoteAddress;
                }
            }

            public override Uri Via
            {
                get
                {
                    return this.innerChannel.Via;
                }
            }
        }
    }
}

