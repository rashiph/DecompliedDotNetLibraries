namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal class BufferedWrite
    {
        private byte[] buffer;
        private int offset;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal BufferedWrite() : this(0x100)
        {
        }

        internal BufferedWrite(int initialSize)
        {
            this.buffer = new byte[initialSize];
        }

        private void EnsureBuffer(int count)
        {
            int length = this.buffer.Length;
            if (count > (length - this.offset))
            {
                int num2 = length;
                do
                {
                    if (num2 == 0x7fffffff)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("WriteBufferOverflow")));
                    }
                    num2 = (num2 < 0x3fffffff) ? (num2 * 2) : 0x7fffffff;
                }
                while (count > (num2 - this.offset));
                byte[] dst = new byte[num2];
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

        internal void Write(byte[] value)
        {
            this.Write(value, 0, value.Length);
        }

        internal void Write(string value)
        {
            this.Write(value, 0, value.Length);
        }

        internal void Write(byte[] value, int index, int count)
        {
            this.EnsureBuffer(count);
            Buffer.BlockCopy(value, index, this.buffer, this.offset, count);
            this.offset += count;
        }

        internal void Write(string value, int index, int count)
        {
            this.EnsureBuffer(count);
            for (int i = 0; i < count; i++)
            {
                char ch = value[index + i];
                if (ch > '\x00ff')
                {
                    object[] args = new object[] { ch, ((int) ch).ToString("X", CultureInfo.InvariantCulture) };
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeHeaderInvalidCharacter", args)));
                }
                this.buffer[this.offset + i] = (byte) ch;
            }
            this.offset += count;
        }

        internal int Length
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.offset;
            }
        }
    }
}

