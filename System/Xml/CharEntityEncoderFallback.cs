namespace System.Xml
{
    using System;
    using System.Text;

    internal class CharEntityEncoderFallback : EncoderFallback
    {
        private int curMarkPos;
        private int endMarkPos;
        private CharEntityEncoderFallbackBuffer fallbackBuffer;
        private int startOffset;
        private int[] textContentMarks;

        internal CharEntityEncoderFallback()
        {
        }

        internal bool CanReplaceAt(int index)
        {
            int curMarkPos = this.curMarkPos;
            int num2 = this.startOffset + index;
            while ((curMarkPos < this.endMarkPos) && (num2 >= this.textContentMarks[curMarkPos + 1]))
            {
                curMarkPos++;
            }
            this.curMarkPos = curMarkPos;
            return ((curMarkPos & 1) != 0);
        }

        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            if (this.fallbackBuffer == null)
            {
                this.fallbackBuffer = new CharEntityEncoderFallbackBuffer(this);
            }
            return this.fallbackBuffer;
        }

        internal void Reset(int[] textContentMarks, int endMarkPos)
        {
            this.textContentMarks = textContentMarks;
            this.endMarkPos = endMarkPos;
            this.curMarkPos = 0;
        }

        public override int MaxCharCount
        {
            get
            {
                return 12;
            }
        }

        internal int StartOffset
        {
            get
            {
                return this.startOffset;
            }
            set
            {
                this.startOffset = value;
            }
        }
    }
}

