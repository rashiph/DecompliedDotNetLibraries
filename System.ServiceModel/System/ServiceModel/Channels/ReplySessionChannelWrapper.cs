namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ReplySessionChannelWrapper : ReplyChannelWrapper, IReplySessionChannel, IReplyChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
        public ReplySessionChannelWrapper(ChannelManagerBase channelManager, IReplySessionChannel innerChannel, RequestContext firstRequest) : base(channelManager, innerChannel, firstRequest)
        {
        }

        private IReplySessionChannel InnerChannel
        {
            get
            {
                return (IReplySessionChannel) base.InnerChannel;
            }
        }

        public IInputSession Session
        {
            get
            {
                return this.InnerChannel.Session;
            }
        }
    }
}

