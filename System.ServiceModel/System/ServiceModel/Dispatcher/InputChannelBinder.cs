namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class InputChannelBinder : IChannelBinder
    {
        private IInputChannel channel;
        private Uri listenUri;

        internal InputChannelBinder(IInputChannel channel, Uri listenUri)
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
            return this.channel.BeginTryReceive(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginWaitForMessage(timeout, callback, state);
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
            Message message;
            if (this.channel.EndTryReceive(result, out message))
            {
                requestContext = this.WrapMessage(message);
                return true;
            }
            requestContext = null;
            return false;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.channel.EndWaitForMessage(result);
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
            Message message;
            if (this.channel.TryReceive(timeout, out message))
            {
                requestContext = this.WrapMessage(message);
                return true;
            }
            requestContext = null;
            return false;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.channel.WaitForMessage(timeout);
        }

        private RequestContext WrapMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }
            return new InputRequestContext(message, this);
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

        private class InputRequestContext : RequestContextBase
        {
            private InputChannelBinder binder;

            internal InputRequestContext(Message request, InputChannelBinder binder) : base(request, TimeSpan.Zero, TimeSpan.Zero)
            {
                this.binder = binder;
            }

            protected override void OnAbort()
            {
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
            }
        }
    }
}

