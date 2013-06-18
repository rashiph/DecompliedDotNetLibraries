namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class DateTimeStorage : DataStorage
    {
        private static readonly DateTime defaultValue = DateTime.MinValue;
        private DateTime[] values;

        internal DateTimeStorage(DataColumn column) : base(column, typeof(DateTime), defaultValue)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                int num2;
                DateTime minValue;
                int num3;
                DateTime maxValue;
                int num4;
                int num5;
                int num6;
                switch (kind)
                {
                    case AggregateType.Min:
                        maxValue = DateTime.MaxValue;
                        num3 = 0;
                        goto Label_007D;

                    case AggregateType.Max:
                        minValue = DateTime.MinValue;
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
                    maxValue = (DateTime.Compare(this.values[num6], maxValue) < 0) ? this.values[num6] : maxValue;
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
                    minValue = (DateTime.Compare(this.values[num5], minValue) >= 0) ? this.values[num5] : minValue;
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
                throw ExprException.Overflow(typeof(DateTime));
            }
        Label_0177:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            DateTime time2 = this.values[recordNo1];
            DateTime time = this.values[recordNo2];
            if ((time2 == defaultValue) || (time == defaultValue))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return DateTime.Compare(time2, time);
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
            DateTime time = this.values[recordNo];
            if ((defaultValue == time) && !base.HasValue(recordNo))
            {
                return -1;
            }
            return DateTime.Compare(time, (DateTime) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            if (base.DateTimeMode == DataSetDateTime.UnspecifiedLocal)
            {
                return XmlConvert.ToString((DateTime) value, XmlDateTimeSerializationMode.Local);
            }
            return XmlConvert.ToString((DateTime) value, XmlDateTimeSerializationMode.RoundtripKind);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToDateTime(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            if (base.DateTimeMode == DataSetDateTime.UnspecifiedLocal)
            {
                return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Unspecified);
            }
            return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            DateTime[] timeArray = (DateTime[]) store;
            bool flag = !base.HasValue(record);
            if (flag || ((base.DateTimeMode & DataSetDateTime.Local) == ((DataSetDateTime) 0)))
            {
                timeArray[storeIndex] = this.values[record];
            }
            else
            {
                timeArray[storeIndex] = this.values[record].ToUniversalTime();
            }
            nullbits.Set(storeIndex, flag);
        }

        public override object Get(int record)
        {
            DateTime time = this.values[record];
            if (!(time != defaultValue) && !base.HasValue(record))
            {
                return base.NullValue;
            }
            return time;
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new DateTime[recordCount];
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
                DateTime time;
                DateTime time2 = ((IConvertible) value).ToDateTime(base.FormatProvider);
                switch (base.DateTimeMode)
                {
                    case DataSetDateTime.Local:
                        if (time2.Kind != DateTimeKind.Local)
                        {
                            if (time2.Kind == DateTimeKind.Utc)
                            {
                                time = time2.ToLocalTime();
                            }
                            else
                            {
                                time = DateTime.SpecifyKind(time2, DateTimeKind.Local);
                            }
                            break;
                        }
                        time = time2;
                        break;

                    case DataSetDateTime.Unspecified:
                    case DataSetDateTime.UnspecifiedLocal:
                        time = DateTime.SpecifyKind(time2, DateTimeKind.Unspecified);
                        break;

                    case DataSetDateTime.Utc:
                        if (time2.Kind != DateTimeKind.Utc)
                        {
                            if (time2.Kind == DateTimeKind.Local)
                            {
                                time = time2.ToUniversalTime();
                            }
                            else
                            {
                                time = DateTime.SpecifyKind(time2, DateTimeKind.Utc);
                            }
                            break;
                        }
                        time = time2;
                        break;

                    default:
                        throw ExceptionBuilder.InvalidDateTimeMode(base.DateTimeMode);
                }
                this.values[record] = time;
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            DateTime[] destinationArray = new DateTime[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (DateTime[]) store;
            base.SetNullStorage(nullbits);
            if (base.DateTimeMode == DataSetDateTime.UnspecifiedLocal)
            {
                for (int i = 0; i < this.values.Length; i++)
                {
                    if (base.HasValue(i))
                    {
                        this.values[i] = DateTime.SpecifyKind(this.values[i].ToLocalTime(), DateTimeKind.Unspecified);
                    }
                }
            }
            else if (base.DateTimeMode == DataSetDateTime.Local)
            {
                for (int j = 0; j < this.values.Length; j++)
                {
                    if (base.HasValue(j))
                    {
                        this.values[j] = this.values[j].ToLocalTime();
                    }
                }
            }
        }
    }
}

