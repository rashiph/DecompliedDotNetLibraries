namespace System.IO.Compression
{
    using System;

    internal class GZipFormatter : IFileFormatWriter
    {
        private uint _crc32;
        private long _inputStreamSize;
        private byte[] headerBytes;

        internal GZipFormatter() : this(3)
        {
        }

        internal GZipFormatter(int compressionLevel)
        {
            this.headerBytes = new byte[] { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 4, 0 };
            if (compressionLevel == 10)
            {
                this.headerBytes[8] = 2;
            }
        }

        public byte[] GetFooter()
        {
            byte[] b = new byte[8];
            this.WriteUInt32(b, this._crc32, 0);
            this.WriteUInt32(b, (uint) this._inputStreamSize, 4);
            return b;
        }

        public byte[] GetHeader()
        {
            return this.headerBytes;
        }

        public void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy)
        {
            this._crc32 = Crc32Helper.UpdateCrc32(this._crc32, buffer, offset, bytesToCopy);
            long a = this._inputStreamSize + ((long) ((ulong) bytesToCopy));
            if (a > 0x100000000L)
            {
                Math.DivRem(a, 0x100000000L, out a);
            }
            this._inputStreamSize = a;
        }

        internal void WriteUInt32(byte[] b, uint value, int startIndex)
        {
            b[startIndex] = (byte) value;
            b[startIndex + 1] = (byte) (value >> 8);
            b[startIndex + 2] = (byte) (value >> 0x10);
            b[startIndex + 3] = (byte) (value >> 0x18);
        }
    }
}

