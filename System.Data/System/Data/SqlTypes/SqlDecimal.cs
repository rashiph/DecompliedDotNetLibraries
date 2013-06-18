namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlDecimal : INullable, IComparable, IXmlSerializable
    {
        private const int HelperTableStartIndexLo = 5;
        private const int HelperTableStartIndexMid = 15;
        private const int HelperTableStartIndexHi = 0x18;
        private const int HelperTableStartIndexHiHi = 0x21;
        internal byte m_bStatus;
        internal byte m_bLen;
        internal byte m_bPrec;
        internal byte m_bScale;
        internal uint m_data1;
        internal uint m_data2;
        internal uint m_data3;
        internal uint m_data4;
        private static readonly byte NUMERIC_MAX_PRECISION;
        public static readonly byte MaxPrecision;
        public static readonly byte MaxScale;
        private static readonly byte x_bNullMask;
        private static readonly byte x_bIsNull;
        private static readonly byte x_bNotNull;
        private static readonly byte x_bReverseNullMask;
        private static readonly byte x_bSignMask;
        private static readonly byte x_bPositive;
        private static readonly byte x_bNegative;
        private static readonly byte x_bReverseSignMask;
        private static readonly uint x_uiZero;
        private static readonly int x_cNumeMax;
        private static readonly long x_lInt32Base;
        private static readonly ulong x_ulInt32Base;
        private static readonly ulong x_ulInt32BaseForMod;
        internal static readonly ulong x_llMax;
        private static readonly uint x_ulBase10;
        private static readonly double DUINT_BASE;
        private static readonly double DUINT_BASE2;
        private static readonly double DUINT_BASE3;
        private static readonly double DMAX_NUME;
        private static readonly uint DBL_DIG;
        private static readonly byte x_cNumeDivScaleMin;
        private static readonly uint[] x_rgulShiftBase;
        private static readonly uint[] DecimalHelpersLo;
        private static readonly uint[] DecimalHelpersMid;
        private static readonly uint[] DecimalHelpersHi;
        private static readonly uint[] DecimalHelpersHiHi;
        private static readonly byte[] rgCLenFromPrec;
        private static readonly uint x_ulT1;
        private static readonly uint x_ulT2;
        private static readonly uint x_ulT3;
        private static readonly uint x_ulT4;
        private static readonly uint x_ulT5;
        private static readonly uint x_ulT6;
        private static readonly uint x_ulT7;
        private static readonly uint x_ulT8;
        private static readonly uint x_ulT9;
        private static readonly ulong x_dwlT10;
        private static readonly ulong x_dwlT11;
        private static readonly ulong x_dwlT12;
        private static readonly ulong x_dwlT13;
        private static readonly ulong x_dwlT14;
        private static readonly ulong x_dwlT15;
        private static readonly ulong x_dwlT16;
        private static readonly ulong x_dwlT17;
        private static readonly ulong x_dwlT18;
        private static readonly ulong x_dwlT19;
        public static readonly SqlDecimal Null;
        public static readonly SqlDecimal MinValue;
        public static readonly SqlDecimal MaxValue;
        private byte CalculatePrecision()
        {
            int num;
            uint num2;
            uint[] decimalHelpersHiHi;
            if (this.m_data4 != 0)
            {
                num = 0x21;
                decimalHelpersHiHi = DecimalHelpersHiHi;
                num2 = this.m_data4;
            }
            else if (this.m_data3 != 0)
            {
                num = 0x18;
                decimalHelpersHiHi = DecimalHelpersHi;
                num2 = this.m_data3;
            }
            else if (this.m_data2 != 0)
            {
                num = 15;
                decimalHelpersHiHi = DecimalHelpersMid;
                num2 = this.m_data2;
            }
            else
            {
                num = 5;
                decimalHelpersHiHi = DecimalHelpersLo;
                num2 = this.m_data1;
            }
            if (num2 < decimalHelpersHiHi[num])
            {
                num -= 2;
                if (num2 < decimalHelpersHiHi[num])
                {
                    num -= 2;
                    if (num2 < decimalHelpersHiHi[num])
                    {
                        num--;
                    }
                    else
                    {
                        num++;
                    }
                }
                else
                {
                    num++;
                }
            }
            else
            {
                num += 2;
                if (num2 < decimalHelpersHiHi[num])
                {
                    num--;
                }
                else
                {
                    num++;
                }
            }
            if (num2 >= decimalHelpersHiHi[num])
            {
                num++;
                if ((num == 0x25) && (num2 >= decimalHelpersHiHi[num]))
                {
                    num++;
                }
            }
            byte num3 = (byte) (num + 1);
            if ((num3 > 1) && this.VerifyPrecision((byte) (num3 - 1)))
            {
                num3 = (byte) (num3 - 1);
            }
            return Math.Max(num3, this.m_bScale);
        }

        private bool VerifyPrecision(byte precision)
        {
            int index = precision - 1;
            if (this.m_data4 < DecimalHelpersHiHi[index])
            {
                return true;
            }
            if (this.m_data4 == DecimalHelpersHiHi[index])
            {
                if (this.m_data3 < DecimalHelpersHi[index])
                {
                    return true;
                }
                if (this.m_data3 == DecimalHelpersHi[index])
                {
                    if (this.m_data2 < DecimalHelpersMid[index])
                    {
                        return true;
                    }
                    if ((this.m_data2 == DecimalHelpersMid[index]) && (this.m_data1 < DecimalHelpersLo[index]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private SqlDecimal(bool fNull)
        {
            this.m_bLen = this.m_bPrec = (byte) (this.m_bScale = 0);
            this.m_bStatus = 0;
            this.m_data1 = this.m_data2 = this.m_data3 = this.m_data4 = x_uiZero;
        }

        public SqlDecimal(decimal value)
        {
            this.m_bStatus = x_bNotNull;
            int[] bits = decimal.GetBits(value);
            uint num = (uint) bits[3];
            this.m_data1 = (uint) bits[0];
            this.m_data2 = (uint) bits[1];
            this.m_data3 = (uint) bits[2];
            this.m_data4 = x_uiZero;
            this.m_bStatus = (byte) (this.m_bStatus | (((num & 0x80000000) == 0x80000000) ? x_bNegative : 0));
            if (this.m_data3 != 0)
            {
                this.m_bLen = 3;
            }
            else if (this.m_data2 != 0)
            {
                this.m_bLen = 2;
            }
            else
            {
                this.m_bLen = 1;
            }
            this.m_bScale = (byte) ((num & 0xff0000) >> 0x10);
            this.m_bPrec = 0;
            this.m_bPrec = this.CalculatePrecision();
        }

        public SqlDecimal(int value)
        {
            this.m_bStatus = x_bNotNull;
            uint num = (uint) value;
            if (value < 0)
            {
                this.m_bStatus = (byte) (this.m_bStatus | x_bNegative);
                if (value != -2147483648)
                {
                    num = (uint) -value;
                }
            }
            this.m_data1 = num;
            this.m_data2 = this.m_data3 = this.m_data4 = x_uiZero;
            this.m_bLen = 1;
            this.m_bPrec = BGetPrecUI4(this.m_data1);
            this.m_bScale = 0;
        }

        public SqlDecimal(long value)
        {
            this.m_bStatus = x_bNotNull;
            ulong dwlVal = (ulong) value;
            if (value < 0L)
            {
                this.m_bStatus = (byte) (this.m_bStatus | x_bNegative);
                if (value != -9223372036854775808L)
                {
                    dwlVal = (ulong) -value;
                }
            }
            this.m_data1 = (uint) dwlVal;
            this.m_data2 = (uint) (dwlVal >> 0x20);
            this.m_data3 = this.m_data4 = 0;
            this.m_bLen = (this.m_data2 == 0) ? ((byte) 1) : ((byte) 2);
            this.m_bPrec = BGetPrecUI8(dwlVal);
            this.m_bScale = 0;
        }

        public SqlDecimal(byte bPrecision, byte bScale, bool fPositive, int[] bits)
        {
            CheckValidPrecScale(bPrecision, bScale);
            if (bits == null)
            {
                throw new ArgumentNullException("bits");
            }
            if (bits.Length != 4)
            {
                throw new ArgumentException(SQLResource.InvalidArraySizeMessage, "bits");
            }
            this.m_bPrec = bPrecision;
            this.m_bScale = bScale;
            this.m_data1 = (uint) bits[0];
            this.m_data2 = (uint) bits[1];
            this.m_data3 = (uint) bits[2];
            this.m_data4 = (uint) bits[3];
            this.m_bLen = 1;
            for (int i = 3; i >= 0; i--)
            {
                if (bits[i] != 0)
                {
                    this.m_bLen = (byte) (i + 1);
                    break;
                }
            }
            this.m_bStatus = x_bNotNull;
            if (!fPositive)
            {
                this.m_bStatus = (byte) (this.m_bStatus | x_bNegative);
            }
            if (this.FZero())
            {
                this.SetPositive();
            }
            if (bPrecision < this.CalculatePrecision())
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
        }

        public SqlDecimal(byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4)
        {
            CheckValidPrecScale(bPrecision, bScale);
            this.m_bPrec = bPrecision;
            this.m_bScale = bScale;
            this.m_data1 = (uint) data1;
            this.m_data2 = (uint) data2;
            this.m_data3 = (uint) data3;
            this.m_data4 = (uint) data4;
            this.m_bLen = 1;
            if (data4 == 0)
            {
                if (data3 == 0)
                {
                    if (data2 == 0)
                    {
                        this.m_bLen = 1;
                    }
                    else
                    {
                        this.m_bLen = 2;
                    }
                }
                else
                {
                    this.m_bLen = 3;
                }
            }
            else
            {
                this.m_bLen = 4;
            }
            this.m_bStatus = x_bNotNull;
            if (!fPositive)
            {
                this.m_bStatus = (byte) (this.m_bStatus | x_bNegative);
            }
            if (this.FZero())
            {
                this.SetPositive();
            }
            if (bPrecision < this.CalculatePrecision())
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
        }

        public SqlDecimal(double dVal) : this(false)
        {
            this.m_bStatus = x_bNotNull;
            if (dVal < 0.0)
            {
                dVal = -dVal;
                this.m_bStatus = (byte) (this.m_bStatus | x_bNegative);
            }
            if (dVal >= DMAX_NUME)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            double num = Math.Floor(dVal);
            double d = dVal - num;
            this.m_bPrec = NUMERIC_MAX_PRECISION;
            this.m_bLen = 1;
            if (num > 0.0)
            {
                dVal = Math.Floor((double) (num / DUINT_BASE));
                this.m_data1 = (uint) (num - (dVal * DUINT_BASE));
                num = dVal;
                if (num > 0.0)
                {
                    dVal = Math.Floor((double) (num / DUINT_BASE));
                    this.m_data2 = (uint) (num - (dVal * DUINT_BASE));
                    num = dVal;
                    this.m_bLen = (byte) (this.m_bLen + 1);
                    if (num > 0.0)
                    {
                        dVal = Math.Floor((double) (num / DUINT_BASE));
                        this.m_data3 = (uint) (num - (dVal * DUINT_BASE));
                        num = dVal;
                        this.m_bLen = (byte) (this.m_bLen + 1);
                        if (num > 0.0)
                        {
                            dVal = Math.Floor((double) (num / DUINT_BASE));
                            this.m_data4 = (uint) (num - (dVal * DUINT_BASE));
                            num = dVal;
                            this.m_bLen = (byte) (this.m_bLen + 1);
                        }
                    }
                }
            }
            uint bScale = this.FZero() ? 0 : this.CalculatePrecision();
            if (bScale > DBL_DIG)
            {
                uint num6;
                uint num3 = bScale - DBL_DIG;
                do
                {
                    num6 = this.DivByULong(10);
                    num3--;
                }
                while (num3 > 0);
                num3 = bScale - DBL_DIG;
                if (num6 >= 5)
                {
                    this.AddULong(1);
                    bScale = this.CalculatePrecision() + num3;
                }
                do
                {
                    this.MultByULong(10);
                    num3--;
                }
                while (num3 > 0);
            }
            this.m_bScale = (bScale < DBL_DIG) ? ((byte) (DBL_DIG - bScale)) : ((byte) 0);
            this.m_bPrec = (byte) (bScale + this.m_bScale);
            if (this.m_bScale > 0)
            {
                bScale = this.m_bScale;
                do
                {
                    uint num5 = (bScale >= 9) ? 9 : bScale;
                    d *= x_rgulShiftBase[((int) num5) - 1];
                    bScale -= num5;
                    this.MultByULong(x_rgulShiftBase[((int) num5) - 1]);
                    this.AddULong((uint) d);
                    d -= Math.Floor(d);
                }
                while (bScale > 0);
            }
            if (d >= 0.5)
            {
                this.AddULong(1);
            }
            if (this.FZero())
            {
                this.SetPositive();
            }
        }

        private SqlDecimal(uint[] rglData, byte bLen, byte bPrec, byte bScale, bool fPositive)
        {
            CheckValidPrecScale(bPrec, bScale);
            this.m_bLen = bLen;
            this.m_bPrec = bPrec;
            this.m_bScale = bScale;
            this.m_data1 = rglData[0];
            this.m_data2 = rglData[1];
            this.m_data3 = rglData[2];
            this.m_data4 = rglData[3];
            this.m_bStatus = x_bNotNull;
            if (!fPositive)
            {
                this.m_bStatus = (byte) (this.m_bStatus | x_bNegative);
            }
            if (this.FZero())
            {
                this.SetPositive();
            }
        }

        public bool IsNull
        {
            get
            {
                return ((this.m_bStatus & x_bNullMask) == x_bIsNull);
            }
        }
        public decimal Value
        {
            get
            {
                return this.ToDecimal();
            }
        }
        public bool IsPositive
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return ((this.m_bStatus & x_bSignMask) == x_bPositive);
            }
        }
        private void SetPositive()
        {
            this.m_bStatus = (byte) (this.m_bStatus & x_bReverseSignMask);
        }

        private void SetSignBit(bool fPositive)
        {
            this.m_bStatus = fPositive ? ((byte) (this.m_bStatus & x_bReverseSignMask)) : ((byte) (this.m_bStatus | x_bNegative));
        }

        public byte Precision
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_bPrec;
            }
        }
        public byte Scale
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_bScale;
            }
        }
        public int[] Data
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return new int[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            }
        }
        public byte[] BinData
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                int num4 = (int) this.m_data1;
                int num3 = (int) this.m_data2;
                int num2 = (int) this.m_data3;
                int num = (int) this.m_data4;
                byte[] buffer = new byte[0x10];
                buffer[0] = (byte) (num4 & 0xff);
                num4 = num4 >> 8;
                buffer[1] = (byte) (num4 & 0xff);
                num4 = num4 >> 8;
                buffer[2] = (byte) (num4 & 0xff);
                num4 = num4 >> 8;
                buffer[3] = (byte) (num4 & 0xff);
                buffer[4] = (byte) (num3 & 0xff);
                num3 = num3 >> 8;
                buffer[5] = (byte) (num3 & 0xff);
                num3 = num3 >> 8;
                buffer[6] = (byte) (num3 & 0xff);
                num3 = num3 >> 8;
                buffer[7] = (byte) (num3 & 0xff);
                buffer[8] = (byte) (num2 & 0xff);
                num2 = num2 >> 8;
                buffer[9] = (byte) (num2 & 0xff);
                num2 = num2 >> 8;
                buffer[10] = (byte) (num2 & 0xff);
                num2 = num2 >> 8;
                buffer[11] = (byte) (num2 & 0xff);
                buffer[12] = (byte) (num & 0xff);
                num = num >> 8;
                buffer[13] = (byte) (num & 0xff);
                num = num >> 8;
                buffer[14] = (byte) (num & 0xff);
                num = num >> 8;
                buffer[15] = (byte) (num & 0xff);
                return buffer;
            }
        }
        public override string ToString()
        {
            char[] chArray;
            if (this.IsNull)
            {
                return SQLResource.NullString;
            }
            uint[] rgulU = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            int bLen = this.m_bLen;
            char[] chArray2 = new char[NUMERIC_MAX_PRECISION + 1];
            int index = 0;
            while ((bLen > 1) || (rgulU[0] != 0))
            {
                uint num4;
                MpDiv1(rgulU, ref bLen, x_ulBase10, out num4);
                chArray2[index++] = ChFromDigit(num4);
            }
            while (index <= this.m_bScale)
            {
                chArray2[index++] = ChFromDigit(0);
            }
            int num3 = 0;
            int num2 = 0;
            if (this.m_bScale > 0)
            {
                num3 = 1;
            }
            if (this.IsPositive)
            {
                chArray = new char[num3 + index];
            }
            else
            {
                chArray = new char[(num3 + index) + 1];
                chArray[num2++] = '-';
            }
            while (index > 0)
            {
                if (index-- == this.m_bScale)
                {
                    chArray[num2++] = '.';
                }
                chArray[num2++] = chArray2[index];
            }
            return new string(chArray);
        }

        public static SqlDecimal Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            SqlDecimal @null = Null;
            char[] chArray = s.ToCharArray();
            int length = chArray.Length;
            int num4 = -1;
            int index = 0;
            @null.m_bPrec = 1;
            @null.m_bScale = 0;
            @null.SetToZero();
            while ((length != 0) && (chArray[length - 1] == ' '))
            {
                length--;
            }
            if (length == 0)
            {
                throw new FormatException(SQLResource.FormatMessage);
            }
            while (chArray[index] == ' ')
            {
                index++;
                length--;
            }
            if (chArray[index] == '-')
            {
                @null.SetSignBit(false);
                index++;
                length--;
            }
            else
            {
                @null.SetSignBit(true);
                if (chArray[index] == '+')
                {
                    index++;
                    length--;
                }
            }
            while ((length > 2) && (chArray[index] == '0'))
            {
                index++;
                length--;
            }
            if (((2 == length) && ('0' == chArray[index])) && ('.' == chArray[index + 1]))
            {
                chArray[index] = '.';
                chArray[index + 1] = '0';
            }
            if ((length == 0) || (length > (NUMERIC_MAX_PRECISION + 1)))
            {
                throw new FormatException(SQLResource.FormatMessage);
            }
            while ((length > 1) && (chArray[index] == '0'))
            {
                index++;
                length--;
            }
            int num3 = 0;
            while (num3 < length)
            {
                char ulAdd = chArray[index];
                index++;
                if ((ulAdd >= '0') && (ulAdd <= '9'))
                {
                    ulAdd = (char) (ulAdd - '0');
                }
                else
                {
                    if ((ulAdd != '.') || (num4 >= 0))
                    {
                        throw new FormatException(SQLResource.FormatMessage);
                    }
                    num4 = num3;
                    goto Label_015C;
                }
                @null.MultByULong(x_ulBase10);
                @null.AddULong(ulAdd);
            Label_015C:
                num3++;
            }
            if (num4 < 0)
            {
                @null.m_bPrec = (byte) num3;
                @null.m_bScale = 0;
            }
            else
            {
                @null.m_bPrec = (byte) (num3 - 1);
                @null.m_bScale = (byte) (@null.m_bPrec - num4);
            }
            if (@null.m_bPrec > NUMERIC_MAX_PRECISION)
            {
                throw new FormatException(SQLResource.FormatMessage);
            }
            if (@null.m_bPrec == 0)
            {
                throw new FormatException(SQLResource.FormatMessage);
            }
            if (@null.FZero())
            {
                @null.SetPositive();
            }
            return @null;
        }

        public double ToDouble()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            double num = 0.0;
            num = this.m_data4;
            num = (num * x_lInt32Base) + this.m_data3;
            num = (num * x_lInt32Base) + this.m_data2;
            num = (num * x_lInt32Base) + this.m_data1;
            num /= Math.Pow(10.0, (double) this.m_bScale);
            if (!this.IsPositive)
            {
                return -num;
            }
            return num;
        }

        private decimal ToDecimal()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            if ((this.m_data4 != 0) || (this.m_bScale > 0x1c))
            {
                throw new OverflowException(SQLResource.ConversionOverflowMessage);
            }
            return new decimal((int) this.m_data1, (int) this.m_data2, (int) this.m_data3, !this.IsPositive, this.m_bScale);
        }

        public static implicit operator SqlDecimal(decimal x)
        {
            return new SqlDecimal(x);
        }

        public static explicit operator SqlDecimal(double x)
        {
            return new SqlDecimal(x);
        }

        public static implicit operator SqlDecimal(long x)
        {
            return new SqlDecimal(new decimal(x));
        }

        public static explicit operator decimal(SqlDecimal x)
        {
            return x.Value;
        }

        public static SqlDecimal operator -(SqlDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            SqlDecimal num = x;
            if (num.FZero())
            {
                num.SetPositive();
                return num;
            }
            num.SetSignBit(!num.IsPositive);
            return num;
        }

        public static SqlDecimal operator +(SqlDecimal x, SqlDecimal y)
        {
            int num;
            ulong num2;
            byte num11;
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            bool fPositive = true;
            bool isPositive = x.IsPositive;
            bool flag2 = y.IsPositive;
            int bScale = x.m_bScale;
            int num7 = y.m_bScale;
            int num9 = Math.Max((int) (x.m_bPrec - bScale), (int) (y.m_bPrec - num7));
            int num3 = Math.Max(bScale, num7);
            int num6 = (num9 + num3) + 1;
            num6 = Math.Min(MaxPrecision, num6);
            if ((num6 - num9) < num3)
            {
                num3 = num6 - num9;
            }
            if (bScale != num3)
            {
                x.AdjustScale(num3 - bScale, true);
            }
            if (num7 != num3)
            {
                y.AdjustScale(num3 - num7, true);
            }
            if (!isPositive)
            {
                isPositive = !isPositive;
                flag2 = !flag2;
                fPositive = !fPositive;
            }
            int bLen = x.m_bLen;
            int num4 = y.m_bLen;
            uint[] rglData = new uint[] { x.m_data1, x.m_data2, x.m_data3, x.m_data4 };
            uint[] numArray4 = new uint[] { y.m_data1, y.m_data2, y.m_data3, y.m_data4 };
            if (flag2)
            {
                num2 = 0L;
                num = 0;
                while ((num < bLen) || (num < num4))
                {
                    if (num < bLen)
                    {
                        num2 += rglData[num];
                    }
                    if (num < num4)
                    {
                        num2 += numArray4[num];
                    }
                    rglData[num] = (uint) num2;
                    num2 = num2 >> 0x20;
                    num++;
                }
                if (num2 != 0L)
                {
                    if (num == x_cNumeMax)
                    {
                        throw new OverflowException(SQLResource.ArithOverflowMessage);
                    }
                    rglData[num] = (uint) num2;
                    num++;
                }
                num11 = (byte) num;
            }
            else
            {
                int num10 = 0;
                if (x.LAbsCmp(y) < 0)
                {
                    fPositive = !fPositive;
                    uint[] numArray5 = numArray4;
                    numArray4 = rglData;
                    rglData = numArray5;
                    bLen = num4;
                    num4 = x.m_bLen;
                }
                num2 = x_ulInt32Base;
                for (num = 0; (num < bLen) || (num < num4); num++)
                {
                    if (num < bLen)
                    {
                        num2 += rglData[num];
                    }
                    if (num < num4)
                    {
                        num2 -= numArray4[num];
                    }
                    rglData[num] = (uint) num2;
                    if (rglData[num] != 0)
                    {
                        num10 = num;
                    }
                    num2 = num2 >> 0x20;
                    num2 += x_ulInt32BaseForMod;
                }
                num11 = (byte) (num10 + 1);
            }
            SqlDecimal num12 = new SqlDecimal(rglData, num11, (byte) num6, (byte) num3, fPositive);
            if (num12.FGt10_38() || (num12.CalculatePrecision() > NUMERIC_MAX_PRECISION))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            if (num12.FZero())
            {
                num12.SetPositive();
            }
            return num12;
        }

        public static SqlDecimal operator -(SqlDecimal x, SqlDecimal y)
        {
            return (x + -y);
        }

        public static SqlDecimal operator *(SqlDecimal x, SqlDecimal y)
        {
            SqlDecimal num11;
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int bLen = y.m_bLen;
            int num10 = x.m_bScale + y.m_bScale;
            int num3 = num10;
            int num13 = ((x.m_bPrec - x.m_bScale) + (y.m_bPrec - y.m_bScale)) + 1;
            int num6 = num3 + num13;
            if (num6 > NUMERIC_MAX_PRECISION)
            {
                num6 = NUMERIC_MAX_PRECISION;
            }
            if (num3 > NUMERIC_MAX_PRECISION)
            {
                num3 = NUMERIC_MAX_PRECISION;
            }
            num3 = Math.Max(Math.Min(num6 - num13, num3), Math.Min(num10, x_cNumeDivScaleMin));
            int digits = num3 - num10;
            bool fPositive = x.IsPositive == y.IsPositive;
            uint[] numArray5 = new uint[] { x.m_data1, x.m_data2, x.m_data3, x.m_data4 };
            uint[] numArray4 = new uint[] { y.m_data1, y.m_data2, y.m_data3, y.m_data4 };
            uint[] rgulU = new uint[9];
            int index = 0;
            for (int i = 0; i < x.m_bLen; i++)
            {
                uint num16 = numArray5[i];
                ulong num2 = 0L;
                index = i;
                for (int j = 0; j < bLen; j++)
                {
                    ulong num7 = num2 + rgulU[index];
                    ulong num15 = numArray4[j];
                    num2 = num16 * num15;
                    num2 += num7;
                    if (num2 < num7)
                    {
                        num7 = x_ulInt32Base;
                    }
                    else
                    {
                        num7 = 0L;
                    }
                    rgulU[index++] = (uint) num2;
                    num2 = (num2 >> 0x20) + num7;
                }
                if (num2 != 0L)
                {
                    rgulU[index++] = (uint) num2;
                }
            }
            while ((rgulU[index] == 0) && (index > 0))
            {
                index--;
            }
            int ciulU = index + 1;
            if (digits != 0)
            {
                if (digits < 0)
                {
                    uint num12;
                    uint num14;
                    do
                    {
                        if (digits <= -9)
                        {
                            num12 = x_rgulShiftBase[8];
                            digits += 9;
                        }
                        else
                        {
                            num12 = x_rgulShiftBase[-digits - 1];
                            digits = 0;
                        }
                        MpDiv1(rgulU, ref ciulU, num12, out num14);
                    }
                    while (digits != 0);
                    if (ciulU > x_cNumeMax)
                    {
                        throw new OverflowException(SQLResource.ArithOverflowMessage);
                    }
                    for (index = ciulU; index < x_cNumeMax; index++)
                    {
                        rgulU[index] = 0;
                    }
                    num11 = new SqlDecimal(rgulU, (byte) ciulU, (byte) num6, (byte) num3, fPositive);
                    if (num11.FGt10_38())
                    {
                        throw new OverflowException(SQLResource.ArithOverflowMessage);
                    }
                    if (num14 >= (num12 / 2))
                    {
                        num11.AddULong(1);
                    }
                    if (num11.FZero())
                    {
                        num11.SetPositive();
                    }
                    return num11;
                }
                if (ciulU > x_cNumeMax)
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                for (index = ciulU; index < x_cNumeMax; index++)
                {
                    rgulU[index] = 0;
                }
                num11 = new SqlDecimal(rgulU, (byte) ciulU, (byte) num6, (byte) num10, fPositive);
                if (num11.FZero())
                {
                    num11.SetPositive();
                }
                num11.AdjustScale(digits, true);
                return num11;
            }
            if (ciulU > x_cNumeMax)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            for (index = ciulU; index < x_cNumeMax; index++)
            {
                rgulU[index] = 0;
            }
            num11 = new SqlDecimal(rgulU, (byte) ciulU, (byte) num6, (byte) num3, fPositive);
            if (num11.FGt10_38())
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            if (num11.FZero())
            {
                num11.SetPositive();
            }
            return num11;
        }

        public static SqlDecimal operator /(SqlDecimal x, SqlDecimal y)
        {
            int num4;
            int num8;
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.FZero())
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            bool fPositive = x.IsPositive == y.IsPositive;
            int num = Math.Max((x.m_bScale + y.m_bPrec) + 1, (int) x_cNumeDivScaleMin);
            int num3 = (x.m_bPrec - x.m_bScale) + y.m_bScale;
            int num2 = ((num + x.m_bPrec) + y.m_bPrec) + 1;
            int num7 = Math.Min(num, x_cNumeDivScaleMin);
            num3 = Math.Min(num3, NUMERIC_MAX_PRECISION);
            num2 = num3 + num;
            if (num2 > NUMERIC_MAX_PRECISION)
            {
                num2 = NUMERIC_MAX_PRECISION;
            }
            num = Math.Max(Math.Min(num2 - num3, num), num7);
            int digits = (num - x.m_bScale) + y.m_bScale;
            x.AdjustScale(digits, true);
            uint[] rgulU = new uint[] { x.m_data1, x.m_data2, x.m_data3, x.m_data4 };
            uint[] rgulD = new uint[] { y.m_data1, y.m_data2, y.m_data3, y.m_data4 };
            uint[] rgulR = new uint[x_cNumeMax + 1];
            uint[] rgulQ = new uint[x_cNumeMax];
            MpDiv(rgulU, x.m_bLen, rgulD, y.m_bLen, rgulQ, out num4, rgulR, out num8);
            ZeroToMaxLen(rgulQ, num4);
            SqlDecimal num5 = new SqlDecimal(rgulQ, (byte) num4, (byte) num2, (byte) num, fPositive);
            if (num5.FZero())
            {
                num5.SetPositive();
            }
            return num5;
        }

        public static explicit operator SqlDecimal(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal(x.ByteValue);
            }
            return Null;
        }

        public static implicit operator SqlDecimal(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDecimal(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDecimal(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDecimal(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDecimal(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal(x.ToDecimal());
            }
            return Null;
        }

        public static explicit operator SqlDecimal(SqlSingle x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal((double) x.Value);
            }
            return Null;
        }

        public static explicit operator SqlDecimal(SqlDouble x)
        {
            if (!x.IsNull)
            {
                return new SqlDecimal(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlDecimal(SqlString x)
        {
            if (!x.IsNull)
            {
                return Parse(x.Value);
            }
            return Null;
        }

        [Conditional("DEBUG")]
        private void AssertValid()
        {
            if (!this.IsNull)
            {
                uint[] numArray2 = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
                uint num1 = numArray2[this.m_bLen - 1];
                for (int i = this.m_bLen; i < x_cNumeMax; i++)
                {
                }
            }
        }

        private static void ZeroToMaxLen(uint[] rgulData, int cUI4sCur)
        {
            switch (cUI4sCur)
            {
                case 1:
                    uint num3;
                    rgulData[3] = num3 = 0;
                    rgulData[1] = rgulData[2] = num3;
                    return;

                case 2:
                    rgulData[2] = rgulData[3] = 0;
                    return;

                case 3:
                    rgulData[3] = 0;
                    return;
            }
        }

        private static byte CLenFromPrec(byte bPrec)
        {
            return rgCLenFromPrec[bPrec - 1];
        }

        private bool FZero()
        {
            return ((this.m_data1 == 0) && (this.m_bLen <= 1));
        }

        private bool FGt10_38()
        {
            if ((this.m_data4 < 0x4b3b4ca8L) || (this.m_bLen != 4))
            {
                return false;
            }
            return (((this.m_data4 > 0x4b3b4ca8L) || (this.m_data3 > 0x5a86c47aL)) || ((this.m_data3 == 0x5a86c47aL) && (this.m_data2 >= 0x98a2240L)));
        }

        private bool FGt10_38(uint[] rglData)
        {
            if (rglData[3] < 0x4b3b4ca8L)
            {
                return false;
            }
            return (((rglData[3] > 0x4b3b4ca8L) || (rglData[2] > 0x5a86c47aL)) || ((rglData[2] == 0x5a86c47aL) && (rglData[1] >= 0x98a2240L)));
        }

        private static byte BGetPrecUI4(uint value)
        {
            int num;
            if (value < x_ulT4)
            {
                if (value < x_ulT2)
                {
                    num = (value >= x_ulT1) ? 2 : 1;
                }
                else
                {
                    num = (value >= x_ulT3) ? 4 : 3;
                }
            }
            else if (value < x_ulT8)
            {
                if (value < x_ulT6)
                {
                    num = (value >= x_ulT5) ? 6 : 5;
                }
                else
                {
                    num = (value >= x_ulT7) ? 8 : 7;
                }
            }
            else
            {
                num = (value >= x_ulT9) ? 10 : 9;
            }
            return (byte) num;
        }

        private static byte BGetPrecUI8(uint ulU0, uint ulU1)
        {
            ulong dwlVal = ulU0 + (ulU1 << 0x20);
            return BGetPrecUI8(dwlVal);
        }

        private static byte BGetPrecUI8(ulong dwlVal)
        {
            int num;
            if (dwlVal < x_ulT8)
            {
                uint num2 = (uint) dwlVal;
                if (num2 < x_ulT4)
                {
                    if (num2 < x_ulT2)
                    {
                        num = (num2 >= x_ulT1) ? 2 : 1;
                    }
                    else
                    {
                        num = (num2 >= x_ulT3) ? 4 : 3;
                    }
                }
                else if (num2 < x_ulT6)
                {
                    num = (num2 >= x_ulT5) ? 6 : 5;
                }
                else
                {
                    num = (num2 >= x_ulT7) ? 8 : 7;
                }
            }
            else if (dwlVal < x_dwlT16)
            {
                if (dwlVal < x_dwlT12)
                {
                    if (dwlVal < x_dwlT10)
                    {
                        num = (dwlVal >= x_ulT9) ? 10 : 9;
                    }
                    else
                    {
                        num = (dwlVal >= x_dwlT11) ? 12 : 11;
                    }
                }
                else if (dwlVal < x_dwlT14)
                {
                    num = (dwlVal >= x_dwlT13) ? 14 : 13;
                }
                else
                {
                    num = (dwlVal >= x_dwlT15) ? 0x10 : 15;
                }
            }
            else if (dwlVal < x_dwlT18)
            {
                num = (dwlVal >= x_dwlT17) ? 0x12 : 0x11;
            }
            else
            {
                num = (dwlVal >= x_dwlT19) ? 20 : 0x13;
            }
            return (byte) num;
        }

        private void AddULong(uint ulAdd)
        {
            ulong num2 = ulAdd;
            int bLen = this.m_bLen;
            uint[] rguiData = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            int index = 0;
        Label_003A:
            num2 += rguiData[index];
            rguiData[index] = (uint) num2;
            num2 = num2 >> 0x20;
            if (0L == num2)
            {
                this.StoreFromWorkingArray(rguiData);
            }
            else
            {
                index++;
                if (index < bLen)
                {
                    goto Label_003A;
                }
                if (index == x_cNumeMax)
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                rguiData[index] = (uint) num2;
                this.m_bLen = (byte) (this.m_bLen + 1);
                if (this.FGt10_38(rguiData))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                this.StoreFromWorkingArray(rguiData);
            }
        }

        private void MultByULong(uint uiMultiplier)
        {
            int bLen = this.m_bLen;
            ulong num = 0L;
            ulong num2 = 0L;
            uint[] rglData = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            for (int i = 0; i < bLen; i++)
            {
                ulong num5 = rglData[i];
                num2 = num5 * uiMultiplier;
                num += num2;
                if (num < num2)
                {
                    num2 = x_ulInt32Base;
                }
                else
                {
                    num2 = 0L;
                }
                rglData[i] = (uint) num;
                num = (num >> 0x20) + num2;
            }
            if (num != 0L)
            {
                if (bLen == x_cNumeMax)
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                rglData[bLen] = (uint) num;
                this.m_bLen = (byte) (this.m_bLen + 1);
            }
            if (this.FGt10_38(rglData))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            this.StoreFromWorkingArray(rglData);
        }

        private uint DivByULong(uint iDivisor)
        {
            ulong num4 = iDivisor;
            ulong num = 0L;
            uint num3 = 0;
            bool flag = true;
            if (num4 == 0L)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            uint[] rguiData = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            for (int i = this.m_bLen; i > 0; i--)
            {
                num = (num << 0x20) + rguiData[i - 1];
                num3 = (uint) (num / num4);
                rguiData[i - 1] = num3;
                num = num % num4;
                if (flag && (num3 == 0))
                {
                    this.m_bLen = (byte) (this.m_bLen - 1);
                }
                else
                {
                    flag = false;
                }
            }
            this.StoreFromWorkingArray(rguiData);
            if (flag)
            {
                this.m_bLen = 1;
            }
            return (uint) num;
        }

        internal void AdjustScale(int digits, bool fRound)
        {
            uint num2;
            bool flag = false;
            int num = digits;
            if ((num + this.m_bScale) < 0)
            {
                throw new SqlTruncateException();
            }
            if ((num + this.m_bScale) > NUMERIC_MAX_PRECISION)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            byte num4 = (byte) (num + this.m_bScale);
            byte num3 = (byte) Math.Min(NUMERIC_MAX_PRECISION, Math.Max(1, num + this.m_bPrec));
            if (num > 0)
            {
                this.m_bScale = num4;
                this.m_bPrec = num3;
                while (num > 0)
                {
                    if (num >= 9)
                    {
                        num2 = x_rgulShiftBase[8];
                        num -= 9;
                    }
                    else
                    {
                        num2 = x_rgulShiftBase[num - 1];
                        num = 0;
                    }
                    this.MultByULong(num2);
                }
            }
            else if (num < 0)
            {
                uint num5;
                do
                {
                    if (num <= -9)
                    {
                        num2 = x_rgulShiftBase[8];
                        num += 9;
                    }
                    else
                    {
                        num2 = x_rgulShiftBase[-num - 1];
                        num = 0;
                    }
                    num5 = this.DivByULong(num2);
                }
                while (num < 0);
                flag = num5 >= (num2 / 2);
                this.m_bScale = num4;
                this.m_bPrec = num3;
            }
            if (flag && fRound)
            {
                this.AddULong(1);
            }
            else if (this.FZero())
            {
                this.SetPositive();
            }
        }

        public static SqlDecimal AdjustScale(SqlDecimal n, int digits, bool fRound)
        {
            if (n.IsNull)
            {
                return Null;
            }
            SqlDecimal num = n;
            num.AdjustScale(digits, fRound);
            return num;
        }

        public static SqlDecimal ConvertToPrecScale(SqlDecimal n, int precision, int scale)
        {
            CheckValidPrecScale(precision, scale);
            if (n.IsNull)
            {
                return Null;
            }
            SqlDecimal num3 = n;
            int digits = scale - num3.m_bScale;
            num3.AdjustScale(digits, true);
            byte num = CLenFromPrec((byte) precision);
            if (num < num3.m_bLen)
            {
                throw new SqlTruncateException();
            }
            if ((num == num3.m_bLen) && (precision < num3.CalculatePrecision()))
            {
                throw new SqlTruncateException();
            }
            num3.m_bPrec = (byte) precision;
            return num3;
        }

        private int LAbsCmp(SqlDecimal snumOp)
        {
            int bLen = snumOp.m_bLen;
            int num3 = this.m_bLen;
            if (bLen != num3)
            {
                if (num3 <= bLen)
                {
                    return -1;
                }
                return 1;
            }
            uint[] numArray4 = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            uint[] numArray3 = new uint[] { snumOp.m_data1, snumOp.m_data2, snumOp.m_data3, snumOp.m_data4 };
            int index = bLen - 1;
            do
            {
                if (numArray4[index] != numArray3[index])
                {
                    if (numArray4[index] <= numArray3[index])
                    {
                        return -1;
                    }
                    return 1;
                }
                index--;
            }
            while (index >= 0);
            return 0;
        }

        private static void MpMove(uint[] rgulS, int ciulS, uint[] rgulD, out int ciulD)
        {
            ciulD = ciulS;
            for (int i = 0; i < ciulS; i++)
            {
                rgulD[i] = rgulS[i];
            }
        }

        private static void MpSet(uint[] rgulD, out int ciulD, uint iulN)
        {
            ciulD = 1;
            rgulD[0] = iulN;
        }

        private static void MpNormalize(uint[] rgulU, ref int ciulU)
        {
            while ((ciulU > 1) && (rgulU[ciulU - 1] == 0))
            {
                ciulU--;
            }
        }

        private static void MpMul1(uint[] piulD, ref int ciulD, uint iulX)
        {
            uint num2 = 0;
            int index = 0;
            while (index < ciulD)
            {
                ulong num4 = piulD[index];
                ulong x = num2 + (num4 * iulX);
                num2 = HI(x);
                piulD[index] = LO(x);
                index++;
            }
            if (num2 != 0)
            {
                piulD[index] = num2;
                ciulD++;
            }
        }

        private static void MpDiv1(uint[] rgulU, ref int ciulU, uint iulD, out uint iulR)
        {
            uint num2 = 0;
            ulong num4 = iulD;
            int index = ciulU;
            while (index > 0)
            {
                index--;
                ulong num3 = (num2 << 0x20) + rgulU[index];
                rgulU[index] = (uint) (num3 / num4);
                num2 = (uint) (num3 - (rgulU[index] * num4));
            }
            iulR = num2;
            MpNormalize(rgulU, ref ciulU);
        }

        internal static ulong DWL(uint lo, uint hi)
        {
            return (lo + (hi << 0x20));
        }

        private static uint HI(ulong x)
        {
            return (uint) (x >> 0x20);
        }

        private static uint LO(ulong x)
        {
            return (uint) x;
        }

        private static void MpDiv(uint[] rgulU, int ciulU, uint[] rgulD, int ciulD, uint[] rgulQ, out int ciulQ, uint[] rgulR, out int ciulR)
        {
            if ((ciulD == 1) && (rgulD[0] == 0))
            {
                ciulQ = ciulR = 0;
            }
            else if ((ciulU == 1) && (ciulD == 1))
            {
                MpSet(rgulQ, out ciulQ, rgulU[0] / rgulD[0]);
                MpSet(rgulR, out ciulR, rgulU[0] % rgulD[0]);
            }
            else if (ciulD > ciulU)
            {
                MpMove(rgulU, ciulU, rgulR, out ciulR);
                MpSet(rgulQ, out ciulQ, 0);
            }
            else if (ciulU <= 2)
            {
                ulong num14 = DWL(rgulU[0], rgulU[1]);
                ulong num11 = rgulD[0];
                if (ciulD > 1)
                {
                    num11 += rgulD[1] << 0x20;
                }
                ulong x = num14 / num11;
                rgulQ[0] = LO(x);
                rgulQ[1] = HI(x);
                ciulQ = (HI(x) != 0) ? 2 : 1;
                x = num14 % num11;
                rgulR[0] = LO(x);
                rgulR[1] = HI(x);
                ciulR = (HI(x) != 0) ? 2 : 1;
            }
            else if (ciulD == 1)
            {
                uint num18;
                MpMove(rgulU, ciulU, rgulQ, out ciulQ);
                MpDiv1(rgulQ, ref ciulQ, rgulD[0], out num18);
                rgulR[0] = num18;
                ciulR = 1;
            }
            else
            {
                ciulQ = ciulR = 0;
                if (rgulU != rgulR)
                {
                    MpMove(rgulU, ciulU, rgulR, out ciulR);
                }
                ciulQ = (ciulU - ciulD) + 1;
                uint num6 = rgulD[ciulD - 1];
                rgulR[ciulU] = 0;
                int index = ciulU;
                uint iulX = (uint) (x_ulInt32Base / ((ulong) (num6 + 1L)));
                if (iulX > 1)
                {
                    MpMul1(rgulD, ref ciulD, iulX);
                    num6 = rgulD[ciulD - 1];
                    MpMul1(rgulR, ref ciulR, iulX);
                }
                uint num16 = rgulD[ciulD - 2];
                do
                {
                    uint num5;
                    ulong num = DWL(rgulR[index - 1], rgulR[index]);
                    if (num6 == rgulR[index])
                    {
                        num5 = (uint) (x_ulInt32Base - ((ulong) 1L));
                    }
                    else
                    {
                        num5 = (uint) (num / ((ulong) num6));
                    }
                    ulong num13 = num5;
                    uint hi = (uint) (num - (num13 * num6));
                    while ((num16 * num13) > DWL(rgulR[index - 2], hi))
                    {
                        num5--;
                        if (hi >= -num6)
                        {
                            break;
                        }
                        hi += num6;
                        num13 = num5;
                    }
                    num = x_ulInt32Base;
                    ulong num8 = 0L;
                    int num4 = 0;
                    int num2 = index - ciulD;
                    while (num4 < ciulD)
                    {
                        ulong num15 = rgulD[num4];
                        num8 += num5 * num15;
                        num += rgulR[num2] - LO(num8);
                        num8 = HI(num8);
                        rgulR[num2] = LO(num);
                        num = (HI(num) + x_ulInt32Base) - ((ulong) 1L);
                        num4++;
                        num2++;
                    }
                    num += rgulR[num2] - num8;
                    rgulR[num2] = LO(num);
                    rgulQ[index - ciulD] = num5;
                    if (HI(num) == 0)
                    {
                        rgulQ[index - ciulD] = num5 - 1;
                        uint num12 = 0;
                        num4 = 0;
                        num2 = index - ciulD;
                        while (num4 < ciulD)
                        {
                            num = (rgulD[num4] + rgulR[num2]) + num12;
                            num12 = HI(num);
                            rgulR[num2] = LO(num);
                            num4++;
                            num2++;
                        }
                        rgulR[num2] += num12;
                    }
                    index--;
                }
                while (index >= ciulD);
                MpNormalize(rgulQ, ref ciulQ);
                ciulR = ciulD;
                MpNormalize(rgulR, ref ciulR);
                if (iulX > 1)
                {
                    uint num20;
                    MpDiv1(rgulD, ref ciulD, iulX, out num20);
                    MpDiv1(rgulR, ref ciulR, iulX, out num20);
                }
            }
        }

        private EComparison CompareNm(SqlDecimal snumOp)
        {
            int num3;
            int num = this.IsPositive ? 1 : -1;
            int num6 = snumOp.IsPositive ? 1 : -1;
            if (num != num6)
            {
                if (num != 1)
                {
                    return EComparison.LT;
                }
                return EComparison.GT;
            }
            SqlDecimal num7 = this;
            SqlDecimal num5 = snumOp;
            int digits = this.m_bScale - snumOp.m_bScale;
            if (digits < 0)
            {
                try
                {
                    num7.AdjustScale(-digits, true);
                    goto Label_007A;
                }
                catch (OverflowException)
                {
                    return ((num > 0) ? EComparison.GT : EComparison.LT);
                }
            }
            if (digits > 0)
            {
                try
                {
                    num5.AdjustScale(digits, true);
                }
                catch (OverflowException)
                {
                    return ((num > 0) ? EComparison.LT : EComparison.GT);
                }
            }
        Label_007A:
            num3 = num7.LAbsCmp(num5);
            if (num3 == 0)
            {
                return EComparison.EQ;
            }
            int num4 = num * num3;
            if (num4 < 0)
            {
                return EComparison.LT;
            }
            return EComparison.GT;
        }

        private static void CheckValidPrecScale(byte bPrec, byte bScale)
        {
            if (((bPrec < 1) || (bPrec > MaxPrecision)) || (((bScale < 0) || (bScale > MaxScale)) || (bScale > bPrec)))
            {
                throw new SqlTypeException(SQLResource.InvalidPrecScaleMessage);
            }
        }

        private static void CheckValidPrecScale(int iPrec, int iScale)
        {
            if (((iPrec < 1) || (iPrec > MaxPrecision)) || (((iScale < 0) || (iScale > MaxScale)) || (iScale > iPrec)))
            {
                throw new SqlTypeException(SQLResource.InvalidPrecScaleMessage);
            }
        }

        public static SqlBoolean operator ==(SqlDecimal x, SqlDecimal y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.CompareNm(y) == EComparison.EQ);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlDecimal x, SqlDecimal y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlDecimal x, SqlDecimal y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.CompareNm(y) == EComparison.LT);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlDecimal x, SqlDecimal y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.CompareNm(y) == EComparison.GT);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return SqlBoolean.Null;
            }
            EComparison comparison = x.CompareNm(y);
            return new SqlBoolean((comparison == EComparison.LT) || (comparison == EComparison.EQ));
        }

        public static SqlBoolean operator >=(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return SqlBoolean.Null;
            }
            EComparison comparison = x.CompareNm(y);
            return new SqlBoolean((comparison == EComparison.GT) || (comparison == EComparison.EQ));
        }

        public static SqlDecimal Add(SqlDecimal x, SqlDecimal y)
        {
            return (x + y);
        }

        public static SqlDecimal Subtract(SqlDecimal x, SqlDecimal y)
        {
            return (x - y);
        }

        public static SqlDecimal Multiply(SqlDecimal x, SqlDecimal y)
        {
            return (x * y);
        }

        public static SqlDecimal Divide(SqlDecimal x, SqlDecimal y)
        {
            return (x / y);
        }

        public static SqlBoolean Equals(SqlDecimal x, SqlDecimal y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlDecimal x, SqlDecimal y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlDecimal x, SqlDecimal y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlDecimal x, SqlDecimal y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlDecimal x, SqlDecimal y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlDecimal x, SqlDecimal y)
        {
            return (x >= y);
        }

        public SqlBoolean ToSqlBoolean()
        {
            return (SqlBoolean) this;
        }

        public SqlByte ToSqlByte()
        {
            return (SqlByte) this;
        }

        public SqlDouble ToSqlDouble()
        {
            return this;
        }

        public SqlInt16 ToSqlInt16()
        {
            return (SqlInt16) this;
        }

        public SqlInt32 ToSqlInt32()
        {
            return (SqlInt32) this;
        }

        public SqlInt64 ToSqlInt64()
        {
            return (SqlInt64) this;
        }

        public SqlMoney ToSqlMoney()
        {
            return (SqlMoney) this;
        }

        public SqlSingle ToSqlSingle()
        {
            return this;
        }

        public SqlString ToSqlString()
        {
            return (SqlString) this;
        }

        private static char ChFromDigit(uint uiDigit)
        {
            return (char) (uiDigit + 0x30);
        }

        private void StoreFromWorkingArray(uint[] rguiData)
        {
            this.m_data1 = rguiData[0];
            this.m_data2 = rguiData[1];
            this.m_data3 = rguiData[2];
            this.m_data4 = rguiData[3];
        }

        private void SetToZero()
        {
            this.m_bLen = 1;
            this.m_data1 = this.m_data2 = this.m_data3 = this.m_data4 = 0;
            this.m_bStatus = (byte) (x_bNotNull | x_bPositive);
        }

        private void MakeInteger(out bool fFraction)
        {
            int bScale = this.m_bScale;
            fFraction = false;
            while (bScale > 0)
            {
                uint num2;
                if (bScale >= 9)
                {
                    num2 = this.DivByULong(x_rgulShiftBase[8]);
                    bScale -= 9;
                }
                else
                {
                    num2 = this.DivByULong(x_rgulShiftBase[bScale - 1]);
                    bScale = 0;
                }
                if (num2 != 0)
                {
                    fFraction = true;
                }
            }
            this.m_bScale = 0;
        }

        public static SqlDecimal Abs(SqlDecimal n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            n.SetPositive();
            return n;
        }

        public static SqlDecimal Ceiling(SqlDecimal n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            if (n.m_bScale != 0)
            {
                bool flag;
                n.MakeInteger(out flag);
                if (flag && n.IsPositive)
                {
                    n.AddULong(1);
                }
                if (n.FZero())
                {
                    n.SetPositive();
                }
            }
            return n;
        }

        public static SqlDecimal Floor(SqlDecimal n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            if (n.m_bScale != 0)
            {
                bool flag;
                n.MakeInteger(out flag);
                if (flag && !n.IsPositive)
                {
                    n.AddULong(1);
                }
                if (n.FZero())
                {
                    n.SetPositive();
                }
            }
            return n;
        }

        public static SqlInt32 Sign(SqlDecimal n)
        {
            if (n.IsNull)
            {
                return SqlInt32.Null;
            }
            if (SqlBoolean.op_True(n == new SqlDecimal(0)))
            {
                return SqlInt32.Zero;
            }
            if (n.IsNull)
            {
                return SqlInt32.Null;
            }
            if (!n.IsPositive)
            {
                return new SqlInt32(-1);
            }
            return new SqlInt32(1);
        }

        private static SqlDecimal Round(SqlDecimal n, int lPosition, bool fTruncate)
        {
            if (n.IsNull)
            {
                return Null;
            }
            if (lPosition >= 0)
            {
                lPosition = Math.Min(NUMERIC_MAX_PRECISION, lPosition);
                if (lPosition >= n.m_bScale)
                {
                    return n;
                }
            }
            else
            {
                lPosition = Math.Max(-NUMERIC_MAX_PRECISION, lPosition);
                if (lPosition < (n.m_bScale - n.m_bPrec))
                {
                    n.SetToZero();
                    return n;
                }
            }
            uint num2 = 0;
            int num = Math.Abs((int) (lPosition - n.m_bScale));
            uint num3 = 1;
            while (num > 0)
            {
                if (num >= 9)
                {
                    num2 = n.DivByULong(x_rgulShiftBase[8]);
                    num3 = x_rgulShiftBase[8];
                    num -= 9;
                }
                else
                {
                    num2 = n.DivByULong(x_rgulShiftBase[num - 1]);
                    num3 = x_rgulShiftBase[num - 1];
                    num = 0;
                }
            }
            if (num3 > 1)
            {
                num2 /= num3 / 10;
            }
            if (n.FZero() && (fTruncate || (num2 < 5)))
            {
                n.SetPositive();
                return n;
            }
            if ((num2 >= 5) && !fTruncate)
            {
                n.AddULong(1);
            }
            num = Math.Abs((int) (lPosition - n.m_bScale));
            while (num-- > 0)
            {
                n.MultByULong(x_ulBase10);
            }
            return n;
        }

        public static SqlDecimal Round(SqlDecimal n, int position)
        {
            return Round(n, position, false);
        }

        public static SqlDecimal Truncate(SqlDecimal n, int position)
        {
            return Round(n, position, true);
        }

        public static SqlDecimal Power(SqlDecimal n, double exp)
        {
            if (n.IsNull)
            {
                return Null;
            }
            byte precision = n.Precision;
            int scale = n.Scale;
            double x = n.ToDouble();
            n = new SqlDecimal(Math.Pow(x, exp));
            n.AdjustScale(scale - n.Scale, true);
            n.m_bPrec = MaxPrecision;
            return n;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlDecimal))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlDecimal));
            }
            SqlDecimal num = (SqlDecimal) value;
            return this.CompareTo(num);
        }

        public int CompareTo(SqlDecimal value)
        {
            if (this.IsNull)
            {
                if (!value.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (value.IsNull)
            {
                return 1;
            }
            if (SqlBoolean.op_True(this < value))
            {
                return -1;
            }
            if (SqlBoolean.op_True(this > value))
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (!(value is SqlDecimal))
            {
                return false;
            }
            SqlDecimal num = (SqlDecimal) value;
            if (num.IsNull || this.IsNull)
            {
                return (num.IsNull && this.IsNull);
            }
            SqlBoolean flag = this == num;
            return flag.Value;
        }

        public override int GetHashCode()
        {
            if (this.IsNull)
            {
                return 0;
            }
            SqlDecimal num6 = this;
            int num5 = num6.CalculatePrecision();
            num6.AdjustScale(NUMERIC_MAX_PRECISION - num5, true);
            int bLen = num6.m_bLen;
            int num = 0;
            int[] data = num6.Data;
            for (int i = 0; i < bLen; i++)
            {
                int num3 = (num >> 0x1c) & 0xff;
                num = num << 4;
                num = (num ^ data[i]) ^ num3;
            }
            return num;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            string attribute = reader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
            if ((attribute != null) && XmlConvert.ToBoolean(attribute))
            {
                reader.ReadElementString();
                this.m_bStatus = (byte) (x_bReverseNullMask & this.m_bStatus);
            }
            else
            {
                SqlDecimal num = Parse(reader.ReadElementString());
                this.m_bStatus = num.m_bStatus;
                this.m_bLen = num.m_bLen;
                this.m_bPrec = num.m_bPrec;
                this.m_bScale = num.m_bScale;
                this.m_data1 = num.m_data1;
                this.m_data2 = num.m_data2;
                this.m_data3 = num.m_data3;
                this.m_data4 = num.m_data4;
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.IsNull)
            {
                writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            }
            else
            {
                writer.WriteString(this.ToString());
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("decimal", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlDecimal()
        {
            NUMERIC_MAX_PRECISION = 0x26;
            MaxPrecision = NUMERIC_MAX_PRECISION;
            MaxScale = NUMERIC_MAX_PRECISION;
            x_bNullMask = 1;
            x_bIsNull = 0;
            x_bNotNull = 1;
            x_bReverseNullMask = ~x_bNullMask;
            x_bSignMask = 2;
            x_bPositive = 0;
            x_bNegative = 2;
            x_bReverseSignMask = ~x_bSignMask;
            x_uiZero = 0;
            x_cNumeMax = 4;
            x_lInt32Base = 0x100000000L;
            x_ulInt32Base = 0x100000000L;
            x_ulInt32BaseForMod = x_ulInt32Base - ((ulong) 1L);
            x_llMax = 0x7fffffffffffffffL;
            x_ulBase10 = 10;
            DUINT_BASE = x_lInt32Base;
            DUINT_BASE2 = DUINT_BASE * DUINT_BASE;
            DUINT_BASE3 = DUINT_BASE2 * DUINT_BASE;
            DMAX_NUME = 1E+38;
            DBL_DIG = 0x11;
            x_cNumeDivScaleMin = 6;
            x_rgulShiftBase = new uint[] { 10, 100, 0x3e8, 0x2710, 0x186a0, 0xf4240, 0x989680, 0x5f5e100, 0x3b9aca00 };
            DecimalHelpersLo = new uint[] { 
                10, 100, 0x3e8, 0x2710, 0x186a0, 0xf4240, 0x989680, 0x5f5e100, 0x3b9aca00, 0x540be400, 0x4876e800, 0xd4a51000, 0x4e72a000, 0x107a4000, 0xa4c68000, 0x6fc10000, 
                0x5d8a0000, 0xa7640000, 0x89e80000, 0x63100000, 0xdea00000, 0xb2400000, 0xf6800000, 0xa1000000, 0x4a000000, 0xe4000000, 0xe8000000, 0x10000000, 0xa0000000, 0x40000000, 0x80000000, 0, 
                0, 0, 0, 0, 0, 0
             };
            DecimalHelpersMid = new uint[] { 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0x17, 0xe8, 0x918, 0x5af3, 0x38d7e, 0x2386f2, 
                0x1634578, 0xde0b6b3, 0x8ac72304, 0x6bc75e2d, 0x35c9adc5, 0x19e0c9ba, 0x2c7e14a, 0x1bcecced, 0x16140148, 0xdcc80cd2, 0x9fd0803c, 0x3e250261, 0x6d7217ca, 0x4674edea, 0xc0914b26, 0x85acef81, 
                0x38c15b0a, 0x378d8e64, 0x2b878fe8, 0xb34b9f10, 0xf436a0, 0x98a2240
             };
            DecimalHelpersHi = new uint[] { 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 5, 0x36, 0x21e, 0x152d, 0xd3c2, 0x84595, 0x52b7d2, 0x33b2e3c, 0x204fce5e, 0x431e0fae, 0x9f2c9cd0, 0x37be2022, 0x2d6d415b, 
                0xc6448d93, 0xbead87c0, 0x72c74d82, 0x7bc90715, 0xd5da46d9, 0x5a86c47a
             };
            DecimalHelpersHiHi = new uint[] { 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 12, 0x7e, 0x4ee, 
                0x314d, 0x1ed09, 0x134261, 0xc097ce, 0x785ee10, 0x4b3b4ca8
             };
            rgCLenFromPrec = new byte[] { 
                1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 
                2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 
                4, 4, 4, 4, 4, 4
             };
            x_ulT1 = 10;
            x_ulT2 = 100;
            x_ulT3 = 0x3e8;
            x_ulT4 = 0x2710;
            x_ulT5 = 0x186a0;
            x_ulT6 = 0xf4240;
            x_ulT7 = 0x989680;
            x_ulT8 = 0x5f5e100;
            x_ulT9 = 0x3b9aca00;
            x_dwlT10 = 0x2540be400L;
            x_dwlT11 = 0x174876e800L;
            x_dwlT12 = 0xe8d4a51000L;
            x_dwlT13 = 0x9184e72a000L;
            x_dwlT14 = 0x5af3107a4000L;
            x_dwlT15 = 0x38d7ea4c68000L;
            x_dwlT16 = 0x2386f26fc10000L;
            x_dwlT17 = 0x16345785d8a0000L;
            x_dwlT18 = 0xde0b6b3a7640000L;
            x_dwlT19 = 10000000000000000000L;
            Null = new SqlDecimal(true);
            MinValue = Parse("-99999999999999999999999999999999999999");
            MaxValue = Parse("99999999999999999999999999999999999999");
        }
    }
}

