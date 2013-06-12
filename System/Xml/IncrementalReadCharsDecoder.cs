namespace System.Xml
{
    using System;

    internal class IncrementalReadCharsDecoder : IncrementalReadDecoder
    {
        private char[] buffer;
        private int curIndex;
        private int endIndex;
        private int startIndex;

        internal IncrementalReadCharsDecoder()
        {
        }

        internal override int Decode(char[] chars, int startPos, int len)
        {
            int num = this.endIndex - this.curIndex;
            if (num > len)
            {
                num = len;
            }
            Buffer.BlockCopy(chars, startPos * 2, this.buffer, this.curIndex * 2, num * 2);
            this.curIndex += num;
            return num;
        }

        internal override int Decode(string str, int startPos, int len)
        {
            int count = this.endIndex - this.curIndex;
            if (count > len)
            {
                count = len;
            }
            str.CopyTo(startPos, this.buffer, this.curIndex, count);
            this.curIndex += count;
            return count;
        }

        internal override void Reset()
        {
        }

        internal override void SetNextOutputBuffer(Array buffer, int index, int count)
        {
            this.buffer = (char[]) buffer;
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

