namespace System.Runtime.InteropServices
{
    using System;
    using System.Threading;

    public sealed class HandleCollector
    {
        private const int deltaPercent = 10;
        private int[] gc_counts;
        private int gc_gen;
        private int handleCount;
        private int initialThreshold;
        private int maximumThreshold;
        private string name;
        private int threshold;

        public HandleCollector(string name, int initialThreshold) : this(name, initialThreshold, 0x7fffffff)
        {
        }

        public HandleCollector(string name, int initialThreshold, int maximumThreshold)
        {
            this.gc_counts = new int[3];
            if (initialThreshold < 0)
            {
                throw new ArgumentOutOfRangeException("initialThreshold", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (maximumThreshold < 0)
            {
                throw new ArgumentOutOfRangeException("maximumThreshold", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (initialThreshold > maximumThreshold)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidThreshold"));
            }
            if (name != null)
            {
                this.name = name;
            }
            else
            {
                this.name = string.Empty;
            }
            this.initialThreshold = initialThreshold;
            this.maximumThreshold = maximumThreshold;
            this.threshold = initialThreshold;
            this.handleCount = 0;
        }

        public void Add()
        {
            int index = -1;
            Interlocked.Increment(ref this.handleCount);
            if (this.handleCount < 0)
            {
                throw new InvalidOperationException(SR.GetString("InvalidOperation_HCCountOverflow"));
            }
            if (this.handleCount > this.threshold)
            {
                lock (this)
                {
                    this.threshold = this.handleCount + (this.handleCount / 10);
                    index = this.gc_gen;
                    if (this.gc_gen < 2)
                    {
                        this.gc_gen++;
                    }
                }
            }
            if ((index >= 0) && ((index == 0) || (this.gc_counts[index] == GC.CollectionCount(index))))
            {
                GC.Collect(index);
                Thread.Sleep((int) (10 * index));
            }
            for (int i = 1; i < 3; i++)
            {
                this.gc_counts[i] = GC.CollectionCount(i);
            }
        }

        public void Remove()
        {
            Interlocked.Decrement(ref this.handleCount);
            if (this.handleCount < 0)
            {
                throw new InvalidOperationException(SR.GetString("InvalidOperation_HCCountOverflow"));
            }
            int num = this.handleCount + (this.handleCount / 10);
            if (num < (this.threshold - (this.threshold / 10)))
            {
                lock (this)
                {
                    if (num > this.initialThreshold)
                    {
                        this.threshold = num;
                    }
                    else
                    {
                        this.threshold = this.initialThreshold;
                    }
                    this.gc_gen = 0;
                }
            }
            for (int i = 1; i < 3; i++)
            {
                this.gc_counts[i] = GC.CollectionCount(i);
            }
        }

        public int Count
        {
            get
            {
                return this.handleCount;
            }
        }

        public int InitialThreshold
        {
            get
            {
                return this.initialThreshold;
            }
        }

        public int MaximumThreshold
        {
            get
            {
                return this.maximumThreshold;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

