namespace System.Web
{
    using System;

    internal class UbyteBufferAllocator : BufferAllocator
    {
        private int _bufferSize;

        internal UbyteBufferAllocator(int bufferSize, int maxFree) : base(maxFree)
        {
            this._bufferSize = bufferSize;
        }

        protected override object AllocBuffer()
        {
            return new byte[this._bufferSize];
        }
    }
}

