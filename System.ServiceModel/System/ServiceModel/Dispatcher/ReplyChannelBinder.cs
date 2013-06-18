namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class ReplyChannelBinder : IChannelBinder
    {
        private IReplyChannel channel;
        private Uri listenUri;

        internal ReplyChannelBinder(IReplyChannel channel, Uri listenUri)
        {
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }
            this.channel = channel;
            this.listenUri = listenUri;
        }

        public void Abort()
        {
            this.channel.Abort();
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginTryReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
            this.channel.Close(timeout);
        }

        public Message EndRequest(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void EndSend(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            return this.channel.EndTryReceiveRequest(result, out requestContext);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            return this.channel.TryReceiveRequest(timeout, out requestContext);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IChannel Channel
        {
            get
            {
                return this.channel;
            }
        }

        public bool HasSession
        {
            get
            {
                return (this.channel is ISessionChannel<IInputSession>);
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.channel.LocalAddress;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }
    }
}

