namespace System.Net
{
    using System;

    internal class BufferAsyncResult : LazyAsyncResult
    {
        public byte[] Buffer;
        public BufferOffsetSize[] Buffers;
        public int Count;
        public bool IsWrite;
        public int Offset;

        public BufferAsyncResult(object asyncObject, BufferOffsetSize[] buffers, object asyncState, AsyncCallback asyncCallback) : base(asyncObject, asyncState, asyncCallback)
        {
            this.Buffers = buffers;
            this.IsWrite = true;
        }

        public BufferAsyncResult(object asyncObject, byte[] buffer, int offset, int count, object asyncState, AsyncCallback asyncCallback) : this(asyncObject, buffer, offset, count, false, asyncState, asyncCallback)
        {
        }

        public BufferAsyncResult(object asyncObject, byte[] buffer, int offset, int count, bool isWrite, object asyncState, AsyncCallback asyncCallback) : base(asyncObject, asyncState, asyncCallback)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
            this.IsWrite = isWrite;
        }
    }
}

