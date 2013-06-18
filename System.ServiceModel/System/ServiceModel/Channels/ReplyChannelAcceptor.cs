namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class ReplyChannelAcceptor : SingletonChannelAcceptor<IReplyChannel, ReplyChannel, RequestContext>
    {
        public ReplyChannelAcceptor(ChannelManagerBase channelManager) : base(channelManager)
        {
        }

        protected override ReplyChannel OnCreateChannel()
        {
            return new ReplyChannel(base.ChannelManager, null);
        }

        protected override void OnTraceMessageReceived(RequestContext requestContext)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40013, System.ServiceModel.SR.GetString("TraceCodeMessageReceived"), MessageTransmitTraceRecord.CreateReceiveTraceRecord((requestContext == null) ? null : requestContext.RequestMessage), this, null);
            }
        }
    }
}

