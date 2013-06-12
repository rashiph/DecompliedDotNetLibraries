namespace System.Data
{
    using System;

    internal abstract class AutoIncrementValue
    {
        private bool auto;

        protected AutoIncrementValue()
        {
        }

        internal AutoIncrementValue Clone()
        {
            AutoIncrementValue value2 = (this is AutoIncrementInt64) ? ((AutoIncrementValue) new AutoIncrementInt64()) : ((AutoIncrementValue) new AutoIncrementBigInteger());
            value2.Auto = this.Auto;
            value2.Seed = this.Seed;
            value2.Step = this.Step;
            value2.Current = this.Current;
            return value2;
        }

        internal abstract void MoveAfter();
        internal abstract void SetCurrent(object value, IFormatProvider formatProvider);
        internal abstract void SetCurrentAndIncrement(object value);

        internal bool Auto
        {
            get
            {
                return this.auto;
            }
            set
            {
                this.auto = value;
            }
        }

        internal abstract object Current { get; set; }

        internal abstract Type DataType { get; }

        internal abstract long Seed { get; set; }

        internal abstract long Step { get; set; }
    }
}

