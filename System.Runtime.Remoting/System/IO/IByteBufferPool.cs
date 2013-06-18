namespace System.IO
{
    using System;

    internal interface IByteBufferPool
    {
        byte[] GetBuffer();
        void ReturnBuffer(byte[] buffer);
    }
}

