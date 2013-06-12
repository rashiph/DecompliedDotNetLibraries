namespace System.Collections.Specialized
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    public struct BitVector32
    {
        private uint data;
        public BitVector32(int data)
        {
            this.data = (uint) data;
        }

        public BitVector32(BitVector32 value)
        {
            this.data = value.data;
        }

        public bool this[int bit]
        {
            get
            {
                return ((this.data & bit) == ((ulong) bit));
            }
            set
            {
                if (value)
                {
                    this.data |= (uint) bit;
                }
                else
                {
                    this.data &= (uint) ~bit;
                }
            }
        }
        public int this[Section section]
        {
            get
            {
                return (int) ((this.data & (section.Mask << (section.Offset & 0x1f))) >> (section.Offset & 0x1f));
            }
            set
            {
                value = value << section.Offset;
                int num = (0xffff & section.Mask) << section.Offset;
                this.data = (uint) ((this.data & ~num) | (value & num));
            }
        }
        public int Data
        {
            get
            {
                return (int) this.data;
            }
        }
        private static short CountBitsSet(short mask)
        {
            short num = 0;
            while ((mask & 1) != 0)
            {
                num = (short) (num + 1);
                mask = (short) (mask >> 1);
            }
            return num;
        }

        public static int CreateMask()
        {
            return CreateMask(0);
        }

        public static int CreateMask(int previous)
        {
            if (previous == 0)
            {
                return 1;
            }
            if (previous == -2147483648)
            {
                throw new InvalidOperationException(SR.GetString("BitVectorFull"));
            }
            return (previous << 1);
        }

        private static short CreateMaskFromHighValue(short highValue)
        {
            short num = 0x10;
            while ((highValue & 0x8000) == 0)
            {
                num = (short) (num - 1);
                highValue = (short) (highValue << 1);
            }
            ushort num2 = 0;
            while (num > 0)
            {
                num = (short) (num - 1);
                num2 = (ushort) (num2 << 1);
                num2 = (ushort) (num2 | 1);
            }
            return (short) num2;
        }

        public static Section CreateSection(short maxValue)
        {
            return CreateSectionHelper(maxValue, 0, 0);
        }

        public static Section CreateSection(short maxValue, Section previous)
        {
            return CreateSectionHelper(maxValue, previous.Mask, previous.Offset);
        }

        private static Section CreateSectionHelper(short maxValue, short priorMask, short priorOffset)
        {
            if (maxValue < 1)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidValue", new object[] { "maxValue", 0 }), "maxValue");
            }
            short offset = (short) (priorOffset + CountBitsSet(priorMask));
            if (offset >= 0x20)
            {
                throw new InvalidOperationException(SR.GetString("BitVectorFull"));
            }
            return new Section(CreateMaskFromHighValue(maxValue), offset);
        }

        public override bool Equals(object o)
        {
            return ((o is BitVector32) && (this.data == ((BitVector32) o).data));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static string ToString(BitVector32 value)
        {
            StringBuilder builder = new StringBuilder(0x2d);
            builder.Append("BitVector32{");
            int data = (int) value.data;
            for (int i = 0; i < 0x20; i++)
            {
                if ((data & 0x80000000L) != 0L)
                {
                    builder.Append("1");
                }
                else
                {
                    builder.Append("0");
                }
                data = data << 1;
            }
            builder.Append("}");
            return builder.ToString();
        }

        public override string ToString()
        {
            return ToString(this);
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Section
        {
            private readonly short mask;
            private readonly short offset;
            internal Section(short mask, short offset)
            {
                this.mask = mask;
                this.offset = offset;
            }

            public short Mask
            {
                get
                {
                    return this.mask;
                }
            }
            public short Offset
            {
                get
                {
                    return this.offset;
                }
            }
            public override bool Equals(object o)
            {
                return ((o is BitVector32.Section) && this.Equals((BitVector32.Section) o));
            }

            public bool Equals(BitVector32.Section obj)
            {
                return ((obj.mask == this.mask) && (obj.offset == this.offset));
            }

            public static bool operator ==(BitVector32.Section a, BitVector32.Section b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(BitVector32.Section a, BitVector32.Section b)
            {
                return !(a == b);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static string ToString(BitVector32.Section value)
            {
                return ("Section{0x" + Convert.ToString(value.Mask, 0x10) + ", 0x" + Convert.ToString(value.Offset, 0x10) + "}");
            }

            public override string ToString()
            {
                return ToString(this);
            }
        }
    }
}

