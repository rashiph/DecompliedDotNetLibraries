namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class RequestChannelBinder : IChannelBinder
    {
        private IRequestChannel channel;

        internal RequestChannelBinder(IRequestChannel channel)
        {
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }
            this.channel = channel;
        }

        public void Abort()
        {
            this.channel.Abort();
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginRequest(message, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginRequest(message, timeout, callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
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
            return this.channel.EndRequest(result);
        }

        public void EndSend(IAsyncResult result)
        {
            this.ValidateNullReply(this.channel.EndRequest(result));
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            return this.channel.Request(message, timeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.ValidateNullReply(this.channel.Request(message, timeout));
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        private void ValidateNullReply(Message message)
        {
            if (message != null)
            {
                throw TraceUtility.ThrowHelperError(ProtocolException.OneWayOperationReturnedNonNull(message), message);
            }
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
                return (this.channel is ISessionChannel<IOutputSession>);
            }
        }

        public Uri ListenUri
        {
            get
            {
                return null;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return EndpointAddress.AnonymousAddress;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.channel.RemoteAddress;
            }
        }
    }
}

