namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Sockets;

    internal class DelegatedStream : Stream
    {
        private NetworkStream netStream;
        private Stream stream;

        protected DelegatedStream()
        {
        }

        protected DelegatedStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.stream = stream;
            this.netStream = stream as NetworkStream;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException(SR.GetString("ReadNotSupported"));
            }
            if (this.netStream != null)
            {
                return this.netStream.UnsafeBeginRead(buffer, offset, count, callback, state);
            }
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException(SR.GetString("WriteNotSupported"));
            }
            if (this.netStream != null)
            {
                return this.netStream.UnsafeBeginWrite(buffer, offset, count, callback, state);
            }
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            this.stream.Close();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException(SR.GetString("ReadNotSupported"));
            }
            return this.stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException(SR.GetString("WriteNotSupported"));
            }
            this.stream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException(SR.GetString("ReadNotSupported"));
            }
            return this.stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this.CanSeek)
            {
                throw new NotSupportedException(SR.GetString("SeekNotSupported"));
            }
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (!this.CanSeek)
            {
                throw new NotSupportedException(SR.GetString("SeekNotSupported"));
            }
            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException(SR.GetString("WriteNotSupported"));
            }
            this.stream.Write(buffer, offset, count);
        }

        protected Stream BaseStream
        {
            get
            {
                return this.stream;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (!this.CanSeek)
                {
                    throw new NotSupportedException(SR.GetString("SeekNotSupported"));
                }
                return this.stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                if (!this.CanSeek)
                {
                    throw new NotSupportedException(SR.GetString("SeekNotSupported"));
                }
                return this.stream.Position;
            }
            set
            {
                if (!this.CanSeek)
                {
                    throw new NotSupportedException(SR.GetString("SeekNotSupported"));
                }
                this.stream.Position = value;
            }
        }
    }
}

