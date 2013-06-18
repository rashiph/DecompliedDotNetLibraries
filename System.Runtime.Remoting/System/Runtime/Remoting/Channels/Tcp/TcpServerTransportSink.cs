namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Principal;
    using System.Threading;

    internal class TcpServerTransportSink : IServerChannelSink, IChannelSinkBase
    {
        private bool _impersonate;
        private IServerChannelSink _nextSink;
        private const int s_MaxSize = 0x2000000;

        internal TcpServerTransportSink(IServerChannelSink nextSink, bool impersonate)
        {
            this._nextSink = nextSink;
            this._impersonate = impersonate;
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            TcpServerSocketHandler handler = null;
            handler = (TcpServerSocketHandler) state;
            handler.SendResponse(headers, stream);
            if (handler.CanServiceAnotherRequest())
            {
                handler.BeginReadMessage();
            }
            else
            {
                handler.Close();
            }
        }

        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotSupportedException();
        }

        internal void ServiceRequest(object state)
        {
            ITransportHeaders headers2;
            Stream stream2;
            ServerProcessing processing;
            TcpServerSocketHandler handler = (TcpServerSocketHandler) state;
            ITransportHeaders requestHeaders = handler.ReadHeaders();
            Stream requestStream = handler.GetRequestStream();
            requestHeaders["__CustomErrorsEnabled"] = handler.CustomErrorsEnabled();
            ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
            sinkStack.Push(this, handler);
            WindowsIdentity impersonationIdentity = handler.ImpersonationIdentity;
            WindowsImpersonationContext context = null;
            IPrincipal currentPrincipal = null;
            bool flag = false;
            if (impersonationIdentity != null)
            {
                currentPrincipal = Thread.CurrentPrincipal;
                flag = true;
                if (this._impersonate)
                {
                    Thread.CurrentPrincipal = new WindowsPrincipal(impersonationIdentity);
                    context = impersonationIdentity.Impersonate();
                }
                else
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(impersonationIdentity, null);
                }
            }
            try
            {
                try
                {
                    IMessage message;
                    processing = this._nextSink.ProcessMessage(sinkStack, null, requestHeaders, requestStream, out message, out headers2, out stream2);
                }
                finally
                {
                    if (flag)
                    {
                        Thread.CurrentPrincipal = currentPrincipal;
                    }
                    if (this._impersonate)
                    {
                        context.Undo();
                    }
                }
            }
            catch
            {
                throw;
            }
            switch (processing)
            {
                case ServerProcessing.Complete:
                    sinkStack.Pop(this);
                    handler.SendResponse(headers2, stream2);
                    break;

                case ServerProcessing.OneWay:
                    handler.SendResponse(headers2, stream2);
                    break;

                case ServerProcessing.Async:
                    sinkStack.StoreAndDispatch(this, handler);
                    break;
            }
            if (processing != ServerProcessing.Async)
            {
                if (handler.CanServiceAnotherRequest())
                {
                    handler.BeginReadMessage();
                }
                else
                {
                    handler.Close();
                }
            }
        }

        public IServerChannelSink NextChannelSink
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._nextSink;
            }
        }

        public IDictionary Properties
        {
            get
            {
                return null;
            }
        }
    }
}

