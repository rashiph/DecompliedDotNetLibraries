namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BufferChunkBytes : IReadChunkBytes
    {
        public byte[] Buffer;
        public int Offset;
        public int Count;
        public int NextByte
        {
            get
            {
                if (this.Count != 0)
                {
                    this.Count--;
                    return this.Buffer[this.Offset++];
                }
                return -1;
            }
            set
            {
                this.Count++;
                this.Offset--;
                this.Buffer[this.Offset] = (byte) value;
            }
        }
    }
}

