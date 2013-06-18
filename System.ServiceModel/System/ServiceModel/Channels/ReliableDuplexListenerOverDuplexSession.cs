namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableDuplexListenerOverDuplexSession : ReliableListenerOverDuplexSession<IDuplexSessionChannel, ServerReliableDuplexSessionChannel>
    {
        public ReliableDuplexListenerOverDuplexSession(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ServerReliableDuplexSessionChannel CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ServerReliableDuplexSessionChannel(this, binder, base.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(IDuplexSessionChannel channel, Message message, ServerReliableDuplexSessionChannel reliableChannel, WsrmMessageInfo info, bool newChannel)
        {
            if (!newChannel)
            {
                IServerReliableChannelBinder binder = (IServerReliableChannelBinder) reliableChannel.Binder;
                if (!binder.UseNewChannel(channel))
                {
                    message.Close();
                    channel.Abort();
                    return;
                }
            }
            reliableChannel.ProcessDemuxedMessage(info);
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

