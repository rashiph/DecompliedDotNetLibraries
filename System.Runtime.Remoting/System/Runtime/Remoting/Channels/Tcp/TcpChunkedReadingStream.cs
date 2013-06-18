namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;

    internal sealed class TcpChunkedReadingStream : TcpReadingStream
    {
        private bool _bFoundEnd;
        private byte[] _byteBuffer = new byte[1];
        private int _bytesLeft;
        private SocketHandler _inputStream;

        internal TcpChunkedReadingStream(SocketHandler inputStream)
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
                    this._bytesLeft = this._inputStream.ReadInt32();
                    if (this._bytesLeft == 0)
                    {
                        this.ReadTrailer();
                        this._bFoundEnd = true;
                    }
                }
                if (!this._bFoundEnd)
                {
                    int num2 = Math.Min(this._bytesLeft, count);
                    int num3 = this._inputStream.Read(buffer, offset, num2);
                    if (num3 <= 0)
                    {
                        throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_ChunkedEncodingError"));
                    }
                    this._bytesLeft -= num3;
                    count -= num3;
                    offset += num3;
                    num += num3;
                    if (this._bytesLeft == 0)
                    {
                        this.ReadTrailer();
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

        private void ReadTrailer()
        {
            if (this._inputStream.ReadByte() != 13)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_ChunkedEncodingError"));
            }
            if (this._inputStream.ReadByte() != 10)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_ChunkedEncodingError"));
            }
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

