namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal class SecureStringHasher : IEqualityComparer<string>
    {
        private int hashCodeRandomizer = Environment.TickCount;

        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.Ordinal);
        }

        public int GetHashCode(string key)
        {
            int hashCodeRandomizer = this.hashCodeRandomizer;
            for (int i = 0; i < key.Length; i++)
            {
                hashCodeRandomizer += (hashCodeRandomizer << 7) ^ key[i];
            }
            hashCodeRandomizer -= hashCodeRandomizer >> 0x11;
            hashCodeRandomizer -= hashCodeRandomizer >> 11;
            return (hashCodeRandomizer - (hashCodeRandomizer >> 5));
        }
    }
}

