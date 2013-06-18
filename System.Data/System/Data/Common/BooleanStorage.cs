namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class BooleanStorage : DataStorage
    {
        private const bool defaultValue = false;
        private bool[] values;

        internal BooleanStorage(DataColumn column) : base(column, typeof(bool), false)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                int num2;
                bool flag2;
                bool flag3;
                int num3;
                int num4;
                switch (kind)
                {
                    case AggregateType.Min:
                        flag3 = true;
                        num2 = 0;
                        goto Label_0051;

                    case AggregateType.Max:
                        flag2 = false;
                        num = 0;
                        goto Label_00A1;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        return base.Aggregate(records, kind);

                    default:
                        goto Label_00F9;
                }
            Label_002A:
                num4 = records[num2];
                if (!this.IsNull(num4))
                {
                    flag3 = this.values[num4] && flag3;
                    flag = true;
                }
                num2++;
            Label_0051:
                if (num2 < records.Length)
                {
                    goto Label_002A;
                }
                if (flag)
                {
                    return flag3;
                }
                return base.NullValue;
            Label_007A:
                num3 = records[num];
                if (!this.IsNull(num3))
                {
                    flag2 = this.values[num3] || flag2;
                    flag = true;
                }
                num++;
            Label_00A1:
                if (num < records.Length)
                {
                    goto Label_007A;
                }
                if (flag)
                {
                    return flag2;
                }
                return base.NullValue;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(bool));
            }
        Label_00F9:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            bool flag2 = this.values[recordNo1];
            bool flag = this.values[recordNo2];
            if (!flag2 || !flag)
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return flag2.CompareTo(flag);
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
            bool flag = this.values[recordNo];
            if (!flag && this.IsNull(recordNo))
            {
                return -1;
            }
            return flag.CompareTo((bool) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((bool) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToBoolean(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToBoolean(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            bool[] flagArray = (bool[]) store;
            flagArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            bool flag = this.values[record];
            if (flag)
            {
                return flag;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new bool[recordCount];
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = false;
                base.SetNullBit(record, true);
            }
            else
            {
                this.values[record] = ((IConvertible) value).ToBoolean(base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            bool[] destinationArray = new bool[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (bool[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

