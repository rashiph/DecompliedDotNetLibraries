namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ContextOutputSessionChannel : ContextOutputChannelBase<IOutputSessionChannel>, IOutputSessionChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
    {
        private ClientContextProtocol contextProtocol;

        public ContextOutputSessionChannel(ChannelManagerBase channelManager, IOutputSessionChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled) : base(channelManager, innerChannel)
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

        public IOutputSession Session
        {
            get
            {
                return base.InnerChannel.Session;
            }
        }
    }
}

