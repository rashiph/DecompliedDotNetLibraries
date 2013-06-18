namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableInputListenerOverDuplexSession : ReliableListenerOverDuplexSession<IInputSessionChannel, ReliableInputSessionChannelOverDuplex>
    {
        public ReliableInputListenerOverDuplexSession(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ReliableInputSessionChannelOverDuplex CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverDuplex(this, binder, base.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(IDuplexSessionChannel channel, Message message, ReliableInputSessionChannelOverDuplex reliableChannel, WsrmMessageInfo info, bool newChannel)
        {
            if (!newChannel && !reliableChannel.Binder.UseNewChannel(channel))
            {
                message.Close();
                channel.Abort();
            }
            else
            {
                reliableChannel.ProcessDemuxedMessage(info);
            }
        }

        protected override bool Duplex
        {
            get
            {
                return false;
            }
        }
    }
}

