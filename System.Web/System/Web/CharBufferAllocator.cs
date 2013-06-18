namespace System.Web
{
    using System;

    internal class CharBufferAllocator : BufferAllocator
    {
        private int _bufferSize;

        internal CharBufferAllocator(int bufferSize, int maxFree) : base(maxFree)
        {
            this._bufferSize = bufferSize;
        }

        protected override object AllocBuffer()
        {
            return new char[this._bufferSize];
        }
    }
}

