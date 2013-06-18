namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableReplyListenerOverReplySession : ReliableListenerOverReplySession<IReplySessionChannel, ReliableReplySessionChannel>
    {
        public ReliableReplyListenerOverReplySession(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ReliableReplySessionChannel CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ReliableReplySessionChannel(this, binder, base.FaultHelper, id, createSequenceInfo.OfferIdentifier);
        }

        protected override void ProcessSequencedItem(IReplySessionChannel channel, RequestContext context, ReliableReplySessionChannel reliableChannel, WsrmMessageInfo info, bool newChannel)
        {
            if (!newChannel && !reliableChannel.Binder.UseNewChannel(channel))
            {
                context.RequestMessage.Close();
                context.Abort();
                channel.Abort();
            }
            else
            {
                reliableChannel.ProcessDemuxedRequest(reliableChannel.Binder.WrapRequestContext(context), info);
            }
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

