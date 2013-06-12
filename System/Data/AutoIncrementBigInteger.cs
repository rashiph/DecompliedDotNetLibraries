namespace System.Data
{
    using System;
    using System.Data.Common;
    using System.Numerics;

    internal sealed class AutoIncrementBigInteger : AutoIncrementValue
    {
        private BigInteger current;
        private long seed;
        private BigInteger step = 1;

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
            this.current = BigIntegerStorage.ConvertToBigInteger(value, formatProvider);
        }

        internal override void SetCurrentAndIncrement(object value)
        {
            BigInteger integer = (BigInteger) value;
            if (this.BoundaryCheck(integer))
            {
                this.current = integer + this.step;
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
                this.current = (BigInteger) value;
            }
        }

        internal override Type DataType
        {
            get
            {
                return typeof(BigInteger);
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
                return (long) this.step;
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

