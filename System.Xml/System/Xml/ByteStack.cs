namespace System.Xml
{
    using System;

    internal class ByteStack
    {
        private int growthRate;
        private int size;
        private byte[] stack;
        private int top;

        public ByteStack(int growthRate)
        {
            this.growthRate = growthRate;
            this.top = 0;
            this.stack = new byte[growthRate];
            this.size = growthRate;
        }

        public byte Peek()
        {
            if (this.top > 0)
            {
                return this.stack[this.top - 1];
            }
            return 0;
        }

        public byte Pop()
        {
            if (this.top > 0)
            {
                return this.stack[--this.top];
            }
            return 0;
        }

        public void Push(byte data)
        {
            if (this.size == this.top)
            {
                byte[] dst = new byte[this.size + this.growthRate];
                if (this.top > 0)
                {
                    Buffer.BlockCopy(this.stack, 0, dst, 0, this.top);
                }
                this.stack = dst;
                this.size += this.growthRate;
            }
            this.stack[this.top++] = data;
        }

        public int Length
        {
            get
            {
                return this.top;
            }
        }
    }
}

