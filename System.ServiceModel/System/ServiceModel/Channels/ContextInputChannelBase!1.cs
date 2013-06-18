namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal abstract class ContextInputChannelBase<TChannel> : LayeredChannel<TChannel> where TChannel: class, IInputChannel
    {
        private ContextExchangeMechanism contextExchangeMechanism;
        private ServiceContextProtocol contextProtocol;

        protected ContextInputChannelBase(ChannelManagerBase channelManager, TChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism) : base(channelManager, innerChannel)
        {
            this.contextExchangeMechanism = contextExchangeMechanism;
            this.contextProtocol = new ServiceContextProtocol(contextExchangeMechanism);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginReceive(callback, state);
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
            this.ProcessContextHeader(message);
            return message;
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (base.InnerChannel.EndTryReceive(result, out message))
            {
                this.ProcessContextHeader(message);
                return true;
            }
            return false;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return base.InnerChannel.EndWaitForMessage(result);
        }

        private void ProcessContextHeader(Message message)
        {
            if (message != null)
            {
                this.contextProtocol.OnIncomingMessage(message);
            }
        }

        public Message Receive()
        {
            Message message = base.InnerChannel.Receive();
            this.ProcessContextHeader(message);
            return message;
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = base.InnerChannel.Receive(timeout);
            this.ProcessContextHeader(message);
            return message;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (base.InnerChannel.TryReceive(timeout, out message))
            {
                this.ProcessContextHeader(message);
                return true;
            }
            return false;
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

