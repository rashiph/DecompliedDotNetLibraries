namespace System.Data
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Numerics;

    internal sealed class AutoIncrementInt64 : AutoIncrementValue
    {
        private long current;
        private long seed;
        private long step = 1L;

        private bool BoundaryCheck(BigInteger value)
        {
            return (((this.step < 0L) && (value <= this.current)) || ((0L < this.step) && (this.current <= value)));
        }

        internal override void MoveAfter()
        {
            this.current += this.step;
        }

        internal override void SetCurrent(object value, IFormatProvider formatProvider)
        {
            this.current = Convert.ToInt64(value, formatProvider);
        }

        internal override void SetCurrentAndIncrement(object value)
        {
            long num = (long) SqlConvert.ChangeType2(value, StorageType.Int64, typeof(long), CultureInfo.InvariantCulture);
            if (this.BoundaryCheck(num))
            {
                this.current = num + this.step;
            }
        }

        internal override object Current
        {
            get
            {
                return this.current;
            }
            set
            {
                this.current = (long) value;
            }
        }

        internal override Type DataType
        {
            get
            {
                return typeof(long);
            }
        }

        internal override long Seed
        {
            get
            {
                return this.seed;
            }
            set
            {
                if ((this.current == this.seed) || this.BoundaryCheck(value))
                {
                    this.current = value;
                }
                this.seed = value;
            }
        }

        internal override long Step
        {
            get
            {
                return this.step;
            }
            set
            {
                if (0L == value)
                {
                    throw ExceptionBuilder.AutoIncrementSeed();
                }
                if (this.step != value)
                {
                    if (this.current != this.Seed)
                    {
                        this.current = (this.current - this.step) + value;
                    }
                    this.step = value;
                }
            }
        }
    }
}

