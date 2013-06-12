namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class CharStorage : DataStorage
    {
        private const char defaultValue = '\0';
        private char[] values;

        internal CharStorage(DataColumn column) : base(column, typeof(char), '\0')
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                char ch;
                int num2;
                char ch2;
                int num3;
                int num4;
                switch (kind)
                {
                    case AggregateType.Min:
                        ch2 = 0xffff;
                        num2 = 0;
                        goto Label_0061;

                    case AggregateType.Max:
                        ch = '\0';
                        num = 0;
                        goto Label_00B9;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        return base.Aggregate(records, kind);

                    default:
                        goto Label_0111;
                }
            Label_002F:
                num4 = records[num2];
                if (!this.IsNull(num4))
                {
                    ch2 = (this.values[num4] < ch2) ? this.values[num4] : ch2;
                    flag = true;
                }
                num2++;
            Label_0061:
                if (num2 < records.Length)
                {
                    goto Label_002F;
                }
                if (flag)
                {
                    return ch2;
                }
                return base.NullValue;
            Label_008A:
                num3 = records[num];
                if (!this.IsNull(num3))
                {
                    ch = (this.values[num3] > ch) ? this.values[num3] : ch;
                    flag = true;
                }
                num++;
            Label_00B9:
                if (num < records.Length)
                {
                    goto Label_008A;
                }
                if (flag)
                {
                    return ch;
                }
                return base.NullValue;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(char));
            }
        Label_0111:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            char ch2 = this.values[recordNo1];
            char ch = this.values[recordNo2];
            if ((ch2 == '\0') || (ch == '\0'))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return ch2.CompareTo(ch);
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
            char ch = this.values[recordNo];
            if ((ch == '\0') && this.IsNull(recordNo))
            {
                return -1;
            }
            return ch.CompareTo((char) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((char) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToChar(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToChar(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            char[] chArray = (char[]) store;
            chArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            char ch = this.values[record];
            if (ch != '\0')
            {
                return ch;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new char[recordCount];
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = '\0';
                base.SetNullBit(record, true);
            }
            else
            {
                char charValue = ((IConvertible) value).ToChar(base.FormatProvider);
                if (((charValue >= 0xd800) && (charValue <= 0xdfff)) || ((charValue < '!') && (((charValue == '\t') || (charValue == '\n')) || (charValue == '\r'))))
                {
                    throw ExceptionBuilder.ProblematicChars(charValue);
                }
                this.values[record] = charValue;
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            char[] destinationArray = new char[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (char[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

