namespace System.Xml
{
    using System;

    internal class MtomBinaryData
    {
        internal byte[] chunk;
        internal IStreamProvider provider;
        internal MtomBinaryDataType type;

        internal MtomBinaryData(IStreamProvider provider)
        {
            this.type = MtomBinaryDataType.Provider;
            this.provider = provider;
        }

        internal MtomBinaryData(byte[] buffer, int offset, int count)
        {
            this.type = MtomBinaryDataType.Segment;
            this.chunk = new byte[count];
            Buffer.BlockCopy(buffer, offset, this.chunk, 0, count);
        }

        internal long Length
        {
            get
            {
                if (this.type == MtomBinaryDataType.Segment)
                {
                    return (long) this.chunk.Length;
                }
                return -1L;
            }
        }
    }
}

