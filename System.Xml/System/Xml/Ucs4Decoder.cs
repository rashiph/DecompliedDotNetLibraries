namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class Ucs4Decoder : System.Text.Decoder
    {
        internal byte[] lastBytes = new byte[4];
        internal int lastBytesCount;

        protected Ucs4Decoder()
        {
        }

        public override void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            bytesUsed = 0;
            charsUsed = 0;
            int num = 0;
            int lastBytesCount = this.lastBytesCount;
            if (lastBytesCount > 0)
            {
                while ((lastBytesCount < 4) && (byteCount > 0))
                {
                    this.lastBytes[lastBytesCount] = bytes[byteIndex];
                    byteIndex++;
                    byteCount--;
                    bytesUsed++;
                    lastBytesCount++;
                }
                if (lastBytesCount < 4)
                {
                    this.lastBytesCount = lastBytesCount;
                    completed = true;
                    return;
                }
                num = this.GetFullChars(this.lastBytes, 0, 4, chars, charIndex);
                charIndex += num;
                charCount -= num;
                charsUsed = num;
                this.lastBytesCount = 0;
                if (charCount == 0)
                {
                    completed = byteCount == 0;
                    return;
                }
            }
            else
            {
                num = 0;
            }
            if ((charCount * 4) < byteCount)
            {
                byteCount = charCount * 4;
                completed = false;
            }
            else
            {
                completed = true;
            }
            bytesUsed += byteCount;
            charsUsed = this.GetFullChars(bytes, byteIndex, byteCount, chars, charIndex) + num;
            int num3 = byteCount & 3;
            if (num3 >= 0)
            {
                for (int i = 0; i < num3; i++)
                {
                    this.lastBytes[i] = bytes[((byteIndex + byteCount) - num3) + i];
                }
                this.lastBytesCount = num3;
            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return ((count + this.lastBytesCount) / 4);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int lastBytesCount = this.lastBytesCount;
            if (this.lastBytesCount > 0)
            {
                while ((this.lastBytesCount < 4) && (byteCount > 0))
                {
                    this.lastBytes[this.lastBytesCount] = bytes[byteIndex];
                    byteIndex++;
                    byteCount--;
                    this.lastBytesCount++;
                }
                if (this.lastBytesCount < 4)
                {
                    return 0;
                }
                lastBytesCount = this.GetFullChars(this.lastBytes, 0, 4, chars, charIndex);
                charIndex += lastBytesCount;
                this.lastBytesCount = 0;
            }
            else
            {
                lastBytesCount = 0;
            }
            lastBytesCount = this.GetFullChars(bytes, byteIndex, byteCount, chars, charIndex) + lastBytesCount;
            int num2 = byteCount & 3;
            if (num2 >= 0)
            {
                for (int i = 0; i < num2; i++)
                {
                    this.lastBytes[i] = bytes[((byteIndex + byteCount) - num2) + i];
                }
                this.lastBytesCount = num2;
            }
            return lastBytesCount;
        }

        internal abstract int GetFullChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);
        internal void Ucs4ToUTF16(uint code, char[] chars, int charIndex)
        {
            chars[charIndex] = (char) ((0xd800 + ((ushort) ((code >> 0x10) - 1))) + ((ushort) ((code >> 10) & 0x3f)));
            chars[charIndex + 1] = (char) (0xdc00 + ((ushort) (code & 0x3ff)));
        }
    }
}

