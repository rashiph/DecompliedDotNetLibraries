namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlStringStorage : DataStorage
    {
        private SqlString[] values;

        public SqlStringStorage(DataColumn column) : base(column, typeof(SqlString), SqlString.Null, SqlString.Null)
        {
        }

        public override object Aggregate(int[] recordNos, AggregateType kind)
        {
            try
            {
                int num;
                int num2;
                int num3;
                int num4;
                switch (kind)
                {
                    case AggregateType.Min:
                        num3 = -1;
                        num = 0;
                        goto Label_003C;

                    case AggregateType.Max:
                        num2 = -1;
                        num = 0;
                        goto Label_00A6;

                    case AggregateType.Count:
                        num4 = 0;
                        num = 0;
                        goto Label_0100;

                    default:
                        goto Label_0125;
                }
            Label_0027:
                if (!this.IsNull(recordNos[num]))
                {
                    num3 = recordNos[num];
                    goto Label_0042;
                }
                num++;
            Label_003C:
                if (num < recordNos.Length)
                {
                    goto Label_0027;
                }
            Label_0042:
                if (num3 >= 0)
                {
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
                return base.NullValue;
            Label_0091:
                if (!this.IsNull(recordNos[num]))
                {
                    num2 = recordNos[num];
                    goto Label_00AC;
                }
                num++;
            Label_00A6:
                if (num < recordNos.Length)
                {
                    goto Label_0091;
                }
            Label_00AC:
                if (num2 >= 0)
                {
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
                return base.NullValue;
            Label_00EB:
                if (!this.IsNull(recordNos[num]))
                {
                    num4++;
                }
                num++;
            Label_0100:
                if (num < recordNos.Length)
                {
                    goto Label_00EB;
                }
                return num4;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(SqlString));
            }
        Label_0125:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public int Compare(SqlString valueNo1, SqlString valueNo2)
        {
            if (valueNo1.IsNull && valueNo2.IsNull)
            {
                return 0;
            }
            if (valueNo1.IsNull)
            {
                return -1;
            }
            if (valueNo2.IsNull)
            {
                return 1;
            }
            return base.Table.Compare(valueNo1.Value, valueNo2.Value);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return this.Compare(this.values[recordNo1], this.values[recordNo2]);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            return this.Compare(this.values[recordNo], (SqlString) value);
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
                return SqlConvert.ConvertToSqlString(value);
            }
            return base.NullValue;
        }

        public override object ConvertXmlToObject(string s)
        {
            SqlString str2 = new SqlString();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = str2;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return (SqlString) serializable;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlString[] strArray = (SqlString[]) store;
            strArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlString[recordCount];
        }

        public override int GetStringLength(int record)
        {
            SqlString str = this.values[record];
            if (!str.IsNull)
            {
                return str.Value.Length;
            }
            return 0;
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            this.values[record] = SqlConvert.ConvertToSqlString(value);
        }

        public override void SetCapacity(int capacity)
        {
            SqlString[] destinationArray = new SqlString[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlString[]) store;
        }
    }
}

