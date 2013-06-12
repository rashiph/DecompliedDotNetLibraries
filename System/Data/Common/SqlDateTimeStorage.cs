namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlDateTimeStorage : DataStorage
    {
        private SqlDateTime[] values;

        public SqlDateTimeStorage(DataColumn column) : base(column, typeof(SqlDateTime), SqlDateTime.Null, SqlDateTime.Null)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                SqlDateTime minValue;
                int num6;
                SqlDateTime maxValue;
                switch (kind)
                {
                    case AggregateType.Min:
                        maxValue = SqlDateTime.MaxValue;
                        num3 = 0;
                        goto Label_007D;

                    case AggregateType.Max:
                        minValue = SqlDateTime.MinValue;
                        num2 = 0;
                        goto Label_00F9;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num4 = 0;
                        num = 0;
                        goto Label_015A;

                    default:
                        goto Label_017F;
                }
            Label_002F:
                num6 = records[num3];
                if (!this.IsNull(num6))
                {
                    if (SqlDateTime.LessThan(this.values[num6], maxValue).IsTrue)
                    {
                        maxValue = this.values[num6];
                    }
                    flag = true;
                }
                num3++;
            Label_007D:
                if (num3 < records.Length)
                {
                    goto Label_002F;
                }
                if (flag)
                {
                    return maxValue;
                }
                return base.NullValue;
            Label_00AB:
                num5 = records[num2];
                if (!this.IsNull(num5))
                {
                    if (SqlDateTime.GreaterThan(this.values[num5], minValue).IsTrue)
                    {
                        minValue = this.values[num5];
                    }
                    flag = true;
                }
                num2++;
            Label_00F9:
                if (num2 < records.Length)
                {
                    goto Label_00AB;
                }
                if (flag)
                {
                    return minValue;
                }
                return base.NullValue;
            Label_0145:
                if (!this.IsNull(records[num]))
                {
                    num4++;
                }
                num++;
            Label_015A:
                if (num < records.Length)
                {
                    goto Label_0145;
                }
                return num4;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(SqlDateTime));
            }
        Label_017F:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return this.values[recordNo1].CompareTo(this.values[recordNo2]);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            return this.values[recordNo].CompareTo((SqlDateTime) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            StringWriter w = new StringWriter(base.FormatProvider);
            using (XmlTextWriter writer = new XmlTextWriter(w))
            {
                ((IXmlSerializable) value).WriteXml(writer);
            }
            return w.ToString();
        }

        public override object ConvertValue(object value)
        {
            if (value != null)
            {
                return SqlConvert.ConvertToSqlDateTime(value);
            }
            return base.NullValue;
        }

        public override object ConvertXmlToObject(string s)
        {
            SqlDateTime time = new SqlDateTime();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = time;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return (SqlDateTime) serializable;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlDateTime[] timeArray = (SqlDateTime[]) store;
            timeArray[storeIndex] = this.values[record];
            nullbits.Set(record, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlDateTime[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            this.values[record] = SqlConvert.ConvertToSqlDateTime(value);
        }

        public override void SetCapacity(int capacity)
        {
            SqlDateTime[] destinationArray = new SqlDateTime[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlDateTime[]) store;
        }
    }
}

