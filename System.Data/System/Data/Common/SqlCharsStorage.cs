namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlCharsStorage : DataStorage
    {
        private SqlChars[] values;

        public SqlCharsStorage(DataColumn column) : base(column, typeof(SqlChars), SqlChars.Null, SqlChars.Null)
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
                throw ExprException.Overflow(typeof(SqlChars));
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
            SqlString str2 = new SqlString();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = str2;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return new SqlChars((SqlString) serializable);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlChars[] charsArray = (SqlChars[]) store;
            charsArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlChars[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            if ((value == DBNull.Value) || (value == null))
            {
                this.values[record] = SqlChars.Null;
            }
            else
            {
                this.values[record] = (SqlChars) value;
            }
        }

        public override void SetCapacity(int capacity)
        {
            SqlChars[] destinationArray = new SqlChars[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlChars[]) store;
        }
    }
}

