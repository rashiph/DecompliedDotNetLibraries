namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class ContextReplyChannel : LayeredChannel<IReplyChannel>, IReplyChannel, IChannel, ICommunicationObject
    {
        private ContextExchangeMechanism contextExchangeMechanism;

        public ContextReplyChannel(ChannelManagerBase channelManager, IReplyChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism) : base(channelManager, innerChannel)
        {
            this.contextExchangeMechanism = contextExchangeMechanism;
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginReceiveRequest(callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginTryReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginWaitForRequest(timeout, callback, state);
        }

        private ContextChannelRequestContext CreateContextChannelRequestContext(RequestContext innerContext)
        {
            ServiceContextProtocol contextProtocol = new ServiceContextProtocol(this.contextExchangeMechanism);
            contextProtocol.OnIncomingMessage(innerContext.RequestMessage);
            return new ContextChannelRequestContext(innerContext, contextProtocol, base.DefaultSendTimeout);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            RequestContext innerContext = base.InnerChannel.EndReceiveRequest(result);
            if (innerContext == null)
            {
                return null;
            }
            return this.CreateContextChannelRequestContext(innerContext);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            RequestContext context2;
            context = null;
            if (!base.InnerChannel.EndTryReceiveRequest(result, out context2))
            {
                return false;
            }
            if (context2 != null)
            {
                context = this.CreateContextChannelRequestContext(context2);
            }
            return true;
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return base.InnerChannel.EndWaitForRequest(result);
        }

        public RequestContext ReceiveRequest()
        {
            RequestContext innerContext = base.InnerChannel.ReceiveRequest();
            if (innerContext == null)
            {
                return null;
            }
            return this.CreateContextChannelRequestContext(innerContext);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            RequestContext innerContext = base.InnerChannel.ReceiveRequest(timeout);
            if (innerContext == null)
            {
                return null;
            }
            return this.CreateContextChannelRequestContext(innerContext);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            RequestContext context2;
            if (base.InnerChannel.TryReceiveRequest(timeout, out context2))
            {
                context = this.CreateContextChannelRequestContext(context2);
                return true;
            }
            context = null;
            return false;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return base.InnerChannel.WaitForRequest(timeout);
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return base.InnerChannel.LocalAddress;
            }
        }
    }
}

