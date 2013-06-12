namespace System.Xml
{
    using System;

    internal class BitStack
    {
        private uint[] bitStack;
        private uint curr = 1;
        private int stackPos;

        public bool PeekBit()
        {
            return ((this.curr & 1) != 0);
        }

        public bool PopBit()
        {
            bool flag = (this.curr & 1) != 0;
            this.curr = this.curr >> 1;
            if (this.curr == 1)
            {
                this.PopCurr();
            }
            return flag;
        }

        private void PopCurr()
        {
            if (this.stackPos > 0)
            {
                this.curr = this.bitStack[--this.stackPos];
            }
        }

        public void PushBit(bool bit)
        {
            if ((this.curr & 0x80000000) != 0)
            {
                this.PushCurr();
            }
            this.curr = (this.curr << 1) | (bit ? 1 : 0);
        }

        private void PushCurr()
        {
            if (this.bitStack == null)
            {
                this.bitStack = new uint[0x10];
            }
            this.bitStack[this.stackPos++] = this.curr;
            this.curr = 1;
            int length = this.bitStack.Length;
            if (this.stackPos >= length)
            {
                uint[] destinationArray = new uint[2 * length];
                Array.Copy(this.bitStack, destinationArray, length);
                this.bitStack = destinationArray;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.curr == 1);
            }
        }
    }
}

