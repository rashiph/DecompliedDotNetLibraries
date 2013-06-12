namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlBytesStorage : DataStorage
    {
        private SqlBytes[] values;

        public SqlBytesStorage(DataColumn column) : base(column, typeof(SqlBytes), SqlBytes.Null, SqlBytes.Null)
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
                        goto Label_0044;

                    default:
                        goto Label_0068;
                }
            Label_0031:
                if (!this.IsNull(records[num]))
                {
                    num2++;
                }
                num++;
            Label_0044:
                if (num < records.Length)
                {
                    goto Label_0031;
                }
                return num2;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(SqlBytes));
            }
        Label_0068:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return 0;
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            return 0;
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

        public override object ConvertXmlToObject(string s)
        {
            SqlBinary binary = new SqlBinary();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = binary;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return new SqlBytes((SqlBinary) serializable);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlBytes[] bytesArray = (SqlBytes[]) store;
            bytesArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlBytes[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            if ((value == DBNull.Value) || (value == null))
            {
                this.values[record] = SqlBytes.Null;
            }
            else
            {
                this.values[record] = (SqlBytes) value;
            }
        }

        public override void SetCapacity(int capacity)
        {
            SqlBytes[] destinationArray = new SqlBytes[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlBytes[]) store;
        }
    }
}

