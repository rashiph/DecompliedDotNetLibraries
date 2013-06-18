namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class InputSessionChannelWrapper : InputChannelWrapper, IInputSessionChannel, IInputChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
        public InputSessionChannelWrapper(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, Message firstMessage) : base(channelManager, innerChannel, firstMessage)
        {
        }

        private IInputSessionChannel InnerChannel
        {
            get
            {
                return (IInputSessionChannel) base.InnerChannel;
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

