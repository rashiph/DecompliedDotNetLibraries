namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security;

    internal sealed class FastResourceComparer : IComparer, IEqualityComparer, IComparer<string>, IEqualityComparer<string>
    {
        internal static readonly FastResourceComparer Default = new FastResourceComparer();

        public int Compare(object a, object b)
        {
            if (a == b)
            {
                return 0;
            }
            string strA = (string) a;
            string strB = (string) b;
            return string.CompareOrdinal(strA, strB);
        }

        public int Compare(string a, string b)
        {
            return string.CompareOrdinal(a, b);
        }

        [SecurityCritical]
        public static unsafe int CompareOrdinal(string a, byte[] bytes, int bCharLength)
        {
            int num = 0;
            int num2 = 0;
            int length = a.Length;
            if (length > bCharLength)
            {
                length = bCharLength;
            }
            if (bCharLength == 0)
            {
                if (a.Length != 0)
                {
                    return -1;
                }
                return 0;
            }
            fixed (byte* numRef = bytes)
            {
                for (byte* numPtr = numRef; (num < length) && (num2 == 0); numPtr += 2)
                {
                    int num4 = numPtr[0] | (numPtr[1] << 8);
                    num2 = a[num++] - num4;
                }
            }
            if (num2 != 0)
            {
                return num2;
            }
            return (a.Length - bCharLength);
        }

        [SecurityCritical]
        public static int CompareOrdinal(byte[] bytes, int aCharLength, string b)
        {
            return -CompareOrdinal(b, bytes, aCharLength);
        }

        [SecurityCritical]
        internal static unsafe int CompareOrdinal(byte* a, int byteLen, string b)
        {
            int num = 0;
            int num2 = 0;
            int length = byteLen >> 1;
            if (length > b.Length)
            {
                length = b.Length;
            }
            while ((num2 < length) && (num == 0))
            {
                a++;
                a++;
                char ch = (char) (a[0] | (a[0] << 8));
                num = ch - b[num2++];
            }
            if (num != 0)
            {
                return num;
            }
            return (byteLen - (b.Length * 2));
        }

        public bool Equals(object a, object b)
        {
            if (a == b)
            {
                return true;
            }
            string str = (string) a;
            string str2 = (string) b;
            return string.Equals(str, str2);
        }

        public bool Equals(string a, string b)
        {
            return string.Equals(a, b);
        }

        public int GetHashCode(object key)
        {
            string str = (string) key;
            return HashFunction(str);
        }

        public int GetHashCode(string key)
        {
            return HashFunction(key);
        }

        internal static int HashFunction(string key)
        {
            uint num = 0x1505;
            for (int i = 0; i < key.Length; i++)
            {
                num = ((num << 5) + num) ^ key[i];
            }
            return (int) num;
        }
    }
}

