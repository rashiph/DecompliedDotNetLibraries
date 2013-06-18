namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SequenceRange
    {
        private long lower;
        private long upper;
        public SequenceRange(long number) : this(number, number)
        {
        }

        public SequenceRange(long lower, long upper)
        {
            if (lower < 0L)
            {
                throw Fx.AssertAndThrow("Argument lower cannot be negative.");
            }
            if (lower > upper)
            {
                throw Fx.AssertAndThrow("Argument upper cannot be less than argument lower.");
            }
            this.lower = lower;
            this.upper = upper;
        }

        public long Lower
        {
            get
            {
                return this.lower;
            }
        }
        public long Upper
        {
            get
            {
                return this.upper;
            }
        }
        public static bool operator ==(SequenceRange a, SequenceRange b)
        {
            return ((a.lower == b.lower) && (a.upper == b.upper));
        }

        public static bool operator !=(SequenceRange a, SequenceRange b)
        {
            return !(a == b);
        }

        public bool Contains(long number)
        {
            return ((number >= this.lower) && (number <= this.upper));
        }

        public bool Contains(SequenceRange range)
        {
            return ((range.Lower >= this.lower) && (range.Upper <= this.upper));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return ((obj is SequenceRange) && (this == ((SequenceRange) obj)));
        }

        public override int GetHashCode()
        {
            long num = this.upper ^ (this.upper - this.lower);
            return (int) ((num << 0x20) ^ (num >> 0x20));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", new object[] { this.lower, this.upper });
        }
    }
}

