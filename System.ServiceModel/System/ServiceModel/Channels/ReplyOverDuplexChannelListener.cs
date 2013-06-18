namespace System.ServiceModel.Channels
{
    using System;

    internal class ReplyOverDuplexChannelListener : ReplyOverDuplexChannelListenerBase<IReplyChannel, IDuplexChannel>
    {
        public ReplyOverDuplexChannelListener(BindingContext context) : base(context)
        {
        }

        protected override IReplyChannel CreateWrappedChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel)
        {
            return new ReplyOverDuplexChannel(channelManager, innerChannel);
        }
    }
}

