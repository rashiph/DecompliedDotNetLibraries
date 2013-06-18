namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class ReliableListenerOverDuplex<TChannel, TReliableChannel> : ReliableListenerOverDatagram<TChannel, TReliableChannel, IDuplexChannel, Message> where TChannel: class, IChannel where TReliableChannel: class, IChannel
    {
        protected ReliableListenerOverDuplex(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
            base.FaultHelper = new SendFaultHelper(context.Binding.SendTimeout, context.Binding.CloseTimeout);
        }

        protected override IAsyncResult BeginTryReceiveItem(IDuplexChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginTryReceive(TimeSpan.MaxValue, callback, state);
        }

        protected override void DisposeItem(Message item)
        {
            ((IDisposable) item).Dispose();
        }

        protected override void EndTryReceiveItem(IDuplexChannel channel, IAsyncResult result, out Message item)
        {
            channel.EndTryReceive(result, out item);
        }

        protected override Message GetMessage(Message item)
        {
            return item;
        }

        protected override void SendReply(Message reply, IDuplexChannel channel, Message item)
        {
            if (FaultHelper.AddressReply(item, reply))
            {
                channel.Send(reply);
            }
        }
    }
}

