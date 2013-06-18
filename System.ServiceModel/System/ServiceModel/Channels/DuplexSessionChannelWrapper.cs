namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class DuplexSessionChannelWrapper : InputChannelWrapper, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
    {
        public DuplexSessionChannelWrapper(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel, Message firstMessage) : base(channelManager, innerChannel, firstMessage)
        {
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginSend(message, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.InnerChannel.EndSend(result);
        }

        public void Send(Message message)
        {
            this.InnerChannel.Send(message);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.InnerChannel.Send(message, timeout);
        }

        private IDuplexSessionChannel InnerChannel
        {
            get
            {
                return (IDuplexSessionChannel) base.InnerChannel;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.InnerChannel.RemoteAddress;
            }
        }

        public IDuplexSession Session
        {
            get
            {
                return this.InnerChannel.Session;
            }
        }

        public Uri Via
        {
            get
            {
                return this.InnerChannel.Via;
            }
        }
    }
}

