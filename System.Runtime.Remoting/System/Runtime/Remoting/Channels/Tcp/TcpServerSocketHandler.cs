namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Threading;

    internal sealed class TcpServerSocketHandler : TcpSocketHandler
    {
        private bool _bChunked;
        private bool _bOneWayRequest;
        private long _connectionId;
        private static long _connectionIdCounter = 0L;
        private int _contentLength;
        private TcpReadingStream _requestStream;
        private static byte[] s_endOfLineBytes = Encoding.ASCII.GetBytes("\r\n");

        internal TcpServerSocketHandler(Socket socket, RequestQueue requestQueue, Stream stream) : base(socket, requestQueue, stream)
        {
            this._connectionId = Interlocked.Increment(ref _connectionIdCounter);
        }

        public bool CanServiceAnotherRequest()
        {
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
            if (!this._bChunked)
            {
                this._requestStream = new TcpFixedLengthReadingStream(this, this._contentLength);
            }
            else
            {
                this._requestStream = new TcpChunkedReadingStream(this);
            }
            return this._requestStream;
        }

        protected override void PrepareForNewMessage()
        {
            if (this._requestStream != null)
            {
                if (!this._requestStream.FoundEnd)
                {
                    this._requestStream.ReadToEnd();
                }
                this._requestStream = null;
            }
        }

        public ITransportHeaders ReadHeaders()
        {
            ushort num;
            BaseTransportHeaders headers = new BaseTransportHeaders();
            base.ReadVersionAndOperation(out num);
            switch (num)
            {
                case 0:
                    this._bOneWayRequest = false;
                    break;

                case 1:
                    this._bOneWayRequest = true;
                    break;

                default:
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_ExpectingRequestOp"), new object[] { num.ToString(CultureInfo.CurrentCulture) }));
            }
            base.ReadContentLength(out this._bChunked, out this._contentLength);
            base.ReadToEndOfHeaders(headers);
            headers.IPAddress = ((IPEndPoint) base.NetSocket.RemoteEndPoint).Address;
            headers.ConnectionId = this._connectionId;
            return headers;
        }

        protected override void SendErrorMessageIfPossible(Exception e)
        {
            try
            {
                this.SendErrorResponse(e, true);
            }
            catch
            {
            }
        }

        public void SendErrorResponse(Exception e, bool bCloseConnection)
        {
            this.SendErrorResponse(this.GenerateFaultString(e), bCloseConnection);
        }

        public void SendErrorResponse(string e, bool bCloseConnection)
        {
            if (!this._bOneWayRequest)
            {
                ChunkedMemoryStream outputStream = new ChunkedMemoryStream(CoreChannel.BufferPool);
                base.WritePreambleAndVersion(outputStream);
                base.WriteUInt16(2, outputStream);
                base.WriteUInt16(0, outputStream);
                base.WriteInt32(0, outputStream);
                base.WriteUInt16(2, outputStream);
                base.WriteByte(3, outputStream);
                base.WriteUInt16(1, outputStream);
                base.WriteUInt16(3, outputStream);
                base.WriteByte(1, outputStream);
                base.WriteCountedString(e, outputStream);
                base.WriteUInt16(5, outputStream);
                base.WriteByte(0, outputStream);
                base.WriteUInt16(0, outputStream);
                outputStream.WriteTo(base.NetStream);
                outputStream.Close();
            }
        }

        public void SendResponse(ITransportHeaders headers, Stream contentStream)
        {
            if (!this._bOneWayRequest)
            {
                ChunkedMemoryStream outputStream = new ChunkedMemoryStream(CoreChannel.BufferPool);
                base.WritePreambleAndVersion(outputStream);
                base.WriteUInt16(2, outputStream);
                base.WriteUInt16(0, outputStream);
                base.WriteInt32((int) contentStream.Length, outputStream);
                base.WriteHeaders(headers, outputStream);
                outputStream.WriteTo(base.NetStream);
                outputStream.Close();
                StreamHelper.CopyStream(contentStream, base.NetStream);
                contentStream.Close();
            }
        }
    }
}

