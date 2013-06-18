namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class ReliableListenerOverReplySession<TChannel, TReliableChannel> : ReliableListenerOverSession<TChannel, TReliableChannel, IReplySessionChannel, IInputSession, RequestContext> where TChannel: class, IChannel where TReliableChannel: class, IChannel
    {
        protected ReliableListenerOverReplySession(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
            base.FaultHelper = new ReplyFaultHelper(context.Binding.SendTimeout, context.Binding.CloseTimeout);
        }

        protected override IAsyncResult BeginTryReceiveItem(IReplySessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginTryReceiveRequest(TimeSpan.MaxValue, callback, channel);
        }

        protected override void DisposeItem(RequestContext item)
        {
            ((IDisposable) item.RequestMessage).Dispose();
            ((IDisposable) item).Dispose();
        }

        protected override void EndTryReceiveItem(IReplySessionChannel channel, IAsyncResult result, out RequestContext item)
        {
            channel.EndTryReceiveRequest(result, out item);
        }

        protected override Message GetMessage(RequestContext item)
        {
            return item.RequestMessage;
        }

        protected override void SendReply(Message reply, IReplySessionChannel channel, RequestContext item)
        {
            if (FaultHelper.AddressReply(item.RequestMessage, reply))
            {
                item.Reply(reply);
            }
        }
    }
}

