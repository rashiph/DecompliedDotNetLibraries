namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Numerics;

    internal sealed class BigIntegerStorage : DataStorage
    {
        private BigInteger[] values;

        internal BigIntegerStorage(DataColumn column) : base(column, typeof(BigInteger), BigInteger.Zero)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            BigInteger integer2 = this.values[recordNo1];
            BigInteger other = this.values[recordNo2];
            if (integer2.IsZero || other.IsZero)
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return integer2.CompareTo(other);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            if (base.NullValue == value)
            {
                if (!base.HasValue(recordNo))
                {
                    return 0;
                }
                return 1;
            }
            BigInteger integer = this.values[recordNo];
            if (integer.IsZero && !base.HasValue(recordNo))
            {
                return -1;
            }
            return integer.CompareTo((BigInteger) value);
        }

        internal static object ConvertFromBigInteger(BigInteger value, Type type, IFormatProvider formatProvider)
        {
            if (type == typeof(string))
            {
                return value.ToString("D", formatProvider);
            }
            if (type == typeof(sbyte))
            {
                return (sbyte) value;
            }
            if (type == typeof(short))
            {
                return (short) value;
            }
            if (type == typeof(int))
            {
                return (int) value;
            }
            if (type == typeof(long))
            {
                return (long) value;
            }
            if (type == typeof(byte))
            {
                return (byte) value;
            }
            if (type == typeof(ushort))
            {
                return (ushort) value;
            }
            if (type == typeof(uint))
            {
                return (uint) value;
            }
            if (type == typeof(ulong))
            {
                return (ulong) value;
            }
            if (type == typeof(float))
            {
                return (float) value;
            }
            if (type == typeof(double))
            {
                return (double) value;
            }
            if (type == typeof(decimal))
            {
                return (decimal) value;
            }
            if (type != typeof(BigInteger))
            {
                throw ExceptionBuilder.ConvertFailed(typeof(BigInteger), type);
            }
            return value;
        }

        public override string ConvertObjectToXml(object value)
        {
            BigInteger integer = (BigInteger) value;
            return integer.ToString("D", CultureInfo.InvariantCulture);
        }

        internal static BigInteger ConvertToBigInteger(object value, IFormatProvider formatProvider)
        {
            if (value.GetType() == typeof(BigInteger))
            {
                return (BigInteger) value;
            }
            if (value.GetType() == typeof(string))
            {
                return BigInteger.Parse((string) value, formatProvider);
            }
            if (value.GetType() == typeof(long))
            {
                return (long) value;
            }
            if (value.GetType() == typeof(int))
            {
                return (int) value;
            }
            if (value.GetType() == typeof(short))
            {
                return (short) value;
            }
            if (value.GetType() == typeof(sbyte))
            {
                return (sbyte) value;
            }
            if (value.GetType() == typeof(ulong))
            {
                return (ulong) value;
            }
            if (value.GetType() == typeof(uint))
            {
                return (uint) value;
            }
            if (value.GetType() == typeof(ushort))
            {
                return (ushort) value;
            }
            if (value.GetType() != typeof(byte))
            {
                throw ExceptionBuilder.ConvertFailed(value.GetType(), typeof(BigInteger));
            }
            return (byte) value;
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ConvertToBigInteger(value, base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return BigInteger.Parse(s, CultureInfo.InvariantCulture);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            BigInteger[] integerArray = (BigInteger[]) store;
            integerArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, !base.HasValue(record));
        }

        public override object Get(int record)
        {
            BigInteger integer = this.values[record];
            if (!integer.IsZero)
            {
                return integer;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new BigInteger[recordCount];
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = BigInteger.Zero;
                base.SetNullBit(record, true);
            }
            else
            {
                this.values[record] = ConvertToBigInteger(value, base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            BigInteger[] destinationArray = new BigInteger[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (BigInteger[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

