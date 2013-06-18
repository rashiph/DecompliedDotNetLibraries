namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class TransactionDuplexChannelGeneric<TChannel> : TransactionReceiveChannelGeneric<TChannel>, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject where TChannel: class, IDuplexChannel
    {
        private MessageDirection sendMessageDirection;

        public TransactionDuplexChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel, MessageDirection direction) : base(channelManager, innerChannel, direction)
        {
            if (direction == MessageDirection.Input)
            {
                this.sendMessageDirection = MessageDirection.Output;
            }
            else
            {
                this.sendMessageDirection = MessageDirection.Input;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
        }

        public virtual IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback asyncCallback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.WriteTransactionDataToMessage(message, this.sendMessageDirection);
            return base.InnerChannel.BeginSend(message, helper.RemainingTime(), asyncCallback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            base.InnerChannel.EndSend(result);
        }

        public override void ReadTransactionDataFromMessage(Message message, MessageDirection direction)
        {
            try
            {
                base.ReadTransactionDataFromMessage(message, direction);
            }
            catch (FaultException exception)
            {
                Message reply = Message.CreateMessage(message.Version, exception.CreateMessageFault(), exception.Action);
                RequestReplyCorrelator.AddressReply(reply, message);
                RequestReplyCorrelator.PrepareReply(reply, message.Headers.MessageId);
                try
                {
                    this.Send(reply);
                }
                finally
                {
                    reply.Close();
                }
                throw;
            }
        }

        public void Send(Message message)
        {
            this.Send(message, base.DefaultSendTimeout);
        }

        public virtual void Send(Message message, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.WriteTransactionDataToMessage(message, this.sendMessageDirection);
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

