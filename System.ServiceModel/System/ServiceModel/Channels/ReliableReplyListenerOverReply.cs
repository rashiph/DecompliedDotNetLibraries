namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableReplyListenerOverReply : ReliableListenerOverReply<IReplySessionChannel, ReliableReplySessionChannel>
    {
        public ReliableReplyListenerOverReply(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ReliableReplySessionChannel CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ReliableReplySessionChannel(this, binder, base.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(ReliableReplySessionChannel reliableChannel, RequestContext context, WsrmMessageInfo info)
        {
            reliableChannel.ProcessDemuxedRequest(reliableChannel.Binder.WrapRequestContext(context), info);
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

