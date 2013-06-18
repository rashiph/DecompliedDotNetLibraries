namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class TransactionRequestChannelGeneric<TChannel> : TransactionChannel<TChannel>, IRequestChannel, IChannel, ICommunicationObject where TChannel: class, IRequestChannel
    {
        public TransactionRequestChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel) : base(channelManager, innerChannel)
        {
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback asyncCallback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.WriteTransactionDataToMessage(message, MessageDirection.Input);
            return base.InnerChannel.BeginRequest(message, helper.RemainingTime(), asyncCallback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            Message message = base.InnerChannel.EndRequest(result);
            if (message != null)
            {
                base.ReadIssuedTokens(message, MessageDirection.Output);
            }
            return message;
        }

        public Message Request(Message message)
        {
            return this.Request(message, base.DefaultSendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.WriteTransactionDataToMessage(message, MessageDirection.Input);
            Message message2 = base.InnerChannel.Request(message, helper.RemainingTime());
            if (message2 != null)
            {
                base.ReadIssuedTokens(message2, MessageDirection.Output);
            }
            return message2;
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return base.InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return base.InnerChannel.Via;
            }
        }
    }
}

