namespace System.Xml
{
    using System;

    internal abstract class Base64Encoder
    {
        internal const int Base64LineSize = 0x4c;
        private char[] charsLine = new char[0x4c];
        private byte[] leftOverBytes;
        private int leftOverBytesCount;
        internal const int LineSizeInBytes = 0x39;

        internal Base64Encoder()
        {
        }

        internal void Encode(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count > (buffer.Length - index))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.leftOverBytesCount > 0)
            {
                int leftOverBytesCount = this.leftOverBytesCount;
                while ((leftOverBytesCount < 3) && (count > 0))
                {
                    this.leftOverBytes[leftOverBytesCount++] = buffer[index++];
                    count--;
                }
                if ((count == 0) && (leftOverBytesCount < 3))
                {
                    this.leftOverBytesCount = leftOverBytesCount;
                    return;
                }
                int num2 = Convert.ToBase64CharArray(this.leftOverBytes, 0, 3, this.charsLine, 0);
                this.WriteChars(this.charsLine, 0, num2);
            }
            this.leftOverBytesCount = count % 3;
            if (this.leftOverBytesCount > 0)
            {
                count -= this.leftOverBytesCount;
                if (this.leftOverBytes == null)
                {
                    this.leftOverBytes = new byte[3];
                }
                for (int i = 0; i < this.leftOverBytesCount; i++)
                {
                    this.leftOverBytes[i] = buffer[(index + count) + i];
                }
            }
            int num4 = index + count;
            int length = 0x39;
            while (index < num4)
            {
                if ((index + length) > num4)
                {
                    length = num4 - index;
                }
                int num6 = Convert.ToBase64CharArray(buffer, index, length, this.charsLine, 0);
                this.WriteChars(this.charsLine, 0, num6);
                index += length;
            }
        }

        internal void Flush()
        {
            if (this.leftOverBytesCount > 0)
            {
                int count = Convert.ToBase64CharArray(this.leftOverBytes, 0, this.leftOverBytesCount, this.charsLine, 0);
                this.WriteChars(this.charsLine, 0, count);
                this.leftOverBytesCount = 0;
            }
        }

        internal abstract void WriteChars(char[] chars, int index, int count);
    }
}

