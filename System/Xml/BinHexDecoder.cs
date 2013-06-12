namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;

    internal class BinHexDecoder : IncrementalReadDecoder
    {
        private byte[] buffer;
        private byte cachedHalfByte;
        private int curIndex;
        private int endIndex;
        private bool hasHalfByteCached;
        private int startIndex;

        public static unsafe byte[] Decode(char[] chars, bool allowOddChars)
        {
            int num2;
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            int length = chars.Length;
            if (length == 0)
            {
                return new byte[0];
            }
            byte[] sourceArray = new byte[(length + 1) / 2];
            bool hasHalfByteCached = false;
            byte cachedHalfByte = 0;
            fixed (char* chRef = chars)
            {
                fixed (byte* numRef = sourceArray)
                {
                    int num3;
                    Decode(chRef, chRef + length, numRef, numRef + sourceArray.Length, ref hasHalfByteCached, ref cachedHalfByte, out num3, out num2);
                }
            }
            if (hasHalfByteCached && !allowOddChars)
            {
                throw new XmlException("Xml_InvalidBinHexValueOddCount", new string(chars));
            }
            if (num2 < sourceArray.Length)
            {
                byte[] destinationArray = new byte[num2];
                Array.Copy(sourceArray, 0, destinationArray, 0, num2);
                sourceArray = destinationArray;
            }
            return sourceArray;
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
                    Decode(chRef, chRef + len, numRef, numRef + (this.endIndex - this.curIndex), ref this.hasHalfByteCached, ref this.cachedHalfByte, out num2, out num);
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
                    Decode(chPtr + startPos, (chPtr + startPos) + len, numRef, numRef + (this.endIndex - this.curIndex), ref this.hasHalfByteCached, ref this.cachedHalfByte, out num2, out num);
                }
            }
            this.curIndex += num;
            return num2;
        }

        private static unsafe void Decode(char* pChars, char* pCharsEndPos, byte* pBytes, byte* pBytesEndPos, ref bool hasHalfByteCached, ref byte cachedHalfByte, out int charsDecoded, out int bytesDecoded)
        {
            char* chPtr = pChars;
            byte* numPtr = pBytes;
            XmlCharType instance = XmlCharType.Instance;
            while ((chPtr < pCharsEndPos) && (numPtr < pBytesEndPos))
            {
                byte num;
                chPtr++;
                char index = chPtr[0];
                if ((index >= 'a') && (index <= 'f'))
                {
                    num = (byte) ((index - 'a') + 10);
                }
                else if ((index >= 'A') && (index <= 'F'))
                {
                    num = (byte) ((index - 'A') + 10);
                }
                else if ((index >= '0') && (index <= '9'))
                {
                    num = (byte) (index - '0');
                }
                else
                {
                    if ((instance.charProperties[index] & 1) == 0)
                    {
                        throw new XmlException("Xml_InvalidBinHexValue", new string(pChars, 0, (int) ((long) ((pCharsEndPos - pChars) / 2))));
                    }
                    continue;
                }
                if (hasHalfByteCached)
                {
                    numPtr++;
                    numPtr[0] = (byte) ((cachedHalfByte << 4) + num);
                    hasHalfByteCached = false;
                }
                else
                {
                    cachedHalfByte = num;
                    hasHalfByteCached = true;
                }
            }
            bytesDecoded = (int) ((long) ((numPtr - pBytes) / 1));
            charsDecoded = (int) ((long) ((chPtr - pChars) / 2));
        }

        internal override void Reset()
        {
            this.hasHalfByteCached = false;
            this.cachedHalfByte = 0;
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

