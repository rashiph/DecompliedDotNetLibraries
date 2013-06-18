namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ContextInputChannel : ContextInputChannelBase<IInputChannel>, IInputChannel, IChannel, ICommunicationObject
    {
        public ContextInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism) : base(channelManager, innerChannel, contextExchangeMechanism)
        {
        }
    }
}

