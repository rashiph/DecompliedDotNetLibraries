namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class MessageRpcInvokeNotification : IInvokeReceivedNotification
    {
        private ServiceModelActivity activity;
        private ChannelHandler handler;

        public MessageRpcInvokeNotification(ServiceModelActivity activity, ChannelHandler handler)
        {
            this.activity = activity;
            this.handler = handler;
        }

        public void NotifyInvokeReceived()
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ChannelHandler.Register(this.handler);
            }
            this.DidInvokerEnsurePump = true;
        }

        public void NotifyInvokeReceived(RequestContext request)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ChannelHandler.Register(this.handler, request);
            }
            this.DidInvokerEnsurePump = true;
        }

        public bool DidInvokerEnsurePump { get; set; }
    }
}

