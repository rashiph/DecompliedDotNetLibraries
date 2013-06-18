namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class TransactionReceiveChannelGeneric<TChannel> : TransactionChannel<TChannel>, IInputChannel, IChannel, ICommunicationObject where TChannel: class, IInputChannel
    {
        private MessageDirection receiveMessageDirection;

        public TransactionReceiveChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel, MessageDirection direction) : base(channelManager, innerChannel)
        {
            this.receiveMessageDirection = direction;
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InputChannel.HelpBeginReceive(this, timeout, callback, state);
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
            return InputChannel.HelpEndReceive(result);
        }

        public virtual bool EndTryReceive(IAsyncResult asyncResult, out Message message)
        {
            if (!base.InnerChannel.EndTryReceive(asyncResult, out message))
            {
                return false;
            }
            if (message != null)
            {
                this.ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            }
            return true;
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
            return InputChannel.HelpReceive(this, timeout);
        }

        public virtual bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (!base.InnerChannel.TryReceive(timeout, out message))
            {
                return false;
            }
            if (message != null)
            {
                this.ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            }
            return true;
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

