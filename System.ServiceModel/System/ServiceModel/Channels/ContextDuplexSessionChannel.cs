namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class ContextDuplexSessionChannel : ContextOutputChannelBase<IDuplexSessionChannel>, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
    {
        private System.ServiceModel.Channels.ContextProtocol contextProtocol;

        public ContextDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism) : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ServiceContextProtocol(contextExchangeMechanism);
        }

        public ContextDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism, Uri address, Uri callbackAddress, bool contextManagementEnabled) : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ClientContextProtocol(contextExchangeMechanism, address, this, callbackAddress, contextManagementEnabled);
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
            if (message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return message;
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            bool flag = base.InnerChannel.EndTryReceive(result, out message);
            if (flag && (message != null))
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return flag;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return base.InnerChannel.EndWaitForMessage(result);
        }

        public Message Receive()
        {
            Message message = base.InnerChannel.Receive();
            if (message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return message;
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = base.InnerChannel.Receive(timeout);
            if (message != null)
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return message;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            bool flag = base.InnerChannel.TryReceive(timeout, out message);
            if (flag && (message != null))
            {
                this.ContextProtocol.OnIncomingMessage(message);
            }
            return flag;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return base.InnerChannel.WaitForMessage(timeout);
        }

        protected override System.ServiceModel.Channels.ContextProtocol ContextProtocol
        {
            get
            {
                return this.contextProtocol;
            }
        }

        protected override bool IsClient
        {
            get
            {
                return (this.ContextProtocol is ClientContextProtocol);
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return base.InnerChannel.LocalAddress;
            }
        }

        public IDuplexSession Session
        {
            get
            {
                return base.InnerChannel.Session;
            }
        }
    }
}

