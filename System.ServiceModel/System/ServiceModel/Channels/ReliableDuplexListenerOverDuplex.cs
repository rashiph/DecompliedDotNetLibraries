namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableDuplexListenerOverDuplex : ReliableListenerOverDuplex<IDuplexSessionChannel, ServerReliableDuplexSessionChannel>
    {
        public ReliableDuplexListenerOverDuplex(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ServerReliableDuplexSessionChannel CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ServerReliableDuplexSessionChannel(this, binder, base.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(ServerReliableDuplexSessionChannel channel, Message message, WsrmMessageInfo info)
        {
            channel.ProcessDemuxedMessage(info);
        }

        protected override bool Duplex
        {
            get
            {
                return true;
            }
        }
    }
}

