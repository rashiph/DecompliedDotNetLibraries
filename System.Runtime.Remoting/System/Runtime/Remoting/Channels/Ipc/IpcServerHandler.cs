namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;

    internal class IpcServerHandler : TcpSocketHandler
    {
        private bool _bOneWayRequest;
        private int _contentLength;
        protected IpcPort _port;
        private RequestQueue _requestQueue;
        protected Stream _requestStream;
        private Stream _stream;

        internal IpcServerHandler(IpcPort port, RequestQueue requestQueue, Stream stream) : base(null, requestQueue, stream)
        {
            this._requestQueue = requestQueue;
            this._port = port;
            this._stream = stream;
        }

        internal Stream GetRequestStream()
        {
            this._requestStream = new TcpFixedLengthReadingStream(this, this._contentLength);
            return this._requestStream;
        }

        protected override void PrepareForNewMessage()
        {
        }

        private void ReadAndVerifyHeaderFormat(string headerName, byte expectedFormat)
        {
            byte num = (byte) base.ReadByte();
            if (num != expectedFormat)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_IncorrectHeaderFormat"), new object[] { expectedFormat, headerName }));
            }
        }

        internal ITransportHeaders ReadHeaders()
        {
            ushort num;
            BaseTransportHeaders headers = new BaseTransportHeaders();
            base.ReadVersionAndOperation(out num);
            if (num == 1)
            {
                this._bOneWayRequest = true;
            }
            bool chunked = false;
            base.ReadContentLength(out chunked, out this._contentLength);
            this.ReadToEndOfHeaders(headers);
            return headers;
        }

        protected void ReadToEndOfHeaders(BaseTransportHeaders headers)
        {
            bool flag = false;
            string str = null;
            for (ushort i = base.ReadUInt16(); i != 0; i = base.ReadUInt16())
            {
                switch (i)
                {
                    case 1:
                    {
                        string str2 = base.ReadCountedString();
                        string str3 = base.ReadCountedString();
                        headers[str2] = str3;
                        break;
                    }
                    case 4:
                    {
                        string str6;
                        this.ReadAndVerifyHeaderFormat("RequestUri", 1);
                        string url = base.ReadCountedString();
                        if (IpcChannelHelper.ParseURL(url, out str6) == null)
                        {
                            str6 = url;
                        }
                        headers.RequestUri = str6;
                        break;
                    }
                    case 2:
                        this.ReadAndVerifyHeaderFormat("StatusCode", 3);
                        if (base.ReadUInt16() != 0)
                        {
                            flag = true;
                        }
                        break;

                    case 3:
                        this.ReadAndVerifyHeaderFormat("StatusPhrase", 1);
                        str = base.ReadCountedString();
                        break;

                    case 6:
                    {
                        this.ReadAndVerifyHeaderFormat("Content-Type", 1);
                        string str7 = base.ReadCountedString();
                        headers.ContentType = str7;
                        break;
                    }
                    default:
                    {
                        byte num3 = (byte) base.ReadByte();
                        switch (num3)
                        {
                            case 1:
                            {
                                base.ReadCountedString();
                                continue;
                            }
                            case 2:
                            {
                                base.ReadByte();
                                continue;
                            }
                            case 3:
                            {
                                base.ReadUInt16();
                                continue;
                            }
                            case 4:
                            {
                                base.ReadInt32();
                                continue;
                            }
                        }
                        throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_UnknownHeaderType"), new object[] { i, num3 }));
                    }
                }
            }
            if (flag)
            {
                if (str == null)
                {
                    str = "";
                }
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_GenericServerError"), new object[] { str }));
            }
        }

        protected override void SendErrorMessageIfPossible(Exception e)
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
                base.WriteCountedString(e.ToString(), outputStream);
                base.WriteUInt16(5, outputStream);
                base.WriteByte(0, outputStream);
                base.WriteUInt16(0, outputStream);
                outputStream.WriteTo(base.NetStream);
                outputStream.Close();
            }
        }

        internal void SendResponse(ITransportHeaders headers, Stream contentStream)
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

        internal IpcPort Port
        {
            get
            {
                return this._port;
            }
        }
    }
}

