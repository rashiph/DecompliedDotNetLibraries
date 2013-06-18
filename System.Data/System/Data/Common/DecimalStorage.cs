namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class DecimalStorage : DataStorage
    {
        private static readonly decimal defaultValue;
        private decimal[] values;

        internal DecimalStorage(DataColumn column) : base(column, typeof(decimal), defaultValue)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                double num;
                double num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                int num8;
                int num9;
                decimal num10;
                decimal num11;
                double num12;
                int num13;
                decimal num14;
                decimal defaultValue;
                int num16;
                int num17;
                int[] numArray;
                double num18;
                int num19;
                int[] numArray2;
                int num20;
                int[] numArray3;
                switch (kind)
                {
                    case AggregateType.Sum:
                        defaultValue = DecimalStorage.defaultValue;
                        numArray3 = records;
                        num9 = 0;
                        goto Label_0076;

                    case AggregateType.Mean:
                        num14 = DecimalStorage.defaultValue;
                        num13 = 0;
                        numArray2 = records;
                        num8 = 0;
                        goto Label_00E6;

                    case AggregateType.Min:
                        num11 = 79228162514264337593543950335M;
                        num5 = 0;
                        goto Label_0286;

                    case AggregateType.Max:
                        num10 = -79228162514264337593543950335M;
                        num4 = 0;
                        goto Label_02F1;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        return base.Aggregate(records, kind);

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        num3 = 0;
                        num = (double) DecimalStorage.defaultValue;
                        num18 = (double) DecimalStorage.defaultValue;
                        num2 = (double) DecimalStorage.defaultValue;
                        num12 = (double) DecimalStorage.defaultValue;
                        numArray = records;
                        num7 = 0;
                        goto Label_01C5;

                    default:
                        goto Label_0353;
                }
            Label_0042:
                num20 = numArray3[num9];
                if (base.HasValue(num20))
                {
                    defaultValue += this.values[num20];
                    flag = true;
                }
                num9++;
            Label_0076:
                if (num9 < numArray3.Length)
                {
                    goto Label_0042;
                }
                if (flag)
                {
                    return defaultValue;
                }
                return base.NullValue;
            Label_00AC:
                num19 = numArray2[num8];
                if (base.HasValue(num19))
                {
                    num14 += this.values[num19];
                    num13++;
                    flag = true;
                }
                num8++;
            Label_00E6:
                if (num8 < numArray2.Length)
                {
                    goto Label_00AC;
                }
                if (flag)
                {
                    return (num14 / num13);
                }
                return base.NullValue;
            Label_0157:
                num6 = numArray[num7];
                if (base.HasValue(num6))
                {
                    num2 += (double) this.values[num6];
                    num12 += ((double) this.values[num6]) * ((double) this.values[num6]);
                    num3++;
                }
                num7++;
            Label_01C5:
                if (num7 < numArray.Length)
                {
                    goto Label_0157;
                }
                if (num3 <= 1)
                {
                    return base.NullValue;
                }
                num = (num3 * num12) - (num2 * num2);
                num18 = num / (num2 * num2);
                if ((num18 < 1E-15) || (num < 0.0))
                {
                    num = 0.0;
                }
                else
                {
                    num /= (double) (num3 * (num3 - 1));
                }
                if (kind == AggregateType.StDev)
                {
                    return Math.Sqrt(num);
                }
                return num;
            Label_0253:
                num17 = records[num5];
                if (base.HasValue(num17))
                {
                    num11 = Math.Min(this.values[num17], num11);
                    flag = true;
                }
                num5++;
            Label_0286:
                if (num5 < records.Length)
                {
                    goto Label_0253;
                }
                if (flag)
                {
                    return num11;
                }
                return base.NullValue;
            Label_02BE:
                num16 = records[num4];
                if (base.HasValue(num16))
                {
                    num10 = Math.Max(this.values[num16], num10);
                    flag = true;
                }
                num4++;
            Label_02F1:
                if (num4 < records.Length)
                {
                    goto Label_02BE;
                }
                if (flag)
                {
                    return num10;
                }
                return base.NullValue;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(decimal));
            }
        Label_0353:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            decimal num3 = this.values[recordNo1];
            decimal num2 = this.values[recordNo2];
            if ((num3 == defaultValue) || (num2 == defaultValue))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return decimal.Compare(num3, num2);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            if (base.NullValue == value)
            {
                if (!base.HasValue(recordNo))
                {
                    return 0;
                }
                return 1;
            }
            decimal num = this.values[recordNo];
            if ((defaultValue == num) && !base.HasValue(recordNo))
            {
                return -1;
            }
            return decimal.Compare(num, (decimal) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((decimal) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToDecimal(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToDecimal(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            decimal[] numArray = (decimal[]) store;
            numArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, !base.HasValue(record));
        }

        public override object Get(int record)
        {
            if (!base.HasValue(record))
            {
                return base.NullValue;
            }
            return this.values[record];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new decimal[recordCount];
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = defaultValue;
                base.SetNullBit(record, true);
            }
            else
            {
                this.values[record] = ((IConvertible) value).ToDecimal(base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            decimal[] destinationArray = new decimal[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (decimal[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

