namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Web;

    internal class HttpHandlerTransportSink : IServerChannelSink, IChannelSinkBase
    {
        private const int _defaultChunkSize = 0x800;
        public IServerChannelSink _nextSink;

        public HttpHandlerTransportSink(IServerChannelSink nextSink)
        {
            this._nextSink = nextSink;
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            throw new NotSupportedException();
        }

        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public void HandleRequest(HttpContext context)
        {
            IMessage message;
            ITransportHeaders headers2;
            Stream stream2;
            HttpRequest request = context.Request;
            HttpResponse httpResponse = context.Response;
            BaseTransportHeaders requestHeaders = new BaseTransportHeaders();
            requestHeaders["__RequestVerb"] = request.HttpMethod;
            requestHeaders["__CustomErrorsEnabled"] = HttpRemotingHandler.CustomErrorsEnabled(context);
            requestHeaders.RequestUri = (string) context.Items["__requestUri"];
            NameValueCollection headers = request.Headers;
            foreach (string str in headers.AllKeys)
            {
                string str2 = headers[str];
                requestHeaders[str] = str2;
            }
            requestHeaders.IPAddress = IPAddress.Parse(request.UserHostAddress);
            Stream inputStream = request.InputStream;
            ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
            sinkStack.Push(this, null);
            switch (this._nextSink.ProcessMessage(sinkStack, null, requestHeaders, inputStream, out message, out headers2, out stream2))
            {
                case ServerProcessing.Complete:
                    this.SendResponse(httpResponse, 200, headers2, stream2);
                    return;

                case ServerProcessing.OneWay:
                    this.SendResponse(httpResponse, 0xca, headers2, stream2);
                    break;

                case ServerProcessing.Async:
                    break;

                default:
                    return;
            }
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotSupportedException();
        }

        private void SendResponse(HttpResponse httpResponse, int statusCode, ITransportHeaders responseHeaders, Stream httpContentStream)
        {
            if (responseHeaders != null)
            {
                string serverHeader = (string) responseHeaders["Server"];
                if (serverHeader != null)
                {
                    serverHeader = HttpServerTransportSink.ServerHeader + ", " + serverHeader;
                }
                else
                {
                    serverHeader = HttpServerTransportSink.ServerHeader;
                }
                responseHeaders["Server"] = serverHeader;
                object obj2 = responseHeaders["__HttpStatusCode"];
                if (obj2 != null)
                {
                    statusCode = Convert.ToInt32(obj2, CultureInfo.InvariantCulture);
                }
                if (httpContentStream != null)
                {
                    int length = -1;
                    try
                    {
                        if (httpContentStream != null)
                        {
                            length = (int) httpContentStream.Length;
                        }
                    }
                    catch
                    {
                    }
                    if (length != -1)
                    {
                        responseHeaders["Content-Length"] = length;
                    }
                }
                else
                {
                    responseHeaders["Content-Length"] = 0;
                }
                foreach (DictionaryEntry entry in responseHeaders)
                {
                    string key = (string) entry.Key;
                    if (!key.StartsWith("__", StringComparison.Ordinal))
                    {
                        httpResponse.AppendHeader(key, entry.Value.ToString());
                    }
                }
            }
            httpResponse.StatusCode = statusCode;
            Stream outputStream = httpResponse.OutputStream;
            if (httpContentStream != null)
            {
                StreamHelper.CopyStream(httpContentStream, outputStream);
                httpContentStream.Close();
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

