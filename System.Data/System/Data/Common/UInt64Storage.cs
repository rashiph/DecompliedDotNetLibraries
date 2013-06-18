namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class UInt64Storage : DataStorage
    {
        private static readonly ulong defaultValue;
        private ulong[] values;

        public UInt64Storage(DataColumn column) : base(column, typeof(ulong), defaultValue)
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
                ulong num10;
                ulong maxValue;
                double num12;
                int num13;
                decimal num14;
                ulong defaultValue;
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
                        defaultValue = UInt64Storage.defaultValue;
                        numArray3 = records;
                        num9 = 0;
                        goto Label_0069;

                    case AggregateType.Mean:
                        num14 = UInt64Storage.defaultValue;
                        num13 = 0;
                        numArray2 = records;
                        num8 = 0;
                        goto Label_00DA;

                    case AggregateType.Min:
                        maxValue = ulong.MaxValue;
                        num5 = 0;
                        goto Label_023F;

                    case AggregateType.Max:
                        num10 = 0L;
                        num4 = 0;
                        goto Label_0295;

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
                        goto Label_018F;

                    default:
                        goto Label_02EE;
                }
            Label_0042:
                num20 = numArray3[num9];
                if (base.HasValue(num20))
                {
                    defaultValue += this.values[num20];
                    flag = true;
                }
                num9++;
            Label_0069:
                if (num9 < numArray3.Length)
                {
                    goto Label_0042;
                }
                if (flag)
                {
                    return defaultValue;
                }
                return base.NullValue;
            Label_00A4:
                num19 = numArray2[num8];
                if (base.HasValue(num19))
                {
                    num14 += this.values[num19];
                    num13++;
                    flag = true;
                }
                num8++;
            Label_00DA:
                if (num8 < numArray2.Length)
                {
                    goto Label_00A4;
                }
                if (flag)
                {
                    return (ulong) (num14 / num13);
                }
                return base.NullValue;
            Label_0148:
                num6 = numArray[num7];
                if (base.HasValue(num6))
                {
                    num2 += this.values[num6];
                    num12 += this.values[num6] * this.values[num6];
                    num3++;
                }
                num7++;
            Label_018F:
                if (num7 < numArray.Length)
                {
                    goto Label_0148;
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
            Label_0215:
                num17 = records[num5];
                if (base.HasValue(num17))
                {
                    maxValue = Math.Min(this.values[num17], maxValue);
                    flag = true;
                }
                num5++;
            Label_023F:
                if (num5 < records.Length)
                {
                    goto Label_0215;
                }
                if (flag)
                {
                    return maxValue;
                }
                return base.NullValue;
            Label_026B:
                num16 = records[num4];
                if (base.HasValue(num16))
                {
                    num10 = Math.Max(this.values[num16], num10);
                    flag = true;
                }
                num4++;
            Label_0295:
                if (num4 < records.Length)
                {
                    goto Label_026B;
                }
                if (flag)
                {
                    return num10;
                }
                return base.NullValue;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(ulong));
            }
        Label_02EE:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            ulong num3 = this.values[recordNo1];
            ulong num2 = this.values[recordNo2];
            if (num3.Equals(defaultValue) || num2.Equals(defaultValue))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            if (num3 < num2)
            {
                return -1;
            }
            if (num3 <= num2)
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
            ulong num = this.values[recordNo];
            if ((defaultValue == num) && !base.HasValue(recordNo))
            {
                return -1;
            }
            return num.CompareTo((ulong) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((ulong) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToUInt64(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToUInt64(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            ulong[] numArray = (ulong[]) store;
            numArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, !base.HasValue(record));
        }

        public override object Get(int record)
        {
            ulong num = this.values[record];
            if (!num.Equals(defaultValue))
            {
                return num;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new ulong[recordCount];
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
                this.values[record] = ((IConvertible) value).ToUInt64(base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            ulong[] destinationArray = new ulong[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (ulong[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

