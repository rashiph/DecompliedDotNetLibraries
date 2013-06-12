namespace System.IO.Compression
{
    using System;
    using System.Runtime.InteropServices;

    internal class OutputBuffer
    {
        private uint bitBuf;
        private int bitCount;
        private byte[] byteBuffer;
        private int pos;

        internal BufferState DumpState()
        {
            BufferState state;
            state.pos = this.pos;
            state.bitBuf = this.bitBuf;
            state.bitCount = this.bitCount;
            return state;
        }

        internal void FlushBits()
        {
            while (this.bitCount >= 8)
            {
                this.byteBuffer[this.pos++] = (byte) this.bitBuf;
                this.bitCount -= 8;
                this.bitBuf = this.bitBuf >> 8;
            }
            if (this.bitCount > 0)
            {
                this.byteBuffer[this.pos++] = (byte) this.bitBuf;
                this.bitBuf = 0;
                this.bitCount = 0;
            }
        }

        internal void RestoreState(BufferState state)
        {
            this.pos = state.pos;
            this.bitBuf = state.bitBuf;
            this.bitCount = state.bitCount;
        }

        internal void UpdateBuffer(byte[] output)
        {
            this.byteBuffer = output;
            this.pos = 0;
        }

        internal void WriteBits(int n, uint bits)
        {
            this.bitBuf |= bits << this.bitCount;
            this.bitCount += n;
            if (this.bitCount >= 0x10)
            {
                this.byteBuffer[this.pos++] = (byte) this.bitBuf;
                this.byteBuffer[this.pos++] = (byte) (this.bitBuf >> 8);
                this.bitCount -= 0x10;
                this.bitBuf = this.bitBuf >> 0x10;
            }
        }

        internal void WriteBytes(byte[] byteArray, int offset, int count)
        {
            if (this.bitCount == 0)
            {
                Array.Copy(byteArray, offset, this.byteBuffer, this.pos, count);
                this.pos += count;
            }
            else
            {
                this.WriteBytesUnaligned(byteArray, offset, count);
            }
        }

        private void WriteBytesUnaligned(byte[] byteArray, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                byte b = byteArray[offset + i];
                this.WriteByteUnaligned(b);
            }
        }

        private void WriteByteUnaligned(byte b)
        {
            this.WriteBits(8, b);
        }

        internal void WriteUInt16(ushort value)
        {
            this.byteBuffer[this.pos++] = (byte) value;
            this.byteBuffer[this.pos++] = (byte) (value >> 8);
        }

        internal int BitsInBuffer
        {
            get
            {
                return ((this.bitCount / 8) + 1);
            }
        }

        internal int BytesWritten
        {
            get
            {
                return this.pos;
            }
        }

        internal int FreeBytes
        {
            get
            {
                return (this.byteBuffer.Length - this.pos);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BufferState
        {
            internal int pos;
            internal uint bitBuf;
            internal int bitCount;
        }
    }
}

