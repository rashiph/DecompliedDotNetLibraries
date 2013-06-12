namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;

    internal abstract class SmiStream
    {
        protected SmiStream()
        {
        }

        public abstract void Flush(SmiEventSink sink);
        public abstract long GetLength(SmiEventSink sink);
        public abstract long GetPosition(SmiEventSink sink);
        public abstract int Read(SmiEventSink sink, byte[] buffer, int offset, int count);
        public abstract int Read(SmiEventSink sink, char[] buffer, int offset, int count);
        public abstract long Seek(SmiEventSink sink, long offset, SeekOrigin origin);
        public abstract void SetLength(SmiEventSink sink, long value);
        public abstract void SetPosition(SmiEventSink sink, long position);
        public abstract void Write(SmiEventSink sink, byte[] buffer, int offset, int count);
        public abstract void Write(SmiEventSink sink, char[] buffer, int offset, int count);

        public abstract bool CanRead { get; }

        public abstract bool CanSeek { get; }

        public abstract bool CanWrite { get; }
    }
}

