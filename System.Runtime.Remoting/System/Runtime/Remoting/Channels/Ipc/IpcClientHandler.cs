namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Runtime.Remoting.Messaging;

    internal class IpcClientHandler : IpcServerHandler
    {
        private bool _bChunked;
        private bool _bOneWayRequest;
        private int _contentLength;
        private TcpReadingStream _responseStream;
        private IpcClientTransportSink _sink;

        internal IpcClientHandler(IpcPort port, Stream stream, IpcClientTransportSink sink) : base(port, null, stream)
        {
            this._sink = sink;
        }

        internal Stream GetResponseStream()
        {
            this._responseStream = new TcpFixedLengthReadingStream(this, this._contentLength);
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

        internal void ReturnToCache()
        {
            this._sink.Cache.ReleaseConnection(base._port);
        }

        internal void SendRequest(IMessage msg, ITransportHeaders headers, Stream contentStream)
        {
            IMethodCallMessage message = (IMethodCallMessage) msg;
            int length = (int) contentStream.Length;
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
            base.WriteInt32(length, outputStream);
            base.WriteUInt16(4, outputStream);
            base.WriteByte(1, outputStream);
            base.WriteCountedString(uri, outputStream);
            base.WriteHeaders(headers, outputStream);
            outputStream.WriteTo(base.NetStream);
            outputStream.Close();
            StreamHelper.CopyStream(contentStream, base.NetStream);
            contentStream.Close();
        }
    }
}

