namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class NamedPipeReplyChannelListener : NamedPipeChannelListener<IReplyChannel, ReplyChannelAcceptor>, ISingletonChannelListener
    {
        private ReplyChannelAcceptor replyAcceptor;

        public NamedPipeReplyChannelListener(NamedPipeTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
            this.replyAcceptor = new ConnectionOrientedTransportChannelListener.ConnectionOrientedTransportReplyChannelAcceptor(this);
        }

        void ISingletonChannelListener.ReceiveRequest(RequestContext requestContext, Action callback, bool canDispatchOnThisThread)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40012, System.ServiceModel.SR.GetString("TraceCodeNamedPipeChannelMessageReceived"), requestContext.RequestMessage);
            }
            this.replyAcceptor.Enqueue(requestContext, callback, canDispatchOnThisThread);
        }

        protected override ReplyChannelAcceptor ChannelAcceptor
        {
            get
            {
                return this.replyAcceptor;
            }
        }

        TimeSpan ISingletonChannelListener.ReceiveTimeout
        {
            get
            {
                return base.InternalReceiveTimeout;
            }
        }
    }
}

