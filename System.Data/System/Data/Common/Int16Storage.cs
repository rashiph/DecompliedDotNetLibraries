namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class Int16Storage : DataStorage
    {
        private const short defaultValue = 0;
        private short[] values;

        internal Int16Storage(DataColumn column) : base(column, typeof(short), (short) 0)
        {
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            bool flag = false;
            try
            {
                int num;
                double num2;
                double num3;
                int num4;
                int num5;
                int num6;
                int num7;
                int num8;
                int num9;
                int num10;
                short num11;
                short num12;
                double num13;
                int num14;
                long num15;
                long num16;
                int num17;
                int num18;
                int[] numArray;
                double num19;
                int num20;
                int[] numArray2;
                int num21;
                int[] numArray3;
                switch (kind)
                {
                    case AggregateType.Sum:
                        num16 = 0L;
                        numArray3 = records;
                        num10 = 0;
                        goto Label_0067;

                    case AggregateType.Mean:
                        num15 = 0L;
                        num14 = 0;
                        numArray2 = records;
                        num9 = 0;
                        goto Label_00C8;

                    case AggregateType.Min:
                        num12 = 0x7fff;
                        num6 = 0;
                        goto Label_0221;

                    case AggregateType.Max:
                        num11 = -32768;
                        num5 = 0;
                        goto Label_027A;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num = 0;
                        num4 = 0;
                        goto Label_02D3;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        num = 0;
                        num2 = 0.0;
                        num19 = 0.0;
                        num3 = 0.0;
                        num13 = 0.0;
                        numArray = records;
                        num8 = 0;
                        goto Label_016E;

                    default:
                        goto Label_02F8;
                }
            Label_003F:
                num21 = numArray3[num10];
                if (base.HasValue(num21))
                {
                    num16 += this.values[num21];
                    flag = true;
                }
                num10++;
            Label_0067:
                if (num10 < numArray3.Length)
                {
                    goto Label_003F;
                }
                if (flag)
                {
                    return num16;
                }
                return base.NullValue;
            Label_009A:
                num20 = numArray2[num9];
                if (base.HasValue(num20))
                {
                    num15 += this.values[num20];
                    num14++;
                    flag = true;
                }
                num9++;
            Label_00C8:
                if (num9 < numArray2.Length)
                {
                    goto Label_009A;
                }
                if (flag)
                {
                    return (short) (num15 / ((long) num14));
                }
                return base.NullValue;
            Label_012A:
                num7 = numArray[num8];
                if (base.HasValue(num7))
                {
                    num3 += this.values[num7];
                    num13 += this.values[num7] * this.values[num7];
                    num++;
                }
                num8++;
            Label_016E:
                if (num8 < numArray.Length)
                {
                    goto Label_012A;
                }
                if (num <= 1)
                {
                    return base.NullValue;
                }
                num2 = (num * num13) - (num3 * num3);
                num19 = num2 / (num3 * num3);
                if ((num19 < 1E-15) || (num2 < 0.0))
                {
                    num2 = 0.0;
                }
                else
                {
                    num2 /= (double) (num * (num - 1));
                }
                if (kind == AggregateType.StDev)
                {
                    return Math.Sqrt(num2);
                }
                return num2;
            Label_01F7:
                num18 = records[num6];
                if (base.HasValue(num18))
                {
                    num12 = Math.Min(this.values[num18], num12);
                    flag = true;
                }
                num6++;
            Label_0221:
                if (num6 < records.Length)
                {
                    goto Label_01F7;
                }
                if (flag)
                {
                    return num12;
                }
                return base.NullValue;
            Label_0250:
                num17 = records[num5];
                if (base.HasValue(num17))
                {
                    num11 = Math.Max(this.values[num17], num11);
                    flag = true;
                }
                num5++;
            Label_027A:
                if (num5 < records.Length)
                {
                    goto Label_0250;
                }
                if (flag)
                {
                    return num11;
                }
                return base.NullValue;
            Label_02BD:
                if (base.HasValue(records[num4]))
                {
                    num++;
                }
                num4++;
            Label_02D3:
                if (num4 < records.Length)
                {
                    goto Label_02BD;
                }
                return num;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(short));
            }
        Label_02F8:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            short num3 = this.values[recordNo1];
            short num2 = this.values[recordNo2];
            if ((num3 == 0) || (num2 == 0))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return (num3 - num2);
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
            short num = this.values[recordNo];
            if ((num == 0) && !base.HasValue(recordNo))
            {
                return -1;
            }
            return num.CompareTo((short) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((short) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToInt16(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToInt16(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            short[] numArray = (short[]) store;
            numArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, !base.HasValue(record));
        }

        public override object Get(int record)
        {
            short num = this.values[record];
            if (num != 0)
            {
                return num;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new short[recordCount];
        }

        public override void Set(int record, object value)
        {
            if (base.NullValue == value)
            {
                this.values[record] = 0;
                base.SetNullBit(record, true);
            }
            else
            {
                this.values[record] = ((IConvertible) value).ToInt16(base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            short[] destinationArray = new short[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (short[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

