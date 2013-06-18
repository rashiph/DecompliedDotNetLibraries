namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;

    internal class TcpClientSocketHandler : TcpSocketHandler
    {
        private bool _bChunked;
        private bool _bOneWayRequest;
        private int _contentLength;
        private string _machinePortAndSid;
        private Stream _requestStream;
        private TcpReadingStream _responseStream;
        private TcpClientTransportSink _sink;
        private static byte[] s_endOfLineBytes = Encoding.ASCII.GetBytes("\r\n");

        public TcpClientSocketHandler(Socket socket, string machinePortAndSid, Stream stream, TcpClientTransportSink sink) : base(socket, stream)
        {
            this._machinePortAndSid = machinePortAndSid;
            this._sink = sink;
        }

        public Stream GetRequestStream(IMessage msg, int contentLength, ITransportHeaders headers)
        {
            IMethodCallMessage message = (IMethodCallMessage) msg;
            string uri = message.Uri;
            this._bOneWayRequest = RemotingServices.IsOneWay(message.MethodBase);
            ChunkedMemoryStream outputStream = new ChunkedMemoryStream(CoreChannel.BufferPool);
            base.WritePreambleAndVersion(outputStream);
            if (!this._bOneWayRequest)
            {
                base.WriteUInt16(0, outputStream);
            }
            else
            {
                base.WriteUInt16(1, outputStream);
            }
            base.WriteUInt16(0, outputStream);
            base.WriteInt32(contentLength, outputStream);
            base.WriteUInt16(4, outputStream);
            base.WriteByte(1, outputStream);
            base.WriteCountedString(uri, outputStream);
            base.WriteHeaders(headers, outputStream);
            outputStream.WriteTo(base.NetStream);
            outputStream.Close();
            this._requestStream = base.NetStream;
            return this._requestStream;
        }

        public Stream GetResponseStream()
        {
            if (!this._bChunked)
            {
                this._responseStream = new TcpFixedLengthReadingStream(this, this._contentLength);
            }
            else
            {
                this._responseStream = new TcpChunkedReadingStream(this);
            }
            return this._responseStream;
        }

        public override void OnInputStreamClosed()
        {
            if (this._responseStream != null)
            {
                this._responseStream.ReadToEnd();
                this._responseStream = null;
            }
            this.ReturnToCache();
        }

        protected override void PrepareForNewMessage()
        {
            this._requestStream = null;
            this._responseStream = null;
        }

        public BaseTransportHeaders ReadHeaders()
        {
            ushort num;
            BaseTransportHeaders headers = new BaseTransportHeaders();
            base.ReadVersionAndOperation(out num);
            if (num != 2)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_ExpectingReplyOp"), new object[] { num.ToString(CultureInfo.CurrentCulture) }));
            }
            base.ReadContentLength(out this._bChunked, out this._contentLength);
            base.ReadToEndOfHeaders(headers);
            return headers;
        }

        public void ReturnToCache()
        {
            this._sink.ClientSocketCache.ReleaseSocket(this._machinePortAndSid, this);
        }

        public void SendRequest(IMessage msg, ITransportHeaders headers, Stream contentStream)
        {
            int length = (int) contentStream.Length;
            this.GetRequestStream(msg, length, headers);
            StreamHelper.CopyStream(contentStream, base.NetStream);
            contentStream.Close();
        }

        public bool OneWayRequest
        {
            get
            {
                return this._bOneWayRequest;
            }
        }
    }
}

