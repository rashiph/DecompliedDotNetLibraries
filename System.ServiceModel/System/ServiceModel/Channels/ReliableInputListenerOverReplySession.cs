namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class ReliableInputListenerOverReplySession : ReliableListenerOverReplySession<IInputSessionChannel, ReliableInputSessionChannelOverReply>
    {
        public ReliableInputListenerOverReplySession(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
        }

        protected override ReliableInputSessionChannelOverReply CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder)
        {
            binder.Open(base.InternalOpenTimeout);
            return new ReliableInputSessionChannelOverReply(this, binder, base.FaultHelper, id);
        }

        protected override void ProcessSequencedItem(IReplySessionChannel channel, RequestContext context, ReliableInputSessionChannelOverReply reliableChannel, WsrmMessageInfo info, bool newChannel)
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
                return false;
            }
        }
    }
}

