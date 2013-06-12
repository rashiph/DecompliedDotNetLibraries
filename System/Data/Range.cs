namespace System.Data
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Range
    {
        private int min;
        private int max;
        private bool isNotNull;
        public Range(int min, int max)
        {
            if (min > max)
            {
                throw ExceptionBuilder.RangeArgument(min, max);
            }
            this.min = min;
            this.max = max;
            this.isNotNull = true;
        }

        public int Count
        {
            get
            {
                if (this.IsNull)
                {
                    return 0;
                }
                return ((this.max - this.min) + 1);
            }
        }
        public bool IsNull
        {
            get
            {
                return !this.isNotNull;
            }
        }
        public int Max
        {
            get
            {
                this.CheckNull();
                return this.max;
            }
        }
        public int Min
        {
            get
            {
                this.CheckNull();
                return this.min;
            }
        }
        internal void CheckNull()
        {
            if (this.IsNull)
            {
                throw ExceptionBuilder.NullRange();
            }
        }
    }
}

