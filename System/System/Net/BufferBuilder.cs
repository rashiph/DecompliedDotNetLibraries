namespace System.Net
{
    using System;

    internal class BufferBuilder
    {
        private byte[] buffer;
        private int offset;

        internal BufferBuilder() : this(0x100)
        {
        }

        internal BufferBuilder(int initialSize)
        {
            this.buffer = new byte[initialSize];
        }

        internal void Append(byte value)
        {
            this.EnsureBuffer(1);
            this.buffer[this.offset++] = value;
        }

        internal void Append(byte[] value)
        {
            this.Append(value, 0, value.Length);
        }

        internal void Append(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                this.Append(value, 0, value.Length);
            }
        }

        internal void Append(byte[] value, int offset, int count)
        {
            this.EnsureBuffer(count);
            Buffer.BlockCopy(value, offset, this.buffer, this.offset, count);
            this.offset += count;
        }

        internal void Append(string value, int offset, int count)
        {
            this.EnsureBuffer(count);
            for (int i = 0; i < count; i++)
            {
                char ch = value[offset + i];
                if (ch > '\x00ff')
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter", new object[] { ch }));
                }
                this.buffer[this.offset + i] = (byte) ch;
            }
            this.offset += count;
        }

        private void EnsureBuffer(int count)
        {
            if (count > (this.buffer.Length - this.offset))
            {
                byte[] dst = new byte[((this.buffer.Length * 2) > (this.buffer.Length + count)) ? (this.buffer.Length * 2) : (this.buffer.Length + count)];
                Buffer.BlockCopy(this.buffer, 0, dst, 0, this.offset);
                this.buffer = dst;
            }
        }

        internal byte[] GetBuffer()
        {
            return this.buffer;
        }

        internal void Reset()
        {
            this.offset = 0;
        }

        internal int Length
        {
            get
            {
                return this.offset;
            }
        }
    }
}

