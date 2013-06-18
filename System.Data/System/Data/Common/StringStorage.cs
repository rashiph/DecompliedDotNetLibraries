namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;

    internal sealed class StringStorage : DataStorage
    {
        private string[] values;

        public StringStorage(DataColumn column) : base(column, typeof(string), string.Empty)
        {
        }

        public override object Aggregate(int[] recordNos, AggregateType kind)
        {
            int num;
            int num3;
            switch (kind)
            {
                case AggregateType.Min:
                    num3 = -1;
                    for (num = 0; num < recordNos.Length; num++)
                    {
                        if (!this.IsNull(recordNos[num]))
                        {
                            num3 = recordNos[num];
                            break;
                        }
                    }
                    break;

                case AggregateType.Max:
                {
                    int num2 = -1;
                    for (num = 0; num < recordNos.Length; num++)
                    {
                        if (!this.IsNull(recordNos[num]))
                        {
                            num2 = recordNos[num];
                            break;
                        }
                    }
                    if (num2 < 0)
                    {
                        return base.NullValue;
                    }
                    num++;
                    while (num < recordNos.Length)
                    {
                        if (this.Compare(num2, recordNos[num]) < 0)
                        {
                            num2 = recordNos[num];
                        }
                        num++;
                    }
                    return this.Get(num2);
                }
                case AggregateType.Count:
                {
                    int num4 = 0;
                    num = 0;
                    while (num < recordNos.Length)
                    {
                        if (this.values[recordNos[num]] != null)
                        {
                            num4++;
                        }
                        num++;
                    }
                    return num4;
                }
                default:
                    throw ExceptionBuilder.AggregateException(kind, base.DataType);
            }
            if (num3 < 0)
            {
                return base.NullValue;
            }
            num++;
            while (num < recordNos.Length)
            {
                if (!this.IsNull(recordNos[num]) && (this.Compare(num3, recordNos[num]) > 0))
                {
                    num3 = recordNos[num];
                }
                num++;
            }
            return this.Get(num3);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            string str2 = this.values[recordNo1];
            string str = this.values[recordNo2];
            if (str2 == str)
            {
                return 0;
            }
            if (str2 == null)
            {
                return -1;
            }
            if (str == null)
            {
                return 1;
            }
            return base.Table.Compare(str2, str);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            string str = this.values[recordNo];
            if (str == null)
            {
                if (base.NullValue == value)
                {
                    return 0;
                }
                return -1;
            }
            if (base.NullValue == value)
            {
                return 1;
            }
            return base.Table.Compare(str, (string) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return (string) value;
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = value.ToString();
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return s;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            string[] strArray = (string[]) store;
            strArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int recordNo)
        {
            string str = this.values[recordNo];
            if (str != null)
            {
                return str;
            }
            return base.NullValue;
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new string[recordCount];
        }

        public override int GetStringLength(int record)
        {
            string str = this.values[record];
            if (str == null)
            {
                return 0;
            }
            return str.Length;
        }

        public override bool IsNull(int record)
        {
            return (null == this.values[record]);
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = null;
            }
            else
            {
                this.values[record] = value.ToString();
            }
        }

        public override void SetCapacity(int capacity)
        {
            string[] destinationArray = new string[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (string[]) store;
        }
    }
}

