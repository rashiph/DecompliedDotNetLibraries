namespace System.Web.Hosting
{
    using System;
    using System.Text;
    using System.Web;

    internal class RecyclableCharBuffer
    {
        private byte[] _byteBuffer;
        private char[] _charBuffer = ((char[]) s_CharBufferAllocator.GetBuffer());
        private int _freePos;
        private bool _recyclable;
        private int _size;
        private const int BUFFER_SIZE = 0x400;
        private const int MAX_FREE_BUFFERS = 0x40;
        private static UbyteBufferAllocator s_ByteBufferAllocator = new UbyteBufferAllocator(Encoding.UTF8.GetMaxByteCount(0x400), 0x40);
        private static CharBufferAllocator s_CharBufferAllocator = new CharBufferAllocator(0x400, 0x40);

        internal RecyclableCharBuffer()
        {
            this._size = this._charBuffer.Length;
            this._freePos = 0;
            this._recyclable = true;
        }

        internal void Append(char ch)
        {
            if (this._freePos >= this._size)
            {
                this.Grow(this._freePos + 1);
            }
            this._charBuffer[this._freePos++] = ch;
        }

        internal void Append(string s)
        {
            int length = s.Length;
            int newSize = this._freePos + length;
            if (newSize > this._size)
            {
                this.Grow(newSize);
            }
            s.CopyTo(0, this._charBuffer, this._freePos, length);
            this._freePos = newSize;
        }

        internal void Dispose()
        {
            if (this._recyclable)
            {
                if (this._charBuffer != null)
                {
                    s_CharBufferAllocator.ReuseBuffer(this._charBuffer);
                }
                if (this._byteBuffer != null)
                {
                    s_ByteBufferAllocator.ReuseBuffer(this._byteBuffer);
                }
            }
            this._charBuffer = null;
            this._byteBuffer = null;
        }

        internal byte[] GetEncodedBytesBuffer()
        {
            return this.GetEncodedBytesBuffer(Encoding.UTF8);
        }

        internal byte[] GetEncodedBytesBuffer(Encoding encoding)
        {
            if (this._byteBuffer == null)
            {
                if (encoding == null)
                {
                    encoding = Encoding.UTF8;
                }
                this.Append('\0');
                if (this._recyclable)
                {
                    this._byteBuffer = (byte[]) s_ByteBufferAllocator.GetBuffer();
                    if (this._freePos > 0)
                    {
                        encoding.GetBytes(this._charBuffer, 0, this._freePos, this._byteBuffer, 0);
                    }
                }
                else
                {
                    this._byteBuffer = encoding.GetBytes(this._charBuffer, 0, this._freePos);
                }
            }
            return this._byteBuffer;
        }

        private void Grow(int newSize)
        {
            if (newSize > this._size)
            {
                if (newSize < (this._size * 2))
                {
                    newSize = this._size * 2;
                }
                char[] destinationArray = new char[newSize];
                if (this._freePos > 0)
                {
                    Array.Copy(this._charBuffer, destinationArray, this._freePos);
                }
                this._charBuffer = destinationArray;
                this._size = newSize;
                this._recyclable = false;
            }
        }

        public override string ToString()
        {
            if ((this._charBuffer != null) && (this._freePos > 0))
            {
                return new string(this._charBuffer, 0, this._freePos);
            }
            return null;
        }
    }
}

