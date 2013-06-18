namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ContextRequestSessionChannel : ContextRequestChannelBase<IRequestSessionChannel>, IRequestSessionChannel, IRequestChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
    {
        public ContextRequestSessionChannel(ChannelManagerBase channelManager, IRequestSessionChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled) : base(channelManager, innerChannel, contextExchangeMechanism, callbackAddress, contextManagementEnabled)
        {
        }

        public IOutputSession Session
        {
            get
            {
                return base.InnerChannel.Session;
            }
        }
    }
}

