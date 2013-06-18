namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ContextOutputChannel : ContextOutputChannelBase<IOutputChannel>, IOutputChannel, IChannel, ICommunicationObject
    {
        private ClientContextProtocol contextProtocol;

        public ContextOutputChannel(ChannelManagerBase channelManager, IOutputChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled) : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ClientContextProtocol(contextExchangeMechanism, base.InnerChannel.Via, this, callbackAddress, contextManagementEnabled);
        }

        protected override System.ServiceModel.Channels.ContextProtocol ContextProtocol
        {
            get
            {
                return this.contextProtocol;
            }
        }

        protected override bool IsClient
        {
            get
            {
                return true;
            }
        }
    }
}

