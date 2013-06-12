namespace System.Net
{
    using System;

    internal class BufferOffsetSize
    {
        internal byte[] Buffer;
        internal int Offset;
        internal int Size;

        internal BufferOffsetSize(byte[] buffer, bool copyBuffer) : this(buffer, 0, buffer.Length, copyBuffer)
        {
        }

        internal BufferOffsetSize(byte[] buffer, int offset, int size, bool copyBuffer)
        {
            if (copyBuffer)
            {
                byte[] dst = new byte[size];
                System.Buffer.BlockCopy(buffer, offset, dst, 0, size);
                offset = 0;
                buffer = dst;
            }
            this.Buffer = buffer;
            this.Offset = offset;
            this.Size = size;
        }
    }
}

