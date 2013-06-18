namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class TimeSpanStorage : DataStorage
    {
        private static readonly TimeSpan defaultValue = TimeSpan.Zero;
        private TimeSpan[] values;

        public TimeSpanStorage(DataColumn column) : base(column, typeof(TimeSpan), defaultValue)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                TimeSpan minValue;
                int num2;
                TimeSpan maxValue;
                int num3;
                int num4;
                switch (kind)
                {
                    case AggregateType.Min:
                        maxValue = TimeSpan.MaxValue;
                        num2 = 0;
                        goto Label_0079;

                    case AggregateType.Max:
                        minValue = TimeSpan.MinValue;
                        num = 0;
                        goto Label_00ED;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        return base.Aggregate(records, kind);

                    default:
                        goto Label_014E;
                }
            Label_002F:
                num4 = records[num2];
                if (!this.IsNull(num4))
                {
                    maxValue = (TimeSpan.Compare(this.values[num4], maxValue) < 0) ? this.values[num4] : maxValue;
                    flag = true;
                }
                num2++;
            Label_0079:
                if (num2 < records.Length)
                {
                    goto Label_002F;
                }
                if (flag)
                {
                    return maxValue;
                }
                return base.NullValue;
            Label_00A6:
                num3 = records[num];
                if (!this.IsNull(num3))
                {
                    minValue = (TimeSpan.Compare(this.values[num3], minValue) >= 0) ? this.values[num3] : minValue;
                    flag = true;
                }
                num++;
            Label_00ED:
                if (num < records.Length)
                {
                    goto Label_00A6;
                }
                if (flag)
                {
                    return minValue;
                }
                return base.NullValue;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(TimeSpan));
            }
        Label_014E:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            TimeSpan span2 = this.values[recordNo1];
            TimeSpan span = this.values[recordNo2];
            if ((span2 == defaultValue) || (span == defaultValue))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return TimeSpan.Compare(span2, span);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            if (base.NullValue == value)
            {
                if (this.IsNull(recordNo))
                {
                    return 0;
                }
                return 1;
            }
            TimeSpan span = this.values[recordNo];
            if ((defaultValue == span) && this.IsNull(recordNo))
            {
                return -1;
            }
            return span.CompareTo((TimeSpan) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((TimeSpan) value);
        }

        private static TimeSpan ConvertToTimeSpan(object value)
        {
            Type type = value.GetType();
            if (type == typeof(string))
            {
                return TimeSpan.Parse((string) value);
            }
            if (type == typeof(int))
            {
                return new TimeSpan((long) ((int) value));
            }
            if (type == typeof(long))
            {
                return new TimeSpan((long) value);
            }
            return (TimeSpan) value;
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ConvertToTimeSpan(value);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToTimeSpan(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            TimeSpan[] spanArray = (TimeSpan[]) store;
            spanArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            TimeSpan span = this.values[record];
            if (span != defaultValue)
            {
                return span;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new TimeSpan[recordCount];
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = defaultValue;
                base.SetNullBit(record, true);
            }
            else
            {
                this.values[record] = ConvertToTimeSpan(value);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            TimeSpan[] destinationArray = new TimeSpan[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (TimeSpan[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

