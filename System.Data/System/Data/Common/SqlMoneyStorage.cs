namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlMoneyStorage : DataStorage
    {
        private SqlMoney[] values;

        public SqlMoneyStorage(DataColumn column) : base(column, typeof(SqlMoney), SqlMoney.Null, SqlMoney.Null)
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
                SqlMoney minValue;
                int num12;
                SqlMoney maxValue;
                SqlDouble num13;
                int num14;
                SqlDecimal num15;
                SqlDecimal num16;
                int[] numArray;
                SqlDouble num17;
                int num18;
                int[] numArray2;
                int num19;
                int[] numArray3;
                switch (kind)
                {
                    case AggregateType.Sum:
                        num16 = 0L;
                        numArray3 = records;
                        num10 = 0;
                        goto Label_007D;

                    case AggregateType.Mean:
                        num15 = 0L;
                        num14 = 0;
                        numArray2 = records;
                        num9 = 0;
                        goto Label_00EF;

                    case AggregateType.Min:
                        maxValue = SqlMoney.MaxValue;
                        num6 = 0;
                        goto Label_0314;

                    case AggregateType.Max:
                        minValue = SqlMoney.MinValue;
                        num5 = 0;
                        goto Label_0393;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num = 0;
                        num4 = 0;
                        goto Label_03F8;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        num = 0;
                        num2 = 0.0;
                        num17 = 0.0;
                        num3 = 0.0;
                        num13 = 0.0;
                        numArray = records;
                        num8 = 0;
                        goto Label_01E7;

                    default:
                        goto Label_041D;
                }
            Label_0044:
                num19 = numArray3[num10];
                if (!this.IsNull(num19))
                {
                    num16 += this.values[num19];
                    flag = true;
                }
                num10++;
            Label_007D:
                if (num10 < numArray3.Length)
                {
                    goto Label_0044;
                }
                if (flag)
                {
                    return num16;
                }
                return base.NullValue;
            Label_00B5:
                num18 = numArray2[num9];
                if (!this.IsNull(num18))
                {
                    num15 += this.values[num18].ToSqlDecimal();
                    num14++;
                    flag = true;
                }
                num9++;
            Label_00EF:
                if (num9 < numArray2.Length)
                {
                    goto Label_00B5;
                }
                if (flag)
                {
                    SqlDecimal num20 = num15 / ((long) num14);
                    return num20.ToSqlMoney();
                }
                return base.NullValue;
            Label_017F:
                num7 = numArray[num8];
                if (!this.IsNull(num7))
                {
                    num3 += this.values[num7].ToSqlDouble();
                    num13 += this.values[num7].ToSqlDouble() * this.values[num7].ToSqlDouble();
                    num++;
                }
                num8++;
            Label_01E7:
                if (num8 < numArray.Length)
                {
                    goto Label_017F;
                }
                if (num <= 1)
                {
                    return base.NullValue;
                }
                num2 = ((SqlDouble) (num * num13)) - (num3 * num3);
                num17 = num2 / (num3 * num3);
                bool x = num17 < 1E-15;
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
            Label_02C4:
                num12 = records[num6];
                if (!this.IsNull(num12))
                {
                    if (SqlMoney.LessThan(this.values[num12], maxValue).IsTrue)
                    {
                        maxValue = this.values[num12];
                    }
                    flag = true;
                }
                num6++;
            Label_0314:
                if (num6 < records.Length)
                {
                    goto Label_02C4;
                }
                if (flag)
                {
                    return maxValue;
                }
                return base.NullValue;
            Label_0343:
                num11 = records[num5];
                if (!this.IsNull(num11))
                {
                    if (SqlMoney.GreaterThan(this.values[num11], minValue).IsTrue)
                    {
                        minValue = this.values[num11];
                    }
                    flag = true;
                }
                num5++;
            Label_0393:
                if (num5 < records.Length)
                {
                    goto Label_0343;
                }
                if (flag)
                {
                    return minValue;
                }
                return base.NullValue;
            Label_03E2:
                if (!this.IsNull(records[num4]))
                {
                    num++;
                }
                num4++;
            Label_03F8:
                if (num4 < records.Length)
                {
                    goto Label_03E2;
                }
                return num;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(SqlMoney));
            }
        Label_041D:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return this.values[recordNo1].CompareTo(this.values[recordNo2]);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            return this.values[recordNo].CompareTo((SqlMoney) value);
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
                return SqlConvert.ConvertToSqlMoney(value);
            }
            return base.NullValue;
        }

        public override object ConvertXmlToObject(string s)
        {
            SqlMoney money = new SqlMoney();
            StringReader input = new StringReader("<col>" + s + "</col>");
            IXmlSerializable serializable = money;
            using (XmlTextReader reader = new XmlTextReader(input))
            {
                serializable.ReadXml(reader);
            }
            return (SqlMoney) serializable;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            SqlMoney[] moneyArray = (SqlMoney[]) store;
            moneyArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new SqlMoney[recordCount];
        }

        public override bool IsNull(int record)
        {
            return this.values[record].IsNull;
        }

        public override void Set(int record, object value)
        {
            this.values[record] = SqlConvert.ConvertToSqlMoney(value);
        }

        public override void SetCapacity(int capacity)
        {
            SqlMoney[] destinationArray = new SqlMoney[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (SqlMoney[]) store;
        }
    }
}

