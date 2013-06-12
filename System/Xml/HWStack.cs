namespace System.Xml
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class HWStack : ICloneable
    {
        private int growthRate;
        private int limit;
        private int size;
        private object[] stack;
        private int used;

        internal HWStack(int GrowthRate) : this(GrowthRate, 0x7fffffff)
        {
        }

        internal HWStack(int GrowthRate, int limit)
        {
            this.growthRate = GrowthRate;
            this.used = 0;
            this.stack = new object[GrowthRate];
            this.size = GrowthRate;
            this.limit = limit;
        }

        private HWStack(object[] stack, int growthRate, int used, int size)
        {
            this.stack = stack;
            this.growthRate = growthRate;
            this.used = used;
            this.size = size;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal void AddToTop(object o)
        {
            if (this.used > 0)
            {
                this.stack[this.used - 1] = o;
            }
        }

        public object Clone()
        {
            return new HWStack((object[]) this.stack.Clone(), this.growthRate, this.used, this.size);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal object Peek()
        {
            if (this.used <= 0)
            {
                return null;
            }
            return this.stack[this.used - 1];
        }

        internal object Pop()
        {
            if (0 < this.used)
            {
                this.used--;
                return this.stack[this.used];
            }
            return null;
        }

        internal object Push()
        {
            if (this.used == this.size)
            {
                if (this.limit <= this.used)
                {
                    throw new XmlException("Xml_StackOverflow", string.Empty);
                }
                object[] destinationArray = new object[this.size + this.growthRate];
                if (this.used > 0)
                {
                    Array.Copy(this.stack, 0, destinationArray, 0, this.used);
                }
                this.stack = destinationArray;
                this.size += this.growthRate;
            }
            return this.stack[this.used++];
        }

        internal object this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.used))
                {
                    throw new IndexOutOfRangeException();
                }
                return this.stack[index];
            }
            set
            {
                if ((index < 0) || (index >= this.used))
                {
                    throw new IndexOutOfRangeException();
                }
                this.stack[index] = value;
            }
        }

        internal int Length
        {
            get
            {
                return this.used;
            }
        }
    }
}

