namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class TcpReplyChannelListener : TcpChannelListener<IReplyChannel, ReplyChannelAcceptor>, ISingletonChannelListener
    {
        private ReplyChannelAcceptor replyAcceptor;

        public TcpReplyChannelListener(TcpTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
            this.replyAcceptor = new ConnectionOrientedTransportChannelListener.ConnectionOrientedTransportReplyChannelAcceptor(this);
        }

        void ISingletonChannelListener.ReceiveRequest(RequestContext requestContext, Action callback, bool canDispatchOnThisThread)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40017, System.ServiceModel.SR.GetString("TraceCodeTcpChannelMessageReceived"), requestContext.RequestMessage);
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

