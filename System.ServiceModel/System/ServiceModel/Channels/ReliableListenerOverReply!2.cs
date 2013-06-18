namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class ReliableListenerOverReply<TChannel, TReliableChannel> : ReliableListenerOverDatagram<TChannel, TReliableChannel, IReplyChannel, RequestContext> where TChannel: class, IChannel where TReliableChannel: class, IChannel
    {
        protected ReliableListenerOverReply(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
            base.FaultHelper = new ReplyFaultHelper(context.Binding.SendTimeout, context.Binding.CloseTimeout);
        }

        protected override IAsyncResult BeginTryReceiveItem(IReplyChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginTryReceiveRequest(TimeSpan.MaxValue, callback, state);
        }

        protected override void DisposeItem(RequestContext item)
        {
            ((IDisposable) item.RequestMessage).Dispose();
            ((IDisposable) item).Dispose();
        }

        protected override void EndTryReceiveItem(IReplyChannel channel, IAsyncResult result, out RequestContext item)
        {
            channel.EndTryReceiveRequest(result, out item);
        }

        protected override Message GetMessage(RequestContext item)
        {
            return item.RequestMessage;
        }

        protected override void SendReply(Message reply, IReplyChannel channel, RequestContext item)
        {
            if (FaultHelper.AddressReply(item.RequestMessage, reply))
            {
                item.Reply(reply);
            }
        }
    }
}

