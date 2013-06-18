namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableInputListenerOverDuplex : ReliableListenerOverDuplex<IInputSessionChannel, ReliableInputSessionChannelOverDuplex>
    {
        public ReliableInputListenerOverDuplex(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ReliableInputSessionChannelOverDuplex CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverDuplex(this, binder, base.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(ReliableInputSessionChannelOverDuplex channel, Message message, WsrmMessageInfo info)
        {
            channel.ProcessDemuxedMessage(info);
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

