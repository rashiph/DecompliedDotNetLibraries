namespace System.Xml
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BinXmlSqlDecimal
    {
        internal byte m_bLen;
        internal byte m_bPrec;
        internal byte m_bScale;
        internal byte m_bSign;
        internal uint m_data1;
        internal uint m_data2;
        internal uint m_data3;
        internal uint m_data4;
        private static readonly byte NUMERIC_MAX_PRECISION;
        private static readonly byte MaxPrecision;
        private static readonly byte MaxScale;
        private static readonly int x_cNumeMax;
        private static readonly long x_lInt32Base;
        private static readonly ulong x_ulInt32Base;
        private static readonly ulong x_ulInt32BaseForMod;
        internal static readonly ulong x_llMax;
        private static readonly double DUINT_BASE;
        private static readonly double DUINT_BASE2;
        private static readonly double DUINT_BASE3;
        private static readonly uint[] x_rgulShiftBase;
        private static readonly byte[] rgCLenFromPrec;
        public bool IsPositive
        {
            get
            {
                return (this.m_bSign == 0);
            }
        }
        public BinXmlSqlDecimal(byte[] data, int offset, bool trim)
        {
            switch (data[offset])
            {
                case 15:
                    this.m_bLen = 3;
                    break;

                case 0x13:
                    this.m_bLen = 4;
                    break;

                case 7:
                    this.m_bLen = 1;
                    break;

                case 11:
                    this.m_bLen = 2;
                    break;

                default:
                    throw new XmlException("XmlBinary_InvalidSqlDecimal", null);
            }
            this.m_bPrec = data[offset + 1];
            this.m_bScale = data[offset + 2];
            this.m_bSign = (data[offset + 3] == 0) ? ((byte) 1) : ((byte) 0);
            this.m_data1 = UIntFromByteArray(data, offset + 4);
            this.m_data2 = (this.m_bLen > 1) ? UIntFromByteArray(data, offset + 8) : 0;
            this.m_data3 = (this.m_bLen > 2) ? UIntFromByteArray(data, offset + 12) : 0;
            this.m_data4 = (this.m_bLen > 3) ? UIntFromByteArray(data, offset + 0x10) : 0;
            if ((this.m_bLen == 4) && (this.m_data4 == 0))
            {
                this.m_bLen = 3;
            }
            if ((this.m_bLen == 3) && (this.m_data3 == 0))
            {
                this.m_bLen = 2;
            }
            if ((this.m_bLen == 2) && (this.m_data2 == 0))
            {
                this.m_bLen = 1;
            }
            if (trim)
            {
                this.TrimTrailingZeros();
            }
        }

        public void Write(Stream strm)
        {
            strm.WriteByte((byte) ((this.m_bLen * 4) + 3));
            strm.WriteByte(this.m_bPrec);
            strm.WriteByte(this.m_bScale);
            strm.WriteByte((this.m_bSign == 0) ? ((byte) 1) : ((byte) 0));
            this.WriteUI4(this.m_data1, strm);
            if (this.m_bLen > 1)
            {
                this.WriteUI4(this.m_data2, strm);
                if (this.m_bLen > 2)
                {
                    this.WriteUI4(this.m_data3, strm);
                    if (this.m_bLen > 3)
                    {
                        this.WriteUI4(this.m_data4, strm);
                    }
                }
            }
        }

        private void WriteUI4(uint val, Stream strm)
        {
            strm.WriteByte((byte) (val & 0xff));
            strm.WriteByte((byte) ((val >> 8) & 0xff));
            strm.WriteByte((byte) ((val >> 0x10) & 0xff));
            strm.WriteByte((byte) ((val >> 0x18) & 0xff));
        }

        private static uint UIntFromByteArray(byte[] data, int offset)
        {
            int num = data[offset];
            num |= data[offset + 1] << 8;
            num |= data[offset + 2] << 0x10;
            num |= data[offset + 3] << 0x18;
            return (uint) num;
        }

        private bool FZero()
        {
            return ((this.m_data1 == 0) && (this.m_bLen <= 1));
        }

        private void StoreFromWorkingArray(uint[] rguiData)
        {
            this.m_data1 = rguiData[0];
            this.m_data2 = rguiData[1];
            this.m_data3 = rguiData[2];
            this.m_data4 = rguiData[3];
        }

        private bool FGt10_38(uint[] rglData)
        {
            if (rglData[3] < 0x4b3b4ca8L)
            {
                return false;
            }
            return (((rglData[3] > 0x4b3b4ca8L) || (rglData[2] > 0x5a86c47aL)) || ((rglData[2] == 0x5a86c47aL) && (rglData[1] >= 0x98a2240L)));
        }

        private static void MpDiv1(uint[] rgulU, ref int ciulU, uint iulD, out uint iulR)
        {
            uint num = 0;
            ulong num3 = iulD;
            int index = ciulU;
            while (index > 0)
            {
                index--;
                ulong num2 = (num << 0x20) + rgulU[index];
                rgulU[index] = (uint) (num2 / num3);
                num = (uint) (num2 - (rgulU[index] * num3));
            }
            iulR = num;
            MpNormalize(rgulU, ref ciulU);
        }

        private static void MpNormalize(uint[] rgulU, ref int ciulU)
        {
            while ((ciulU > 1) && (rgulU[ciulU - 1] == 0))
            {
                ciulU--;
            }
        }

        internal void AdjustScale(int digits, bool fRound)
        {
            uint num2;
            bool flag = false;
            int num5 = digits;
            if ((num5 + this.m_bScale) < 0)
            {
                throw new XmlException("SqlTypes_ArithTruncation", null);
            }
            if ((num5 + this.m_bScale) > NUMERIC_MAX_PRECISION)
            {
                throw new XmlException("SqlTypes_ArithOverflow", null);
            }
            byte num3 = (byte) (num5 + this.m_bScale);
            byte num4 = (byte) Math.Min(NUMERIC_MAX_PRECISION, Math.Max(1, num5 + this.m_bPrec));
            if (num5 > 0)
            {
                this.m_bScale = num3;
                this.m_bPrec = num4;
                while (num5 > 0)
                {
                    if (num5 >= 9)
                    {
                        num2 = x_rgulShiftBase[8];
                        num5 -= 9;
                    }
                    else
                    {
                        num2 = x_rgulShiftBase[num5 - 1];
                        num5 = 0;
                    }
                    this.MultByULong(num2);
                }
            }
            else if (num5 < 0)
            {
                uint num;
                do
                {
                    if (num5 <= -9)
                    {
                        num2 = x_rgulShiftBase[8];
                        num5 += 9;
                    }
                    else
                    {
                        num2 = x_rgulShiftBase[-num5 - 1];
                        num5 = 0;
                    }
                    num = this.DivByULong(num2);
                }
                while (num5 < 0);
                flag = num >= (num2 / 2);
                this.m_bScale = num3;
                this.m_bPrec = num4;
            }
            if (flag && fRound)
            {
                this.AddULong(1);
            }
            else if (this.FZero())
            {
                this.m_bSign = 0;
            }
        }

        private void AddULong(uint ulAdd)
        {
            ulong num = ulAdd;
            int bLen = this.m_bLen;
            uint[] rguiData = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            int index = 0;
        Label_003F:
            num += rguiData[index];
            rguiData[index] = (uint) num;
            num = num >> 0x20;
            if (0L == num)
            {
                this.StoreFromWorkingArray(rguiData);
            }
            else
            {
                index++;
                if (index < bLen)
                {
                    goto Label_003F;
                }
                if (index == x_cNumeMax)
                {
                    throw new XmlException("SqlTypes_ArithOverflow", null);
                }
                rguiData[index] = (uint) num;
                this.m_bLen = (byte) (this.m_bLen + 1);
                if (this.FGt10_38(rguiData))
                {
                    throw new XmlException("SqlTypes_ArithOverflow", null);
                }
                this.StoreFromWorkingArray(rguiData);
            }
        }

        private void MultByULong(uint uiMultiplier)
        {
            int bLen = this.m_bLen;
            ulong num2 = 0L;
            ulong num3 = 0L;
            uint[] rglData = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            for (int i = 0; i < bLen; i++)
            {
                ulong num5 = rglData[i];
                num3 = num5 * uiMultiplier;
                num2 += num3;
                if (num2 < num3)
                {
                    num3 = x_ulInt32Base;
                }
                else
                {
                    num3 = 0L;
                }
                rglData[i] = (uint) num2;
                num2 = (num2 >> 0x20) + num3;
            }
            if (num2 != 0L)
            {
                if (bLen == x_cNumeMax)
                {
                    throw new XmlException("SqlTypes_ArithOverflow", null);
                }
                rglData[bLen] = (uint) num2;
                this.m_bLen = (byte) (this.m_bLen + 1);
            }
            if (this.FGt10_38(rglData))
            {
                throw new XmlException("SqlTypes_ArithOverflow", null);
            }
            this.StoreFromWorkingArray(rglData);
        }

        internal uint DivByULong(uint iDivisor)
        {
            ulong num = iDivisor;
            ulong num2 = 0L;
            uint num3 = 0;
            bool flag = true;
            if (num == 0L)
            {
                throw new XmlException("SqlTypes_DivideByZero", null);
            }
            uint[] rguiData = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            for (int i = this.m_bLen; i > 0; i--)
            {
                num2 = (num2 << 0x20) + rguiData[i - 1];
                num3 = (uint) (num2 / num);
                rguiData[i - 1] = num3;
                num2 = num2 % num;
                flag = flag && (num3 == 0);
                if (flag)
                {
                    this.m_bLen = (byte) (this.m_bLen - 1);
                }
            }
            this.StoreFromWorkingArray(rguiData);
            if (flag)
            {
                this.m_bLen = 1;
            }
            return (uint) num2;
        }

        private static byte CLenFromPrec(byte bPrec)
        {
            return rgCLenFromPrec[bPrec - 1];
        }

        private static char ChFromDigit(uint uiDigit)
        {
            return (char) (uiDigit + 0x30);
        }

        public decimal ToDecimal()
        {
            if ((this.m_data4 != 0) || (this.m_bScale > 0x1c))
            {
                throw new XmlException("SqlTypes_ArithOverflow", null);
            }
            return new decimal((int) this.m_data1, (int) this.m_data2, (int) this.m_data3, !this.IsPositive, this.m_bScale);
        }

        private void TrimTrailingZeros()
        {
            uint[] rgulU = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            int bLen = this.m_bLen;
            if ((bLen != 1) || (rgulU[0] != 0))
            {
                while ((this.m_bScale > 0) && ((bLen > 1) || (rgulU[0] != 0)))
                {
                    uint num2;
                    MpDiv1(rgulU, ref bLen, 10, out num2);
                    if (num2 != 0)
                    {
                        break;
                    }
                    this.m_data1 = rgulU[0];
                    this.m_data2 = rgulU[1];
                    this.m_data3 = rgulU[2];
                    this.m_data4 = rgulU[3];
                    this.m_bScale = (byte) (this.m_bScale - 1);
                }
            }
            else
            {
                this.m_bScale = 0;
                return;
            }
            if ((this.m_bLen == 4) && (this.m_data4 == 0))
            {
                this.m_bLen = 3;
            }
            if ((this.m_bLen == 3) && (this.m_data3 == 0))
            {
                this.m_bLen = 2;
            }
            if ((this.m_bLen == 2) && (this.m_data2 == 0))
            {
                this.m_bLen = 1;
            }
        }

        public override string ToString()
        {
            uint[] rgulU = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            int bLen = this.m_bLen;
            char[] chArray = new char[NUMERIC_MAX_PRECISION + 1];
            int index = 0;
            while ((bLen > 1) || (rgulU[0] != 0))
            {
                uint num3;
                MpDiv1(rgulU, ref bLen, 10, out num3);
                chArray[index++] = ChFromDigit(num3);
            }
            while (index <= this.m_bScale)
            {
                chArray[index++] = ChFromDigit(0);
            }
            bool isPositive = this.IsPositive;
            int num4 = isPositive ? index : (index + 1);
            if (this.m_bScale > 0)
            {
                num4++;
            }
            char[] chArray2 = new char[num4];
            int num5 = 0;
            if (!isPositive)
            {
                chArray2[num5++] = '-';
            }
            while (index > 0)
            {
                if (index-- == this.m_bScale)
                {
                    chArray2[num5++] = '.';
                }
                chArray2[num5++] = chArray[index];
            }
            return new string(chArray2);
        }

        [Conditional("DEBUG")]
        private void AssertValid()
        {
            uint[] numArray = new uint[] { this.m_data1, this.m_data2, this.m_data3, this.m_data4 };
            uint num1 = numArray[this.m_bLen - 1];
            for (int i = this.m_bLen; i < x_cNumeMax; i++)
            {
            }
        }

        static BinXmlSqlDecimal()
        {
            NUMERIC_MAX_PRECISION = 0x26;
            MaxPrecision = NUMERIC_MAX_PRECISION;
            MaxScale = NUMERIC_MAX_PRECISION;
            x_cNumeMax = 4;
            x_lInt32Base = 0x100000000L;
            x_ulInt32Base = 0x100000000L;
            x_ulInt32BaseForMod = x_ulInt32Base - ((ulong) 1L);
            x_llMax = 0x7fffffffffffffffL;
            DUINT_BASE = x_lInt32Base;
            DUINT_BASE2 = DUINT_BASE * DUINT_BASE;
            DUINT_BASE3 = DUINT_BASE2 * DUINT_BASE;
            x_rgulShiftBase = new uint[] { 10, 100, 0x3e8, 0x2710, 0x186a0, 0xf4240, 0x989680, 0x5f5e100, 0x3b9aca00 };
            rgCLenFromPrec = new byte[] { 
                1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 
                2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 
                4, 4, 4, 4, 4, 4
             };
        }
    }
}

