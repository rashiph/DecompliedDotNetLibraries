namespace System.Security.Cryptography
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    internal sealed class BigInt
    {
        private static readonly char[] decValues = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private const int m_base = 0x100;
        private byte[] m_elements;
        private const int m_maxbytes = 0x80;
        private int m_size;

        internal BigInt()
        {
            this.m_elements = new byte[0x80];
        }

        internal BigInt(byte b)
        {
            this.m_elements = new byte[0x80];
            this.SetDigit(0, b);
        }

        internal static void Add(BigInt a, byte b, ref BigInt c)
        {
            byte digit = b;
            int num2 = 0;
            int size = a.Size;
            int num4 = 0;
            for (int i = 0; i < size; i++)
            {
                num2 = a.GetDigit(i) + digit;
                c.SetDigit(i, (byte) (num2 & 0xff), ref num4);
                digit = (byte) ((num2 >> 8) & 0xff);
            }
            if (digit != 0)
            {
                c.SetDigit(a.Size, digit, ref num4);
            }
            c.Size = num4;
        }

        internal void Clear()
        {
            this.m_size = 0;
        }

        internal void CopyFrom(BigInt a)
        {
            Array.Copy(a.m_elements, this.m_elements, 0x80);
            this.m_size = a.m_size;
        }

        private void Divide(int b)
        {
            int num = 0;
            int num2 = 0;
            int size = this.Size;
            int num4 = 0;
            while (size-- > 0)
            {
                num2 = (0x100 * num) + this.GetDigit(size);
                num = num2 % b;
                this.SetDigit(size, (byte) (num2 / b), ref num4);
            }
            this.Size = num4;
        }

        internal static void Divide(BigInt numerator, BigInt denominator, ref BigInt quotient, ref BigInt remainder)
        {
            if (numerator < denominator)
            {
                quotient.Clear();
                remainder.CopyFrom(numerator);
            }
            else if (numerator == denominator)
            {
                quotient.Clear();
                quotient.SetDigit(0, 1);
                remainder.Clear();
            }
            else
            {
                BigInt a = new BigInt();
                a.CopyFrom(numerator);
                BigInt num2 = new BigInt();
                num2.CopyFrom(denominator);
                uint num3 = 0;
                while (num2.Size < a.Size)
                {
                    num2.Multiply(0x100);
                    num3++;
                }
                if (num2 > a)
                {
                    num2.Divide(0x100);
                    num3--;
                }
                int num4 = 0;
                int digit = 0;
                int b = 0;
                BigInt c = new BigInt();
                quotient.Clear();
                for (int i = 0; i <= num3; i++)
                {
                    num4 = (a.Size == num2.Size) ? a.GetDigit(a.Size - 1) : ((0x100 * a.GetDigit(a.Size - 1)) + a.GetDigit(a.Size - 2));
                    digit = num2.GetDigit(num2.Size - 1);
                    b = num4 / digit;
                    if (b >= 0x100)
                    {
                        b = 0xff;
                    }
                    Multiply(num2, b, ref c);
                    while (c > a)
                    {
                        b--;
                        Multiply(num2, b, ref c);
                    }
                    quotient.Multiply(0x100);
                    Add(quotient, (byte) b, ref quotient);
                    Subtract(a, c, ref a);
                    num2.Divide(0x100);
                }
                remainder.CopyFrom(a);
            }
        }

        public override bool Equals(object obj)
        {
            return ((obj is BigInt) && (this == ((BigInt) obj)));
        }

        internal void FromDecimal(string decNum)
        {
            BigInt a = new BigInt();
            BigInt c = new BigInt();
            int length = decNum.Length;
            for (int i = 0; i < length; i++)
            {
                if ((decNum[i] <= '9') && (decNum[i] >= '0'))
                {
                    Multiply(a, 10, ref c);
                    Add(c, (byte) (decNum[i] - '0'), ref a);
                }
            }
            this.CopyFrom(a);
        }

        internal void FromHexadecimal(string hexNum)
        {
            byte[] array = System.Security.Cryptography.X509Certificates.X509Utils.DecodeHexString(hexNum);
            Array.Reverse(array);
            int hexArraySize = System.Security.Cryptography.X509Certificates.X509Utils.GetHexArraySize(array);
            Array.Copy(array, this.m_elements, hexArraySize);
            this.Size = hexArraySize;
        }

        internal byte GetDigit(int index)
        {
            if ((index >= 0) && (index < this.m_size))
            {
                return this.m_elements[index];
            }
            return 0;
        }

        public override int GetHashCode()
        {
            int num = 0;
            for (int i = 0; i < this.m_size; i++)
            {
                num += this.GetDigit(i);
            }
            return num;
        }

        internal bool IsZero()
        {
            for (int i = 0; i < this.m_size; i++)
            {
                if (this.m_elements[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void Multiply(int b)
        {
            if (b == 0)
            {
                this.Clear();
            }
            else
            {
                int num = 0;
                int num2 = 0;
                int size = this.Size;
                int num4 = 0;
                for (int i = 0; i < size; i++)
                {
                    num2 = (b * this.GetDigit(i)) + num;
                    num = num2 / 0x100;
                    this.SetDigit(i, (byte) (num2 % 0x100), ref num4);
                }
                if (num != 0)
                {
                    byte[] bytes = BitConverter.GetBytes(num);
                    for (int j = 0; j < bytes.Length; j++)
                    {
                        this.SetDigit(size + j, bytes[j], ref num4);
                    }
                }
                this.Size = num4;
            }
        }

        private static void Multiply(BigInt a, int b, ref BigInt c)
        {
            if (b == 0)
            {
                c.Clear();
            }
            else
            {
                int num = 0;
                int num2 = 0;
                int size = a.Size;
                int num4 = 0;
                for (int i = 0; i < size; i++)
                {
                    num2 = (b * a.GetDigit(i)) + num;
                    num = num2 / 0x100;
                    c.SetDigit(i, (byte) (num2 % 0x100), ref num4);
                }
                if (num != 0)
                {
                    byte[] bytes = BitConverter.GetBytes(num);
                    for (int j = 0; j < bytes.Length; j++)
                    {
                        c.SetDigit(size + j, bytes[j], ref num4);
                    }
                }
                c.Size = num4;
            }
        }

        internal static void Negate(ref BigInt a)
        {
            int size = 0;
            for (int i = 0; i < 0x80; i++)
            {
                a.SetDigit(i, (byte) (~a.GetDigit(i) & 0xff), ref size);
            }
            for (int j = 0; j < 0x80; j++)
            {
                a.SetDigit(j, (byte) (a.GetDigit(j) + 1), ref size);
                if ((a.GetDigit(j) & 0xff) != 0)
                {
                    break;
                }
                a.SetDigit(j, (byte) (a.GetDigit(j) & 0xff), ref size);
            }
            a.Size = size;
        }

        public static bool operator ==(BigInt value1, BigInt value2)
        {
            if (value1 == null)
            {
                return (value2 == null);
            }
            if (value2 == null)
            {
                return (value1 == null);
            }
            int size = value1.Size;
            int num2 = value2.Size;
            if (size != num2)
            {
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (value1.m_elements[i] != value2.m_elements[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator >(BigInt value1, BigInt value2)
        {
            if (value1 == null)
            {
                return false;
            }
            if (value2 == null)
            {
                return true;
            }
            int size = value1.Size;
            int num2 = value2.Size;
            if (size == num2)
            {
                while (size-- > 0)
                {
                    if (value1.m_elements[size] != value2.m_elements[size])
                    {
                        return (value1.m_elements[size] > value2.m_elements[size]);
                    }
                }
                return false;
            }
            return (size > num2);
        }

        public static bool operator !=(BigInt value1, BigInt value2)
        {
            return !(value1 == value2);
        }

        public static bool operator <(BigInt value1, BigInt value2)
        {
            if (value1 == null)
            {
                return true;
            }
            if (value2 == null)
            {
                return false;
            }
            int size = value1.Size;
            int num2 = value2.Size;
            if (size == num2)
            {
                while (size-- > 0)
                {
                    if (value1.m_elements[size] != value2.m_elements[size])
                    {
                        return (value1.m_elements[size] < value2.m_elements[size]);
                    }
                }
                return false;
            }
            return (size < num2);
        }

        internal void SetDigit(int index, byte digit)
        {
            if ((index >= 0) && (index < 0x80))
            {
                this.m_elements[index] = digit;
                if ((index >= this.m_size) && (digit != 0))
                {
                    this.m_size = index + 1;
                }
                if ((index == (this.m_size - 1)) && (digit == 0))
                {
                    this.m_size--;
                }
            }
        }

        internal void SetDigit(int index, byte digit, ref int size)
        {
            if ((index >= 0) && (index < 0x80))
            {
                this.m_elements[index] = digit;
                if ((index >= size) && (digit != 0))
                {
                    size = index + 1;
                }
                if ((index == (size - 1)) && (digit == 0))
                {
                    size--;
                }
            }
        }

        internal static void Subtract(BigInt a, BigInt b, ref BigInt c)
        {
            byte num = 0;
            int num2 = 0;
            if (a < b)
            {
                Subtract(b, a, ref c);
                Negate(ref c);
            }
            else
            {
                int index = 0;
                int size = a.Size;
                int num5 = 0;
                for (index = 0; index < size; index++)
                {
                    num2 = (a.GetDigit(index) - b.GetDigit(index)) - num;
                    num = 0;
                    if (num2 < 0)
                    {
                        num2 += 0x100;
                        num = 1;
                    }
                    c.SetDigit(index, (byte) (num2 & 0xff), ref num5);
                }
                c.Size = num5;
            }
        }

        internal byte[] ToByteArray()
        {
            byte[] destinationArray = new byte[this.Size];
            Array.Copy(this.m_elements, destinationArray, this.Size);
            return destinationArray;
        }

        internal string ToDecimal()
        {
            if (this.IsZero())
            {
                return "0";
            }
            BigInt denominator = new BigInt(10);
            BigInt numerator = new BigInt();
            BigInt quotient = new BigInt();
            BigInt remainder = new BigInt();
            numerator.CopyFrom(this);
            char[] array = new char[(int) Math.Ceiling((double) ((this.m_size * 2) * 1.21))];
            int length = 0;
            do
            {
                Divide(numerator, denominator, ref quotient, ref remainder);
                array[length++] = decValues[remainder.IsZero() ? 0 : remainder.m_elements[0]];
                numerator.CopyFrom(quotient);
            }
            while (!quotient.IsZero());
            Array.Reverse(array, 0, length);
            return new string(array, 0, length);
        }

        internal int Size
        {
            get
            {
                return this.m_size;
            }
            set
            {
                if (value > 0x80)
                {
                    this.m_size = 0x80;
                }
                if (value < 0)
                {
                    this.m_size = 0;
                }
                this.m_size = value;
            }
        }
    }
}

