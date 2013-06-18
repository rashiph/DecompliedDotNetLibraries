namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Text;
    using System.Threading;

    internal sealed class HttpServerSocketHandler : HttpSocketHandler
    {
        private static byte[] _bufferhttpContinue = Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");
        private bool _chunkedEncoding;
        private long _connectionId;
        private static long _connectionIdCounter = 0L;
        private int _contentLength;
        private bool _keepAlive;
        private HttpReadingStream _requestStream;
        private HttpServerResponseStream _responseStream;
        private System.Runtime.Remoting.Channels.Http.HttpVersion _version;
        private static ValidateByteDelegate s_validateVerbDelegate = new ValidateByteDelegate(HttpServerSocketHandler.ValidateVerbCharacter);

        internal HttpServerSocketHandler(Socket socket, RequestQueue requestQueue, Stream stream) : base(socket, requestQueue, stream)
        {
            this._connectionId = Interlocked.Increment(ref _connectionIdCounter);
        }

        public bool CanServiceAnotherRequest()
        {
            if ((!this._keepAlive || (this._requestStream == null)) || (!this._requestStream.FoundEnd && !this._requestStream.ReadToEnd()))
            {
                return false;
            }
            return true;
        }

        private string GenerateFaultString(Exception e)
        {
            if (!base.CustomErrorsEnabled())
            {
                return e.ToString();
            }
            return CoreChannel.GetResourceString("Remoting_InternalError");
        }

        public Stream GetRequestStream()
        {
            if (this._chunkedEncoding)
            {
                this._requestStream = new HttpChunkedReadingStream(this);
            }
            else
            {
                this._requestStream = new HttpFixedLengthReadingStream(this, this._contentLength);
            }
            return this._requestStream;
        }

        public Stream GetResponseStream(string statusCode, string reasonPhrase, ITransportHeaders headers)
        {
            bool flag = false;
            bool flag2 = false;
            int length = 0;
            object obj2 = headers["__HttpStatusCode"];
            string str = headers["__HttpReasonPhrase"] as string;
            if (obj2 != null)
            {
                statusCode = obj2.ToString();
            }
            if (str != null)
            {
                reasonPhrase = str;
            }
            if (!this.CanServiceAnotherRequest())
            {
                headers["Connection"] = "Close";
            }
            object obj3 = headers["Content-Length"];
            if (obj3 != null)
            {
                flag = true;
                if (obj3 is int)
                {
                    length = (int) obj3;
                }
                else
                {
                    length = Convert.ToInt32(obj3, CultureInfo.InvariantCulture);
                }
            }
            flag2 = this.AllowChunkedResponse && !flag;
            if (flag2)
            {
                headers["Transfer-Encoding"] = "chunked";
            }
            ChunkedMemoryStream outputStream = new ChunkedMemoryStream(CoreChannel.BufferPool);
            base.WriteResponseFirstLine(statusCode, reasonPhrase, outputStream);
            base.WriteHeaders(headers, outputStream);
            outputStream.WriteTo(base.NetStream);
            outputStream.Close();
            if (flag2)
            {
                this._responseStream = new HttpChunkedResponseStream(base.NetStream);
            }
            else
            {
                this._responseStream = new HttpFixedLengthResponseStream(base.NetStream, length);
            }
            return this._responseStream;
        }

        protected override void PrepareForNewMessage()
        {
            this._requestStream = null;
            this._responseStream = null;
            this._contentLength = 0;
            this._chunkedEncoding = false;
            this._keepAlive = false;
        }

        private bool ReadFirstLine(out string verb, out string requestURI, out string version)
        {
            int num;
            verb = null;
            requestURI = null;
            version = null;
            verb = base.ReadToChar(' ', s_validateVerbDelegate);
            byte[] uriBytes = base.ReadToByte(0x20);
            HttpChannelHelper.DecodeUriInPlace(uriBytes, out num);
            requestURI = Encoding.UTF8.GetString(uriBytes, 0, num);
            version = base.ReadToEndOfLine();
            return true;
        }

        public BaseTransportHeaders ReadHeaders()
        {
            string str;
            string str2;
            string str3;
            string str5;
            bool bSendContinue = false;
            BaseTransportHeaders headers = new BaseTransportHeaders();
            this.ReadFirstLine(out str, out str2, out str3);
            if (((str == null) || (str2 == null)) || (str3 == null))
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_UnableToReadFirstLine"));
            }
            if (str3.Equals("HTTP/1.1"))
            {
                this._version = System.Runtime.Remoting.Channels.Http.HttpVersion.V1_1;
            }
            else if (str3.Equals("HTTP/1.0"))
            {
                this._version = System.Runtime.Remoting.Channels.Http.HttpVersion.V1_0;
            }
            else
            {
                this._version = System.Runtime.Remoting.Channels.Http.HttpVersion.V1_1;
            }
            if (this._version == System.Runtime.Remoting.Channels.Http.HttpVersion.V1_1)
            {
                this._keepAlive = true;
            }
            else
            {
                this._keepAlive = false;
            }
            if (HttpChannelHelper.ParseURL(str2, out str5) == null)
            {
                str5 = str2;
            }
            headers["__RequestVerb"] = str;
            headers.RequestUri = str5;
            headers["__HttpVersion"] = str3;
            if ((this._version == System.Runtime.Remoting.Channels.Http.HttpVersion.V1_1) && (str.Equals("POST") || str.Equals("PUT")))
            {
                bSendContinue = true;
            }
            base.ReadToEndOfHeaders(headers, out this._chunkedEncoding, out this._contentLength, ref this._keepAlive, ref bSendContinue);
            if (bSendContinue && (this._version != System.Runtime.Remoting.Channels.Http.HttpVersion.V1_0))
            {
                this.SendContinue();
            }
            headers["__IPAddress"] = ((IPEndPoint) base.NetSocket.RemoteEndPoint).Address;
            headers["__ConnectionId"] = this._connectionId;
            return headers;
        }

        private void SendContinue()
        {
            base.NetStream.Write(_bufferhttpContinue, 0, _bufferhttpContinue.Length);
        }

        protected override void SendErrorMessageIfPossible(Exception e)
        {
            if ((this._responseStream == null) && !(e is SocketException))
            {
                Stream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false));
                writer.WriteLine(this.GenerateFaultString(e));
                writer.Flush();
                this.SendResponse(stream, "500", CoreChannel.GetResourceString("Remoting_InternalError"), null);
            }
        }

        public void SendResponse(Stream httpContentStream, string statusCode, string reasonPhrase, ITransportHeaders headers)
        {
            if (this._responseStream != null)
            {
                this._responseStream.Close();
                if (this._responseStream != httpContentStream)
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_WrongResponseStream"));
                }
                this._responseStream = null;
            }
            else
            {
                if (headers == null)
                {
                    headers = new TransportHeaders();
                }
                string serverHeader = (string) headers["Server"];
                if (serverHeader != null)
                {
                    serverHeader = HttpServerTransportSink.ServerHeader + ", " + serverHeader;
                }
                else
                {
                    serverHeader = HttpServerTransportSink.ServerHeader;
                }
                headers["Server"] = serverHeader;
                if (!this.AllowChunkedResponse && (httpContentStream != null))
                {
                    headers["Content-Length"] = httpContentStream.Length.ToString(CultureInfo.InvariantCulture);
                }
                else if (httpContentStream == null)
                {
                    headers["Content-Length"] = "0";
                }
                this.GetResponseStream(statusCode, reasonPhrase, headers);
                if (httpContentStream != null)
                {
                    StreamHelper.CopyStream(httpContentStream, this._responseStream);
                    this._responseStream.Close();
                    httpContentStream.Close();
                }
                this._responseStream = null;
            }
        }

        private static bool ValidateVerbCharacter(byte b)
        {
            if (!char.IsLetter((char) b) && (b != 0x2d))
            {
                return false;
            }
            return true;
        }

        public bool AllowChunkedResponse
        {
            get
            {
                return false;
            }
        }
    }
}

