namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;

    internal sealed class HttpChunkedReadingStream : HttpReadingStream
    {
        private bool _bFoundEnd;
        private byte[] _byteBuffer = new byte[1];
        private int _bytesLeft;
        private static byte[] _endChunk = Encoding.ASCII.GetBytes("\r\n");
        private HttpSocketHandler _inputStream;
        private static byte[] _trailer = Encoding.ASCII.GetBytes("0\r\n\r\n\r\n");

        internal HttpChunkedReadingStream(HttpSocketHandler inputStream)
        {
            this._inputStream = inputStream;
            this._bytesLeft = 0;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = 0;
            while (!this._bFoundEnd && (count > 0))
            {
                if (this._bytesLeft == 0)
                {
                    while (true)
                    {
                        byte b = (byte) this._inputStream.ReadByte();
                        if (b == 13)
                        {
                            if (((ushort) this._inputStream.ReadByte()) != 10)
                            {
                                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_ChunkedEncodingError"));
                            }
                            break;
                        }
                        int num3 = HttpChannelHelper.CharacterHexDigitToDecimal(b);
                        if ((num3 < 0) || (num3 > 15))
                        {
                            throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_ChunkedEncodingError"));
                        }
                        this._bytesLeft = (this._bytesLeft * 0x10) + num3;
                    }
                    if (this._bytesLeft == 0)
                    {
                        while (this._inputStream.ReadToEndOfLine().Length != 0)
                        {
                        }
                        this._bFoundEnd = true;
                    }
                }
                if (!this._bFoundEnd)
                {
                    int num4 = Math.Min(this._bytesLeft, count);
                    int num5 = this._inputStream.Read(buffer, offset, num4);
                    if (num5 <= 0)
                    {
                        throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_ChunkedEncodingError"));
                    }
                    this._bytesLeft -= num5;
                    count -= num5;
                    offset += num5;
                    num += num5;
                    if (this._bytesLeft != 0)
                    {
                        continue;
                    }
                    char ch = (char) this._inputStream.ReadByte();
                    if (ch != '\r')
                    {
                        throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_ChunkedEncodingError"));
                    }
                    ch = (char) this._inputStream.ReadByte();
                    if (ch != '\n')
                    {
                        throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_ChunkedEncodingError"));
                    }
                }
            }
            return num;
        }

        public override int ReadByte()
        {
            if (this.Read(this._byteBuffer, 0, 1) == 0)
            {
                return -1;
            }
            return this._byteBuffer[0];
        }

        public override bool FoundEnd
        {
            get
            {
                return this._bFoundEnd;
            }
        }
    }
}

