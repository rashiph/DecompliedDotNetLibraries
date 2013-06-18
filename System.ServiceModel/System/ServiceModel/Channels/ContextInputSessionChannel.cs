namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ContextInputSessionChannel : ContextInputChannelBase<IInputSessionChannel>, IInputSessionChannel, IInputChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
        public ContextInputSessionChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism) : base(channelManager, innerChannel, contextExchangeMechanism)
        {
        }

        public IInputSession Session
        {
            get
            {
                return base.InnerChannel.Session;
            }
        }
    }
}

