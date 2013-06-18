namespace System.ServiceModel.Channels
{
    using System;

    internal class ReplyOverDuplexChannel : ReplyOverDuplexChannelBase<IDuplexChannel>
    {
        public ReplyOverDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel) : base(channelManager, innerChannel)
        {
        }
    }
}

