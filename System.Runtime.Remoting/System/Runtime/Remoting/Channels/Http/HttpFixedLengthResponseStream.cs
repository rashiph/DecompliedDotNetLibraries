namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.IO;

    internal sealed class HttpFixedLengthResponseStream : HttpServerResponseStream
    {
        private static int _length;
        private Stream _outputStream;

        internal HttpFixedLengthResponseStream(Stream outputStream, int length)
        {
            this._outputStream = outputStream;
            _length = length;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this._outputStream.Flush();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            this._outputStream.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._outputStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this._outputStream.WriteByte(value);
        }
    }
}

