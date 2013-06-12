namespace System.Xml.Schema
{
    using System;
    using System.Reflection;

    internal sealed class BitSet
    {
        private uint[] bits;
        private const int bitSlotMask = 0x1f;
        private const int bitSlotShift = 5;
        private int count;

        private BitSet()
        {
        }

        public BitSet(int count)
        {
            this.count = count;
            this.bits = new uint[this.Subscript(count + 0x1f)];
        }

        public void And(BitSet other)
        {
            if (this != other)
            {
                int length = this.bits.Length;
                int num2 = other.bits.Length;
                int index = (length > num2) ? num2 : length;
                int num4 = index;
                while (num4-- > 0)
                {
                    this.bits[num4] &= other.bits[num4];
                }
                while (index < length)
                {
                    this.bits[index] = 0;
                    index++;
                }
            }
        }

        public void Clear()
        {
            int index = this.bits.Length;
            while (index-- > 0)
            {
                this.bits[index] = 0;
            }
        }

        public void Clear(int index)
        {
            int num = this.Subscript(index);
            this.EnsureLength(num + 1);
            this.bits[num] &= (uint) ~(((int) 1) << index);
        }

        public BitSet Clone()
        {
            return new BitSet { count = this.count, bits = (uint[]) this.bits.Clone() };
        }

        private void EnsureLength(int nRequiredLength)
        {
            if (nRequiredLength > this.bits.Length)
            {
                int num = 2 * this.bits.Length;
                if (num < nRequiredLength)
                {
                    num = nRequiredLength;
                }
                uint[] destinationArray = new uint[num];
                Array.Copy(this.bits, destinationArray, this.bits.Length);
                this.bits = destinationArray;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (this != obj)
            {
                BitSet set = (BitSet) obj;
                int length = this.bits.Length;
                int num2 = set.bits.Length;
                int num3 = (length > num2) ? num2 : length;
                int index = num3;
                while (index-- > 0)
                {
                    if (this.bits[index] != set.bits[index])
                    {
                        return false;
                    }
                }
                if (length > num3)
                {
                    int num5 = length;
                    while (num5-- > num3)
                    {
                        if (this.bits[num5] != 0)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    int num6 = num2;
                    while (num6-- > num3)
                    {
                        if (set.bits[num6] != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool Get(int index)
        {
            bool flag = false;
            if (index < this.count)
            {
                int num = this.Subscript(index);
                flag = (this.bits[num] & (((int) 1) << index)) != 0L;
            }
            return flag;
        }

        public override int GetHashCode()
        {
            int num = 0x4d2;
            int length = this.bits.Length;
            while (--length >= 0)
            {
                num ^= (int) (this.bits[length] * (length + 1));
            }
            return (num ^ num);
        }

        public bool Intersects(BitSet other)
        {
            int index = Math.Min(this.bits.Length, other.bits.Length);
            while (--index >= 0)
            {
                if ((this.bits[index] & other.bits[index]) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public int NextSet(int startFrom)
        {
            int bitIndex = startFrom + 1;
            if (bitIndex == this.count)
            {
                return -1;
            }
            int index = this.Subscript(bitIndex);
            bitIndex &= 0x1f;
            uint num3 = this.bits[index] >> bitIndex;
            while (num3 == 0)
            {
                if (++index == this.bits.Length)
                {
                    return -1;
                }
                bitIndex = 0;
                num3 = this.bits[index];
            }
            while ((num3 & 1) == 0)
            {
                num3 = num3 >> 1;
                bitIndex++;
            }
            return ((index << 5) + bitIndex);
        }

        public void Or(BitSet other)
        {
            if (this != other)
            {
                int length = other.bits.Length;
                this.EnsureLength(length);
                int index = length;
                while (index-- > 0)
                {
                    this.bits[index] |= other.bits[index];
                }
            }
        }

        public void Set(int index)
        {
            int num = this.Subscript(index);
            this.EnsureLength(num + 1);
            this.bits[num] |= ((uint) 1) << index;
        }

        private int Subscript(int bitIndex)
        {
            return (bitIndex >> 5);
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                uint num = 0;
                for (int i = 0; i < this.bits.Length; i++)
                {
                    num |= this.bits[i];
                }
                return (num == 0);
            }
        }

        public bool this[int index]
        {
            get
            {
                return this.Get(index);
            }
        }
    }
}

