namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlDecimalStorage : DataStorage
    {
        private SqlDecimal[] values;

        public SqlDecimalStorage(DataColumn column) : base(column, typeof(SqlDecimal), SqlDecimal.Null, SqlDecimal.Null)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                SqlDouble num2;
                SqlDouble num3;
                int num4;
                int num5;
                int num6;
                int num7;
                int num8;
                int num9;
                int num10;
                int num11;
                SqlDecimal minValue;
                int num13;
                SqlDecimal maxValue;
                SqlDouble num15;
                int num16;
                SqlDecimal num17;
                SqlDecimal num18;
                int[] numArray;
                SqlDouble num19;
                int num21;
                int[] numArray2;
                int num22;
                int[] numArray3;
                switch (kind)
                {
                    case AggregateType.Sum:
                        num18 = 0L;
                        numArray3 = records;
                        num10 = 0;
                        goto Label_0078;

                    case AggregateType.Mean:
                        num17 = 0L;
                        num16 = 0;
                        numArray2 = records;
                        num9 = 0;
                        goto Label_00EA;

                    case AggregateType.Min:
                        maxValue = SqlDecimal.MaxValue;
                        num6 = 0;
                        goto Label_0306;

                    case AggregateType.Max:
                        minValue = SqlDecimal.MinValue;
                        num5 = 0;
                        goto Label_0385;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num = 0;
                        num4 = 0;
                        goto Label_03EA;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        num = 0;
                        num2 = 0.0;
                        num19 = 0.0;
                        num3 = 0.0;
                        num15 = 0.0;
                        numArray = records;
                        num8 = 0;
                        goto Label_01D9;

                    default:
                        goto Label_040F;
                }
            Label_0044:
                num22 = numArray3[num10];
                if (!this.IsNull(num22))
                {
                    num18 += this.values[num22];
                    flag = true;
                }
                num10++;
            Label_0078:
                if (num10 < numArray3.Length)
                {
                    goto Label_0044;
                }
                if (flag)
                {
                    return num18;
                }
                return base.NullValue;
            Label_00B0:
                num21 = numArray2[num9];
                if (!this.IsNull(num21))
                {
                    num17 += this.values[num21];
                    num16++;
                    flag = true;
                }
                num9++;
            Label_00EA:
                if (num9 < numArray2.Length)
                {
                    goto Label_00B0;
                }
                if (flag)
                {
                    return (num17 / ((long) num16));
                }
                return base.NullValue;
            Label_0171:
                num7 = numArray[num8];
                if (!this.IsNull(num7))
                {
                    num3 += this.values[num7].ToSqlDouble();
                    num15 += this.values[num7].ToSqlDouble() * this.values[num7].ToSqlDouble();
                    num++;
                }
                num8++;
            Label_01D9:
                if (num8 < numArray.Length)
                {
                    goto Label_0171;
                }
                if (num <= 1)
                {
                    return base.NullValue;
                }
                num2 = ((SqlDouble) (num * num15)) - (num3 * num3);
                num19 = num2 / (num3 * num3);
                bool x = num19 < 1E-15;
                if (!SqlBoolean.op_True(x))
                {
                }
                if (SqlBoolean.op_True(x | (num2 < 0.0)))
                {
                    num2 = 0.0;
                }
                else
                {
                    num2 /= (double) (num * (num - 1));
                }
                if (kind == AggregateType.StDev)
                {
                    return Math.Sqrt(num2.Value);
                }
                return num2;
            Label_02B6:
                num13 = records[num6];
                if (!this.IsNull(num13))
                {
                    if (SqlDecimal.LessThan(this.values[num13], maxValue).IsTrue)
                    {
                        maxValue = this.values[num13];
                    }
                    flag = true;
                }
                num6++;
            Label_0306:
                if (num6 < records.Length)
                {
                    goto Label_02B6;
                }
                if (flag)
                {
                    return maxValue;
                }
                return base.NullValue;
            Label_0335:
                num11 = records[num5];
                if (!this.IsNull(num11))
                {
                    if (SqlDecimal.GreaterThan(this.values[num11], minValue).IsTrue)
                    {
                        minValue = this.values[num11];
                    }
                    flag = true;
                }
                num5++;
            Label_0385:
                if (num5 < records.Length)
                {
                    goto Label_0335;
                }
                if (flag)
                {
                    return minValue;
                }
                return base.NullValue;
            Label_03D4:
                if (!this.IsNull(records[num4]))
                {
                    num++;
                }
                num4++;
            Label_03EA:
                if (num4 < records.Length)
                {
                    goto Label_03D4;
                }
                return num;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(SqlDecimal));
            }
        Label_040F:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return this.values[recordNo1].CompareTo(this.values[recordNo2]);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            return this.values[recordNo].CompareTo((SqlDecimal) value);
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
                return SqlConvert.ConvertToSqlDecimal(value);
            }
            return base.NullValue;
        }

        public override object ConvertXmlToObject(string s)
        {
            SqlDecimal num = new SqlDecimal();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = num;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return (SqlDecimal) serializable;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlDecimal[] numArray = (SqlDecimal[]) store;
            numArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlDecimal[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            this.values[record] = SqlConvert.ConvertToSqlDecimal(value);
        }

        public override void SetCapacity(int capacity)
        {
            SqlDecimal[] destinationArray = new SqlDecimal[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlDecimal[]) store;
        }
    }
}

