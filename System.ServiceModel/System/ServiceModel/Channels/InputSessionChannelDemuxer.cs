namespace System.ServiceModel.Channels
{
    using System;

    internal class InputSessionChannelDemuxer : SessionChannelDemuxer<IInputSessionChannel, Message>
    {
        public InputSessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions) : base(context, peekTimeout, maxPendingSessions)
        {
        }

        protected override void AbortItem(Message message)
        {
            TypedChannelDemuxer.AbortMessage(message);
        }

        protected override IAsyncResult BeginReceive(IInputSessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(callback, state);
        }

        protected override IAsyncResult BeginReceive(IInputSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(timeout, callback, state);
        }

        protected override IInputSessionChannel CreateChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, Message firstMessage)
        {
            return new InputSessionChannelWrapper(channelManager, innerChannel, firstMessage);
        }

        protected override void EndpointNotFound(IInputSessionChannel channel, Message message)
        {
            if (base.DemuxFailureHandler != null)
            {
                base.DemuxFailureHandler.HandleDemuxFailure(message);
            }
            this.AbortItem(message);
            channel.Abort();
        }

        protected override Message EndReceive(IInputSessionChannel channel, IAsyncResult result)
        {
            return channel.EndReceive(result);
        }

        protected override Message GetMessage(Message message)
        {
            return message;
        }
    }
}

