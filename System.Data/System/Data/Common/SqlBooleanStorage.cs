namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlBooleanStorage : DataStorage
    {
        private SqlBoolean[] values;

        public SqlBooleanStorage(DataColumn column) : base(column, typeof(SqlBoolean), SqlBoolean.Null, SqlBoolean.Null)
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
                SqlBoolean flag2;
                SqlBoolean flag3;
                int num5;
                int num6;
                switch (kind)
                {
                    case AggregateType.Min:
                        flag3 = 1;
                        num3 = 0;
                        goto Label_0061;

                    case AggregateType.Max:
                        flag2 = 0;
                        num2 = 0;
                        goto Label_00C1;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return base.NullValue;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num4 = 0;
                        num = 0;
                        goto Label_012A;

                    default:
                        goto Label_014F;
                }
            Label_0030:
                num6 = records[num3];
                if (!this.IsNull(num6))
                {
                    flag3 = SqlBoolean.And(this.values[num6], flag3);
                    flag = true;
                }
                num3++;
            Label_0061:
                if (num3 < records.Length)
                {
                    goto Label_0030;
                }
                if (flag)
                {
                    return flag3;
                }
                return base.NullValue;
            Label_0090:
                num5 = records[num2];
                if (!this.IsNull(num5))
                {
                    flag2 = SqlBoolean.Or(this.values[num5], flag2);
                    flag = true;
                }
                num2++;
            Label_00C1:
                if (num2 < records.Length)
                {
                    goto Label_0090;
                }
                if (flag)
                {
                    return flag2;
                }
                return base.NullValue;
            Label_0115:
                if (!this.IsNull(records[num]))
                {
                    num4++;
                }
                num++;
            Label_012A:
                if (num < records.Length)
                {
                    goto Label_0115;
                }
                return num4;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(SqlBoolean));
            }
        Label_014F:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return this.values[recordNo1].CompareTo(this.values[recordNo2]);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            return this.values[recordNo].CompareTo((SqlBoolean) value);
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
                return SqlConvert.ConvertToSqlBoolean(value);
            }
            return base.NullValue;
        }

        public override object ConvertXmlToObject(string s)
        {
            SqlBoolean flag = new SqlBoolean();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = flag;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return (SqlBoolean) serializable;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlBoolean[] flagArray = (SqlBoolean[]) store;
            flagArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlBoolean[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            this.values[record] = SqlConvert.ConvertToSqlBoolean(value);
        }

        public override void SetCapacity(int capacity)
        {
            SqlBoolean[] destinationArray = new SqlBoolean[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlBoolean[]) store;
        }
    }
}

