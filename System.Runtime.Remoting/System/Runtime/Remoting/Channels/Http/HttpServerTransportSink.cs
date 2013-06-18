namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;

    internal class HttpServerTransportSink : IServerChannelSink, IChannelSinkBase
    {
        private IServerChannelSink _nextSink;
        private static string s_serverHeader = ("MS .NET Remoting, MS .NET CLR " + Environment.Version.ToString());

        public HttpServerTransportSink(IServerChannelSink nextSink)
        {
            this._nextSink = nextSink;
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            HttpServerSocketHandler handler = null;
            handler = (HttpServerSocketHandler) state;
            handler.SendResponse(stream, "200", "OK", headers);
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
            HttpServerSocketHandler handler = (HttpServerSocketHandler) state;
            if (handler.AllowChunkedResponse)
            {
                return handler.GetResponseStream("200", "OK", headers);
            }
            return null;
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotSupportedException();
        }

        internal void ServiceRequest(object state)
        {
            IMessage message;
            ITransportHeaders headers2;
            Stream stream2;
            HttpServerSocketHandler handler = (HttpServerSocketHandler) state;
            ITransportHeaders requestHeaders = handler.ReadHeaders();
            Stream requestStream = handler.GetRequestStream();
            requestHeaders["__CustomErrorsEnabled"] = handler.CustomErrorsEnabled();
            ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
            sinkStack.Push(this, handler);
            ServerProcessing processing = this._nextSink.ProcessMessage(sinkStack, null, requestHeaders, requestStream, out message, out headers2, out stream2);
            switch (processing)
            {
                case ServerProcessing.Complete:
                    sinkStack.Pop(this);
                    handler.SendResponse(stream2, "200", "OK", headers2);
                    break;

                case ServerProcessing.OneWay:
                    handler.SendResponse(null, "202", "Accepted", headers2);
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

        internal static string ServerHeader
        {
            get
            {
                return s_serverHeader;
            }
        }
    }
}

