namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class Int64Storage : DataStorage
    {
        private const long defaultValue = 0L;
        private long[] values;

        internal Int64Storage(DataColumn column) : base(column, typeof(long), 0L)
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
                long num10;
                long num11;
                double num12;
                int num13;
                decimal num14;
                long num15;
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
                        num15 = 0L;
                        numArray3 = records;
                        num9 = 0;
                        goto Label_0066;

                    case AggregateType.Mean:
                        num14 = 0M;
                        num13 = 0;
                        numArray2 = records;
                        num8 = 0;
                        goto Label_00D3;

                    case AggregateType.Min:
                        num11 = 0x7fffffffffffffffL;
                        num5 = 0;
                        goto Label_023C;

                    case AggregateType.Max:
                        num10 = -9223372036854775808L;
                        num4 = 0;
                        goto Label_0299;

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
                        num = 0.0;
                        num18 = 0.0;
                        num2 = 0.0;
                        num12 = 0.0;
                        numArray = records;
                        num7 = 0;
                        goto Label_0185;

                    default:
                        goto Label_02F2;
                }
            Label_003F:
                num20 = numArray3[num9];
                if (base.HasValue(num20))
                {
                    num15 += this.values[num20];
                    flag = true;
                }
                num9++;
            Label_0066:
                if (num9 < numArray3.Length)
                {
                    goto Label_003F;
                }
                if (flag)
                {
                    return num15;
                }
                return base.NullValue;
            Label_009D:
                num19 = numArray2[num8];
                if (base.HasValue(num19))
                {
                    num14 += this.values[num19];
                    num13++;
                    flag = true;
                }
                num8++;
            Label_00D3:
                if (num8 < numArray2.Length)
                {
                    goto Label_009D;
                }
                if (flag)
                {
                    return (long) (num14 / num13);
                }
                return base.NullValue;
            Label_0141:
                num6 = numArray[num7];
                if (base.HasValue(num6))
                {
                    num2 += this.values[num6];
                    num12 += this.values[num6] * this.values[num6];
                    num3++;
                }
                num7++;
            Label_0185:
                if (num7 < numArray.Length)
                {
                    goto Label_0141;
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
            Label_0212:
                num17 = records[num5];
                if (base.HasValue(num17))
                {
                    num11 = Math.Min(this.values[num17], num11);
                    flag = true;
                }
                num5++;
            Label_023C:
                if (num5 < records.Length)
                {
                    goto Label_0212;
                }
                if (flag)
                {
                    return num11;
                }
                return base.NullValue;
            Label_026F:
                num16 = records[num4];
                if (base.HasValue(num16))
                {
                    num10 = Math.Max(this.values[num16], num10);
                    flag = true;
                }
                num4++;
            Label_0299:
                if (num4 < records.Length)
                {
                    goto Label_026F;
                }
                if (flag)
                {
                    return num10;
                }
                return base.NullValue;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(long));
            }
        Label_02F2:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            long num2 = this.values[recordNo1];
            long num = this.values[recordNo2];
            if ((num2 == 0L) || (num == 0L))
            {
                int num3 = base.CompareBits(recordNo1, recordNo2);
                if (num3 != 0)
                {
                    return num3;
                }
            }
            if (num2 < num)
            {
                return -1;
            }
            if (num2 <= num)
            {
                return 0;
            }
            return 1;
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
            long num = this.values[recordNo];
            if ((0L == num) && !base.HasValue(recordNo))
            {
                return -1;
            }
            return num.CompareTo((long) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((long) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToInt64(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToInt64(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            long[] numArray = (long[]) store;
            numArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, !base.HasValue(record));
        }

        public override object Get(int record)
        {
            long num = this.values[record];
            if (num != 0L)
            {
                return num;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new long[recordCount];
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = 0L;
                base.SetNullBit(record, true);
            }
            else
            {
                this.values[record] = ((IConvertible) value).ToInt64(base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            long[] destinationArray = new long[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (long[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

