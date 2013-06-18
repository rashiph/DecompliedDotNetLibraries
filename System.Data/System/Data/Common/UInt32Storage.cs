namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class UInt32Storage : DataStorage
    {
        private static readonly uint defaultValue;
        private uint[] values;

        public UInt32Storage(DataColumn column) : base(column, typeof(uint), defaultValue)
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
                uint num11;
                uint maxValue;
                double num13;
                int num14;
                long num15;
                ulong defaultValue;
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
                        defaultValue = UInt32Storage.defaultValue;
                        numArray3 = records;
                        num10 = 0;
                        goto Label_006B;

                    case AggregateType.Mean:
                        num15 = UInt32Storage.defaultValue;
                        num14 = 0;
                        numArray2 = records;
                        num9 = 0;
                        goto Label_00D0;

                    case AggregateType.Min:
                        maxValue = uint.MaxValue;
                        num6 = 0;
                        goto Label_0228;

                    case AggregateType.Max:
                        num11 = 0;
                        num5 = 0;
                        goto Label_027D;

                    case AggregateType.First:
                        if (records.Length <= 0)
                        {
                            return null;
                        }
                        return this.values[records[0]];

                    case AggregateType.Count:
                        num = 0;
                        num4 = 0;
                        goto Label_02D6;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        num = 0;
                        num2 = 0.0;
                        num19 = 0.0;
                        num3 = 0.0;
                        num13 = 0.0;
                        numArray = records;
                        num8 = 0;
                        goto Label_0179;

                    default:
                        goto Label_02FB;
                }
            Label_0043:
                num21 = numArray3[num10];
                if (base.HasValue(num21))
                {
                    defaultValue += this.values[num21];
                    flag = true;
                }
                num10++;
            Label_006B:
                if (num10 < numArray3.Length)
                {
                    goto Label_0043;
                }
                if (flag)
                {
                    return defaultValue;
                }
                return base.NullValue;
            Label_00A2:
                num20 = numArray2[num9];
                if (base.HasValue(num20))
                {
                    num15 += this.values[num20];
                    num14++;
                    flag = true;
                }
                num9++;
            Label_00D0:
                if (num9 < numArray2.Length)
                {
                    goto Label_00A2;
                }
                if (flag)
                {
                    return (uint) (num15 / ((long) num14));
                }
                return base.NullValue;
            Label_0132:
                num7 = numArray[num8];
                if (base.HasValue(num7))
                {
                    num3 += this.values[num7];
                    num13 += this.values[num7] * this.values[num7];
                    num++;
                }
                num8++;
            Label_0179:
                if (num8 < numArray.Length)
                {
                    goto Label_0132;
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
            Label_01FE:
                num18 = records[num6];
                if (base.HasValue(num18))
                {
                    maxValue = Math.Min(this.values[num18], maxValue);
                    flag = true;
                }
                num6++;
            Label_0228:
                if (num6 < records.Length)
                {
                    goto Label_01FE;
                }
                if (flag)
                {
                    return maxValue;
                }
                return base.NullValue;
            Label_0253:
                num17 = records[num5];
                if (base.HasValue(num17))
                {
                    num11 = Math.Max(this.values[num17], num11);
                    flag = true;
                }
                num5++;
            Label_027D:
                if (num5 < records.Length)
                {
                    goto Label_0253;
                }
                if (flag)
                {
                    return num11;
                }
                return base.NullValue;
            Label_02C0:
                if (base.HasValue(records[num4]))
                {
                    num++;
                }
                num4++;
            Label_02D6:
                if (num4 < records.Length)
                {
                    goto Label_02C0;
                }
                return num;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(uint));
            }
        Label_02FB:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            uint num2 = this.values[recordNo1];
            uint num = this.values[recordNo2];
            if ((num2 == defaultValue) || (num == defaultValue))
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
            uint num = this.values[recordNo];
            if ((defaultValue == num) && !base.HasValue(recordNo))
            {
                return -1;
            }
            return num.CompareTo((uint) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((uint) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToUInt32(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToUInt32(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            uint[] numArray = (uint[]) store;
            numArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, !base.HasValue(record));
        }

        public override object Get(int record)
        {
            uint num = this.values[record];
            if (!num.Equals(defaultValue))
            {
                return num;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new uint[recordCount];
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
                this.values[record] = ((IConvertible) value).ToUInt32(base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            uint[] destinationArray = new uint[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (uint[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

