namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Xml;

    internal sealed class ByteStorage : DataStorage
    {
        private const byte defaultValue = 0;
        private byte[] values;

        internal ByteStorage(DataColumn column) : base(column, typeof(byte), (byte) 0)
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
                byte num10;
                byte num11;
                double num12;
                int num13;
                long num14;
                ulong num15;
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
                        goto Label_0067;

                    case AggregateType.Mean:
                        num14 = 0L;
                        num13 = 0;
                        numArray2 = records;
                        num8 = 0;
                        goto Label_00C8;

                    case AggregateType.Min:
                        num11 = 0xff;
                        num5 = 0;
                        goto Label_0221;

                    case AggregateType.Max:
                        num10 = 0;
                        num4 = 0;
                        goto Label_0276;

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
                        goto Label_016E;

                    default:
                        goto Label_02CF;
                }
            Label_003F:
                num20 = numArray3[num9];
                if (!this.IsNull(num20))
                {
                    num15 += this.values[num20];
                    flag = true;
                }
                num9++;
            Label_0067:
                if (num9 < numArray3.Length)
                {
                    goto Label_003F;
                }
                if (flag)
                {
                    return num15;
                }
                return base.NullValue;
            Label_009A:
                num19 = numArray2[num8];
                if (!this.IsNull(num19))
                {
                    num14 += this.values[num19];
                    num13++;
                    flag = true;
                }
                num8++;
            Label_00C8:
                if (num8 < numArray2.Length)
                {
                    goto Label_009A;
                }
                if (flag)
                {
                    return (byte) (num14 / ((long) num13));
                }
                return base.NullValue;
            Label_012A:
                num6 = numArray[num7];
                if (!this.IsNull(num6))
                {
                    num2 += this.values[num6];
                    num12 += this.values[num6] * this.values[num6];
                    num3++;
                }
                num7++;
            Label_016E:
                if (num7 < numArray.Length)
                {
                    goto Label_012A;
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
            Label_01F7:
                num17 = records[num5];
                if (!this.IsNull(num17))
                {
                    num11 = Math.Min(this.values[num17], num11);
                    flag = true;
                }
                num5++;
            Label_0221:
                if (num5 < records.Length)
                {
                    goto Label_01F7;
                }
                if (flag)
                {
                    return num11;
                }
                return base.NullValue;
            Label_024C:
                num16 = records[num4];
                if (!this.IsNull(num16))
                {
                    num10 = Math.Max(this.values[num16], num10);
                    flag = true;
                }
                num4++;
            Label_0276:
                if (num4 < records.Length)
                {
                    goto Label_024C;
                }
                if (flag)
                {
                    return num10;
                }
                return base.NullValue;
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(typeof(byte));
            }
        Label_02CF:
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            byte num3 = this.values[recordNo1];
            byte num2 = this.values[recordNo2];
            if ((num3 == 0) || (num2 == 0))
            {
                int num = base.CompareBits(recordNo1, recordNo2);
                if (num != 0)
                {
                    return num;
                }
            }
            return num3.CompareTo(num2);
        }

        public override int CompareValueTo(int recordNo, object value)
        {
            if (base.NullValue == value)
            {
                if (this.IsNull(recordNo))
                {
                    return 0;
                }
                return 1;
            }
            byte num = this.values[recordNo];
            if ((num == 0) && this.IsNull(recordNo))
            {
                return -1;
            }
            return num.CompareTo((byte) value);
        }

        public override string ConvertObjectToXml(object value)
        {
            return XmlConvert.ToString((byte) value);
        }

        public override object ConvertValue(object value)
        {
            if (base.NullValue != value)
            {
                if (value != null)
                {
                    value = ((IConvertible) value).ToByte(base.FormatProvider);
                    return value;
                }
                value = base.NullValue;
            }
            return value;
        }

        public override object ConvertXmlToObject(string s)
        {
            return XmlConvert.ToByte(s);
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            byte[] buffer = (byte[]) store;
            buffer[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int record)
        {
            byte num = this.values[record];
            if (num != 0)
            {
                return num;
            }
            return base.GetBits(record);
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new byte[recordCount];
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
                this.values[record] = ((IConvertible) value).ToByte(base.FormatProvider);
                base.SetNullBit(record, false);
            }
        }

        public override void SetCapacity(int capacity)
        {
            byte[] destinationArray = new byte[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (byte[]) store;
            base.SetNullStorage(nullbits);
        }
    }
}

