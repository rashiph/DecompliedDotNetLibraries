namespace System.IO
{
    using System;

    internal class ByteBufferAllocator : IByteBufferPool
    {
        private int _bufferSize;

        public ByteBufferAllocator(int bufferSize)
        {
            this._bufferSize = bufferSize;
        }

        public byte[] GetBuffer()
        {
            return new byte[this._bufferSize];
        }

        public void ReturnBuffer(byte[] buffer)
        {
        }
    }
}

