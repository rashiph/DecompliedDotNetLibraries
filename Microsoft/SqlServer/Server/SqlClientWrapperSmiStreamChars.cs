namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.IO;

    internal class SqlClientWrapperSmiStreamChars : SqlStreamChars
    {
        private SmiEventSink_Default _sink;
        private SmiStream _stream;

        internal SqlClientWrapperSmiStreamChars(SmiEventSink_Default sink, SmiStream stream)
        {
            this._sink = sink;
            this._stream = stream;
        }

        public override void Flush()
        {
            this._stream.Flush(this._sink);
            this._sink.ProcessMessagesAndThrow();
        }

        internal int Read(byte[] buffer, int offset, int count)
        {
            int num = this._stream.Read(this._sink, buffer, offset, count);
            this._sink.ProcessMessagesAndThrow();
            return num;
        }

        public override int Read(char[] buffer, int offset, int count)
        {
            int num = this._stream.Read(this._sink, buffer, offset * 2, count);
            this._sink.ProcessMessagesAndThrow();
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long num = this._stream.Seek(this._sink, offset * 2L, origin);
            this._sink.ProcessMessagesAndThrow();
            return num;
        }

        public override void SetLength(long value)
        {
            if (value < 0L)
            {
                throw ADP.ArgumentOutOfRange("value");
            }
            this._stream.SetLength(this._sink, value * 2L);
            this._sink.ProcessMessagesAndThrow();
        }

        internal void Write(byte[] buffer, int offset, int count)
        {
            this._stream.Write(this._sink, buffer, offset, count);
            this._sink.ProcessMessagesAndThrow();
        }

        public override void Write(char[] buffer, int offset, int count)
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

        public override bool IsNull
        {
            get
            {
                return (null == this._stream);
            }
        }

        public override long Length
        {
            get
            {
                long length = this._stream.GetLength(this._sink);
                this._sink.ProcessMessagesAndThrow();
                if (length > 0L)
                {
                    return (length / 2L);
                }
                return length;
            }
        }

        public override long Position
        {
            get
            {
                long num = this._stream.GetPosition(this._sink) / 2L;
                this._sink.ProcessMessagesAndThrow();
                return num;
            }
            set
            {
                if (value < 0L)
                {
                    throw ADP.ArgumentOutOfRange("Position");
                }
                this._stream.SetPosition(this._sink, value * 2L);
                this._sink.ProcessMessagesAndThrow();
            }
        }
    }
}

