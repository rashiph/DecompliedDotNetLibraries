namespace System.Net
{
    using System;

    internal class NestedMultipleAsyncResult : LazyAsyncResult
    {
        internal BufferOffsetSize[] Buffers;
        internal int Size;

        internal NestedMultipleAsyncResult(object asyncObject, object asyncState, AsyncCallback asyncCallback, BufferOffsetSize[] buffers) : base(asyncObject, asyncState, asyncCallback)
        {
            this.Buffers = buffers;
            this.Size = 0;
            for (int i = 0; i < this.Buffers.Length; i++)
            {
                this.Size += this.Buffers[i].Size;
            }
        }
    }
}

