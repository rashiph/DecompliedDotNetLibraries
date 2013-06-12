namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class DateTimeOffsetStorage : DataStorage
    {
        private static readonly DateTimeOffset defaultValue = DateTimeOffset.MinValue;
        private DateTimeOffset[] values;

        internal DateTimeOffsetStorage(DataColumn column) : base(column, typeof(DateTimeOffset), defaultValue)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                int num2;
                DateTimeOffset minValue;
                int num3;
                DateTimeOffset maxValue;
                int num4;
                int num5;
                int num6;
                switch (kind)
                {
                    case AggregateType.Min:
                        maxValue = DateTimeOffset.MaxValue;
                        num3 = 0;
                        goto Label_007D;

                    case AggregateType.Max:
                        minValue = DateTimeOffset.MinValue;
                        num2 = 0;
                        goto Label_00F2;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num4 = 0;
                        num = 0;
                        goto Label_0152;

                    default:
                        goto Label_0177;
                }
            Label_0030:
                num6 = records[num3];
                if (base.HasValue(num6))
                {
                    maxValue = (DateTimeOffset.Compare(this.values[num6], maxValue) < 0) ? this.values[num6] : maxValue;
                    flag = true;
                }
                num3++;
            Label_007D:
                if (num3 < records.Length)
                {
                    goto Label_0030;
                }
                if (flag)
                {
                    return maxValue;
                }
                return base.NullValue;
            Label_00AB:
                num5 = records[num2];
                if (base.HasValue(num5))
                {
                    minValue = (DateTimeOffset.Compare(this.values[num5], minValue) >= 0) ? this.values[num5] : minValue;
                    flag = true;
                }
                num2++;
            Label_00F2:
                if (num2 < records.Length)
                {
                    goto Label_00AB;
                }
                if (flag)
                {
                    return minValue;
                }
                return base.NullValue;
            Label_013D:
                if (base.HasValue(records[num]))
                {
                    num4++;
                }
                num++;
            Label_0152:
                if (num < records.Length)
                {
                    goto Label_013D;
                }
                return num4;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(DateTimeOffset));
            }
        Label_0177:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            DateTimeOffset first = this.values[recordNo1];
            DateTimeOffset second = this.values[recordNo2];
            if ((first == defaultValue) || (second == defaultValue))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return DateTimeOffset.Compare(first, second);
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
            DateTimeOffset first = this.values[recordNo];
            if ((defaultValue == first) && !base.HasValue(recordNo))
            {
                return -1;
            }
            return DateTimeOffset.Compare(first, (DateTimeOffset) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((DateTimeOffset) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = (DateTimeOffset) value;
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToDateTimeOffset(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            DateTimeOffset[] offsetArray = (DateTimeOffset[]) store;
            offsetArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, !base.HasValue(record));
        }

        public override object Get(int record)
        {
            DateTimeOffset offset = this.values[record];
            if (!(offset != defaultValue) && !base.HasValue(record))
            {
                return base.NullValue;
            }
            return offset;
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new DateTimeOffset[recordCount];
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
                this.values[record] = (DateTimeOffset) value;
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            DateTimeOffset[] destinationArray = new DateTimeOffset[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (DateTimeOffset[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

