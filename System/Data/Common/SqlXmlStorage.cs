namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Xml;

    internal sealed class SqlXmlStorage : DataStorage
    {
        private SqlXml[] values;

        public SqlXmlStorage(DataColumn column) : base(column, typeof(SqlXml), SqlXml.Null, SqlXml.Null)
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
                throw ExprException.Overflow(typeof(SqlXml));
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
            SqlXml xml = (SqlXml) value;
            if (xml.IsNull)
            {
                return ADP.StrEmpty;
            }
            return xml.Value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return new SqlXml(new XmlTextReader(s, XmlNodeType.Element, null));
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlXml[] xmlArray = (SqlXml[]) store;
            xmlArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlXml[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            if ((value == DBNull.Value) || (value == null))
            {
                this.values[record] = SqlXml.Null;
            }
            else
            {
                this.values[record] = (SqlXml) value;
            }
        }

        public override void SetCapacity(int capacity)
        {
            SqlXml[] destinationArray = new SqlXml[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlXml[]) store;
        }
    }
}

