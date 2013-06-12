namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;

    internal class Base64Decoder : IncrementalReadDecoder
    {
        private int bits;
        private int bitsFilled;
        private byte[] buffer;
        private static readonly string CharsBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private int curIndex;
        private int endIndex;
        private const byte Invalid = 0xff;
        private static readonly byte[] MapBase64 = ConstructMapBase64();
        private const int MaxValidChar = 0x7a;
        private int startIndex;

        private static byte[] ConstructMapBase64()
        {
            byte[] buffer = new byte[0x7b];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0xff;
            }
            for (int j = 0; j < CharsBase64.Length; j++)
            {
                buffer[CharsBase64[j]] = (byte) j;
            }
            return buffer;
        }

        internal override unsafe int Decode(char[] chars, int startPos, int len)
        {
            int num;
            int num2;
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (len < 0)
            {
                throw new ArgumentOutOfRangeException("len");
            }
            if (startPos < 0)
            {
                throw new ArgumentOutOfRangeException("startPos");
            }
            if ((chars.Length - startPos) < len)
            {
                throw new ArgumentOutOfRangeException("len");
            }
            if (len == 0)
            {
                return 0;
            }
            fixed (char* chRef = &(chars[startPos]))
            {
                fixed (byte* numRef = &(this.buffer[this.curIndex]))
                {
                    this.Decode(chRef, chRef + len, numRef, numRef + (this.endIndex - this.curIndex), out num2, out num);
                }
            }
            this.curIndex += num;
            return num2;
        }

        internal override unsafe int Decode(string str, int startPos, int len)
        {
            int num;
            int num2;
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (len < 0)
            {
                throw new ArgumentOutOfRangeException("len");
            }
            if (startPos < 0)
            {
                throw new ArgumentOutOfRangeException("startPos");
            }
            if ((str.Length - startPos) < len)
            {
                throw new ArgumentOutOfRangeException("len");
            }
            if (len == 0)
            {
                return 0;
            }
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                fixed (byte* numRef = &(this.buffer[this.curIndex]))
                {
                    this.Decode(chPtr + startPos, (chPtr + startPos) + len, numRef, numRef + (this.endIndex - this.curIndex), out num2, out num);
                }
            }
            this.curIndex += num;
            return num2;
        }

        private unsafe void Decode(char* pChars, char* pCharsEndPos, byte* pBytes, byte* pBytesEndPos, out int charsDecoded, out int bytesDecoded)
        {
            byte* numPtr = pBytes;
            char* chPtr = pChars;
            int bits = this.bits;
            int bitsFilled = this.bitsFilled;
            XmlCharType instance = XmlCharType.Instance;
            while ((chPtr < pCharsEndPos) && (numPtr < pBytesEndPos))
            {
                char index = chPtr[0];
                if (index == '=')
                {
                    break;
                }
                chPtr++;
                if ((instance.charProperties[index] & 1) == 0)
                {
                    int num3;
                    if ((index > 'z') || ((num3 = MapBase64[index]) == 0xff))
                    {
                        throw new XmlException("Xml_InvalidBase64Value", new string(pChars, 0, (int) ((long) ((pCharsEndPos - pChars) / 2))));
                    }
                    bits = (bits << 6) | num3;
                    bitsFilled += 6;
                    if (bitsFilled >= 8)
                    {
                        numPtr++;
                        numPtr[0] = (byte) ((bits >> (bitsFilled - 8)) & 0xff);
                        bitsFilled -= 8;
                        if (numPtr == pBytesEndPos)
                        {
                            goto Label_00F4;
                        }
                    }
                }
            }
            if ((chPtr < pCharsEndPos) && (chPtr[0] == '='))
            {
                bitsFilled = 0;
                do
                {
                    chPtr++;
                }
                while ((chPtr < pCharsEndPos) && (chPtr[0] == '='));
                if (chPtr < pCharsEndPos)
                {
                    do
                    {
                        chPtr++;
                        if ((instance.charProperties[chPtr[0]] & 1) == 0)
                        {
                            throw new XmlException("Xml_InvalidBase64Value", new string(pChars, 0, (int) ((long) ((pCharsEndPos - pChars) / 2))));
                        }
                    }
                    while (chPtr < pCharsEndPos);
                }
            }
        Label_00F4:
            this.bits = bits;
            this.bitsFilled = bitsFilled;
            bytesDecoded = (int) ((long) ((numPtr - pBytes) / 1));
            charsDecoded = (int) ((long) ((chPtr - pChars) / 2));
        }

        internal override void Reset()
        {
            this.bitsFilled = 0;
            this.bits = 0;
        }

        internal override void SetNextOutputBuffer(Array buffer, int index, int count)
        {
            this.buffer = (byte[]) buffer;
            this.startIndex = index;
            this.curIndex = index;
            this.endIndex = index + count;
        }

        internal override int DecodedCount
        {
            get
            {
                return (this.curIndex - this.startIndex);
            }
        }

        internal override bool IsFull
        {
            get
            {
                return (this.curIndex == this.endIndex);
            }
        }
    }
}

