namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class LayeredInputChannel : LayeredChannel<IInputChannel>, IInputChannel, IChannel, ICommunicationObject
    {
        public LayeredInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel) : base(channelManager, innerChannel)
        {
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
            this.InternalOnReceive(message);
            return message;
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            bool flag = base.InnerChannel.EndTryReceive(result, out message);
            this.InternalOnReceive(message);
            return flag;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return base.InnerChannel.EndWaitForMessage(result);
        }

        private void InternalOnReceive(Message message)
        {
            if (message != null)
            {
                this.OnReceive(message);
            }
        }

        protected virtual void OnReceive(Message message)
        {
        }

        public Message Receive()
        {
            Message message = base.InnerChannel.Receive();
            this.InternalOnReceive(message);
            return message;
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = base.InnerChannel.Receive(timeout);
            this.InternalOnReceive(message);
            return message;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            bool flag = base.InnerChannel.TryReceive(timeout, out message);
            this.InternalOnReceive(message);
            return flag;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return base.InnerChannel.WaitForMessage(timeout);
        }

        public virtual EndpointAddress LocalAddress
        {
            get
            {
                return base.InnerChannel.LocalAddress;
            }
        }
    }
}

