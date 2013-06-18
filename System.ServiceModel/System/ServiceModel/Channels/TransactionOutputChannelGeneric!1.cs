namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class TransactionOutputChannelGeneric<TChannel> : TransactionChannel<TChannel>, IOutputChannel, IChannel, ICommunicationObject where TChannel: class, IOutputChannel
    {
        public TransactionOutputChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel) : base(channelManager, innerChannel)
        {
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback asyncCallback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.WriteTransactionDataToMessage(message, MessageDirection.Input);
            return base.InnerChannel.BeginSend(message, helper.RemainingTime(), asyncCallback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            base.InnerChannel.EndSend(result);
        }

        public void Send(Message message)
        {
            this.Send(message, base.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.WriteTransactionDataToMessage(message, MessageDirection.Input);
            base.InnerChannel.Send(message, helper.RemainingTime());
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

