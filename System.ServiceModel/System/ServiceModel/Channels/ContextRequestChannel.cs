namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ContextRequestChannel : ContextRequestChannelBase<IRequestChannel>, IRequestChannel, IChannel, ICommunicationObject
    {
        public ContextRequestChannel(ChannelManagerBase channelManager, IRequestChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled) : base(channelManager, innerChannel, contextExchangeMechanism, callbackAddress, contextManagementEnabled)
        {
        }
    }
}

