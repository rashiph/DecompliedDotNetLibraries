namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class DuplexOneWayChannelListener : LayeredChannelListener<IInputChannel>
    {
        private IChannelListener<IDuplexChannel> innerChannelListener;
        private bool packetRoutable;

        public DuplexOneWayChannelListener(OneWayBindingElement bindingElement, BindingContext context) : base(context.Binding, context.BuildInnerChannelListener<IDuplexChannel>())
        {
            this.packetRoutable = bindingElement.PacketRoutable;
        }

        protected override IInputChannel OnAcceptChannel(TimeSpan timeout)
        {
            IDuplexChannel innerChannel = this.innerChannelListener.AcceptChannel(timeout);
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

        protected override IInputChannel OnEndAcceptChannel(IAsyncResult result)
        {
            IDuplexChannel innerChannel = this.innerChannelListener.EndAcceptChannel(result);
            return this.WrapInnerChannel(innerChannel);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.innerChannelListener.EndWaitForChannel(result);
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<IDuplexChannel>) this.InnerChannelListener;
            base.OnOpening();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.innerChannelListener.WaitForChannel(timeout);
        }

        private IInputChannel WrapInnerChannel(IDuplexChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return null;
            }
            return new DuplexOneWayInputChannel(this, innerChannel);
        }

        private class DuplexOneWayInputChannel : LayeredChannel<IDuplexChannel>, IInputChannel, IChannel, ICommunicationObject
        {
            private bool validateHeader;

            public DuplexOneWayInputChannel(DuplexOneWayChannelListener listener, IDuplexChannel innerChannel) : base(listener, innerChannel)
            {
                this.validateHeader = listener.packetRoutable;
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.InnerChannel.BeginReceive(timeout, callback, state);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.InnerChannel.BeginTryReceive(timeout, callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.InnerChannel.BeginWaitForMessage(timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                Message message = base.InnerChannel.EndReceive(result);
                return this.ValidateMessage(message);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                bool flag = base.InnerChannel.EndTryReceive(result, out message);
                message = this.ValidateMessage(message);
                return flag;
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return base.InnerChannel.EndWaitForMessage(result);
            }

            public Message Receive()
            {
                return this.Receive(base.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                Message message = base.InnerChannel.Receive(timeout);
                return this.ValidateMessage(message);
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                bool flag = base.InnerChannel.TryReceive(timeout, out message);
                message = this.ValidateMessage(message);
                return flag;
            }

            private Message ValidateMessage(Message message)
            {
                if (this.validateHeader && (message != null))
                {
                    PacketRoutableHeader.ValidateMessage(message);
                }
                return message;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return base.InnerChannel.WaitForMessage(timeout);
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return base.InnerChannel.LocalAddress;
                }
            }
        }
    }
}

