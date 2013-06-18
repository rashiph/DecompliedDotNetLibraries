namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableInputListenerOverReply : ReliableListenerOverReply<IInputSessionChannel, ReliableInputSessionChannelOverReply>
    {
        public ReliableInputListenerOverReply(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ReliableInputSessionChannelOverReply CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverReply(this, binder, base.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(ReliableInputSessionChannelOverReply reliableChannel, RequestContext context, WsrmMessageInfo info)
        {
            reliableChannel.ProcessDemuxedRequest(reliableChannel.Binder.WrapRequestContext(context), info);
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

