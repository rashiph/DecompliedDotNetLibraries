namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlGuidStorage : DataStorage
    {
        private SqlGuid[] values;

        public SqlGuidStorage(DataColumn column) : base(column, typeof(SqlGuid), SqlGuid.Null, SqlGuid.Null)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            try
            {
                int num;
                int num2;
                switch (kind)
                {
                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num2 = 0;
                        num = 0;
                        goto Label_0052;

                    default:
                        goto Label_0076;
                }
            Label_003F:
                if (!this.IsNull(records[num]))
                {
                    num2++;
                }
                num++;
            Label_0052:
                if (num < records.Length)
                {
                    goto Label_003F;
                }
                return num2;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(SqlGuid));
            }
        Label_0076:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return this.values[recordNo1].CompareTo(this.values[recordNo2]);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            return this.values[recordNo].CompareTo((SqlGuid) value);
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
                return SqlConvert.ConvertToSqlGuid(value);
            }
            return base.NullValue;
        }

        public override object ConvertXmlToObject(string s)
        {
            SqlGuid guid = new SqlGuid();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = guid;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return (SqlGuid) serializable;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlGuid[] guidArray = (SqlGuid[]) store;
            guidArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlGuid[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            this.values[record] = SqlConvert.ConvertToSqlGuid(value);
        }

        public override void SetCapacity(int capacity)
        {
            SqlGuid[] destinationArray = new SqlGuid[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlGuid[]) store;
        }
    }
}

