namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class InputChannelWrapper : ChannelWrapper<IInputChannel, Message>, IInputChannel, IChannel, ICommunicationObject
    {
        public InputChannelWrapper(ChannelManagerBase channelManager, IInputChannel innerChannel, Message firstMessage) : base(channelManager, innerChannel, firstMessage)
        {
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            Message firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return new ChannelWrapper<IInputChannel, Message>.ReceiveAsyncResult(firstItem, callback, state);
            }
            return base.InnerChannel.BeginReceive(callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Message firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return new ChannelWrapper<IInputChannel, Message>.ReceiveAsyncResult(firstItem, callback, state);
            }
            return base.InnerChannel.BeginReceive(timeout, callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Message firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return new ChannelWrapper<IInputChannel, Message>.ReceiveAsyncResult(firstItem, callback, state);
            }
            return base.InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (base.HaveFirstItem())
            {
                return new ChannelWrapper<IInputChannel, Message>.WaitAsyncResult(callback, state);
            }
            return base.InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        protected override void CloseFirstItem(TimeSpan timeout)
        {
            Message firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                TypedChannelDemuxer.AbortMessage(firstItem);
            }
        }

        public Message EndReceive(IAsyncResult result)
        {
            if (result is ChannelWrapper<IInputChannel, Message>.ReceiveAsyncResult)
            {
                return ChannelWrapper<IInputChannel, Message>.ReceiveAsyncResult.End(result);
            }
            return base.InnerChannel.EndReceive(result);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (result is ChannelWrapper<IInputChannel, Message>.ReceiveAsyncResult)
            {
                message = ChannelWrapper<IInputChannel, Message>.ReceiveAsyncResult.End(result);
                return true;
            }
            return base.InnerChannel.EndTryReceive(result, out message);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            if (result is ChannelWrapper<IInputChannel, Message>.WaitAsyncResult)
            {
                return ChannelWrapper<IInputChannel, Message>.WaitAsyncResult.End(result);
            }
            return base.InnerChannel.EndWaitForMessage(result);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        public Message Receive()
        {
            Message firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return firstItem;
            }
            return base.InnerChannel.Receive();
        }

        public Message Receive(TimeSpan timeout)
        {
            Message firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return firstItem;
            }
            return base.InnerChannel.Receive(timeout);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = base.GetFirstItem();
            return ((message != null) || base.InnerChannel.TryReceive(timeout, out message));
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return (base.HaveFirstItem() || base.InnerChannel.WaitForMessage(timeout));
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

