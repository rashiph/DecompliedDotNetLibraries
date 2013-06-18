namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class OutputChannelBinder : IChannelBinder
    {
        private IOutputChannel channel;

        internal OutputChannelBinder(IOutputChannel channel)
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
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginSend(message, timeout, callback, state);
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
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void EndSend(IAsyncResult result)
        {
            this.channel.EndSend(result);
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
            throw TraceUtility.ThrowHelperError(new NotImplementedException(), message);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.channel.Send(message, timeout);
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
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

