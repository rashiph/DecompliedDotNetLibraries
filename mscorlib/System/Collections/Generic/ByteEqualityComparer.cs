namespace System.Collections.Generic
{
    using System;
    using System.Security;

    [Serializable]
    internal class ByteEqualityComparer : EqualityComparer<byte>
    {
        public override bool Equals(object obj)
        {
            ByteEqualityComparer comparer = obj as ByteEqualityComparer;
            return (comparer != null);
        }

        public override bool Equals(byte x, byte y)
        {
            return (x == y);
        }

        public override int GetHashCode()
        {
            return base.GetType().Name.GetHashCode();
        }

        public override int GetHashCode(byte b)
        {
            return b.GetHashCode();
        }

        [SecuritySafeCritical]
        internal override unsafe int IndexOf(byte[] array, byte value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            if (count > (array.Length - startIndex))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (count == 0)
            {
                return -1;
            }
            fixed (byte* numRef = array)
            {
                return Buffer.IndexOfByte(numRef, value, startIndex, count);
            }
        }

        internal override int LastIndexOf(byte[] array, byte value, int startIndex, int count)
        {
            int num = (startIndex - count) + 1;
            for (int i = startIndex; i >= num; i--)
            {
                if (array[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}

