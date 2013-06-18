namespace System.ServiceModel.Channels
{
    using System;

    internal class ReplySessionOverDuplexSessionChannelListener : ReplyOverDuplexChannelListenerBase<IReplySessionChannel, IDuplexSessionChannel>
    {
        public ReplySessionOverDuplexSessionChannelListener(BindingContext context) : base(context)
        {
        }

        protected override IReplySessionChannel CreateWrappedChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel)
        {
            return new ReplySessionOverDuplexSessionChannel(channelManager, innerChannel);
        }
    }
}

