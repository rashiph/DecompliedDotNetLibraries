namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;

    internal class SqlClientWrapperSmiStream : Stream
    {
        private SmiEventSink_Default _sink;
        private SmiStream _stream;

        internal SqlClientWrapperSmiStream(SmiEventSink_Default sink, SmiStream stream)
        {
            this._sink = sink;
            this._stream = stream;
        }

        public override void Flush()
        {
            this._stream.Flush(this._sink);
            this._sink.ProcessMessagesAndThrow();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = this._stream.Read(this._sink, buffer, offset, count);
            this._sink.ProcessMessagesAndThrow();
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long num = this._stream.Seek(this._sink, offset, origin);
            this._sink.ProcessMessagesAndThrow();
            return num;
        }

        public override void SetLength(long value)
        {
            this._stream.SetLength(this._sink, value);
            this._sink.ProcessMessagesAndThrow();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._stream.Write(this._sink, buffer, offset, count);
            this._sink.ProcessMessagesAndThrow();
        }

        public override bool CanRead
        {
            get
            {
                return this._stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this._stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this._stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                long length = this._stream.GetLength(this._sink);
                this._sink.ProcessMessagesAndThrow();
                return length;
            }
        }

        public override long Position
        {
            get
            {
                long position = this._stream.GetPosition(this._sink);
                this._sink.ProcessMessagesAndThrow();
                return position;
            }
            set
            {
                this._stream.SetPosition(this._sink, value);
                this._sink.ProcessMessagesAndThrow();
            }
        }
    }
}

