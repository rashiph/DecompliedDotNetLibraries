namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class UTF16Decoder : System.Text.Decoder
    {
        private bool bigEndian;
        private const int CharSize = 2;
        private int lastByte = -1;

        public UTF16Decoder(bool bigEndian)
        {
            this.bigEndian = bigEndian;
        }

        public override void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            charsUsed = 0;
            bytesUsed = 0;
            if (this.lastByte >= 0)
            {
                if (byteCount == 0)
                {
                    completed = true;
                    return;
                }
                int num = bytes[byteIndex++];
                byteCount--;
                bytesUsed++;
                chars[charIndex++] = this.bigEndian ? ((char) ((this.lastByte << 8) | num)) : ((char) ((num << 8) | this.lastByte));
                charCount--;
                charsUsed++;
                this.lastByte = -1;
            }
            if ((charCount * 2) < byteCount)
            {
                byteCount = charCount * 2;
                completed = false;
            }
            else
            {
                completed = true;
            }
            if (this.bigEndian != BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(bytes, byteIndex, chars, charIndex * 2, byteCount & -2);
            }
            else
            {
                int num2 = byteIndex;
                int num3 = num2 + (byteCount & -2);
                if (!this.bigEndian)
                {
                    while (num2 < num3)
                    {
                        int num6 = bytes[num2++];
                        int num7 = bytes[num2++];
                        chars[charIndex++] = (char) ((num7 << 8) | num6);
                    }
                }
                else
                {
                    while (num2 < num3)
                    {
                        int num4 = bytes[num2++];
                        int num5 = bytes[num2++];
                        chars[charIndex++] = (char) ((num4 << 8) | num5);
                    }
                }
            }
            charsUsed += byteCount / 2;
            bytesUsed += byteCount;
            if ((byteCount & 1) != 0)
            {
                this.lastByte = bytes[(byteIndex + byteCount) - 1];
            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return this.GetCharCount(bytes, index, count, false);
        }

        public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            int num = count + ((this.lastByte >= 0) ? 1 : 0);
            if (flush && ((num % 2) != 0))
            {
                throw new ArgumentException(Res.GetString("Enc_InvalidByteInEncoding", new object[] { -1 }), null);
            }
            return (num / 2);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int num = this.GetCharCount(bytes, byteIndex, byteCount);
            if (this.lastByte >= 0)
            {
                if (byteCount == 0)
                {
                    return num;
                }
                int num2 = bytes[byteIndex++];
                byteCount--;
                chars[charIndex++] = this.bigEndian ? ((char) ((this.lastByte << 8) | num2)) : ((char) ((num2 << 8) | this.lastByte));
                this.lastByte = -1;
            }
            if ((byteCount & 1) != 0)
            {
                this.lastByte = bytes[byteIndex + --byteCount];
            }
            if (this.bigEndian != BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(bytes, byteIndex, chars, charIndex * 2, byteCount);
                return num;
            }
            int num3 = byteIndex + byteCount;
            if (!this.bigEndian)
            {
                while (byteIndex < num3)
                {
                    int num6 = bytes[byteIndex++];
                    int num7 = bytes[byteIndex++];
                    chars[charIndex++] = (char) ((num7 << 8) | num6);
                }
                return num;
            }
            while (byteIndex < num3)
            {
                int num4 = bytes[byteIndex++];
                int num5 = bytes[byteIndex++];
                chars[charIndex++] = (char) ((num4 << 8) | num5);
            }
            return num;
        }
    }
}

