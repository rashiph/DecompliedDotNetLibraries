namespace System.Runtime.Remoting.Channels.Http
{
    using System;

    internal sealed class HttpFixedLengthReadingStream : HttpReadingStream
    {
        private int _bytesLeft;
        private HttpSocketHandler _inputStream;

        internal HttpFixedLengthReadingStream(HttpSocketHandler inputStream, int contentLength)
        {
            this._inputStream = inputStream;
            this._bytesLeft = contentLength;
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
            if (this._bytesLeft == 0)
            {
                return 0;
            }
            int num = this._inputStream.Read(buffer, offset, Math.Min(this._bytesLeft, count));
            if (num > 0)
            {
                this._bytesLeft -= num;
            }
            return num;
        }

        public override int ReadByte()
        {
            if (this._bytesLeft == 0)
            {
                return -1;
            }
            this._bytesLeft--;
            return this._inputStream.ReadByte();
        }

        public override bool FoundEnd
        {
            get
            {
                return (this._bytesLeft == 0);
            }
        }
    }
}

