namespace System.Runtime.Caching
{
    using System;
    using System.Collections;

    internal class MemoryCacheEqualityComparer : IEqualityComparer
    {
        bool IEqualityComparer.Equals(object x, object y)
        {
            MemoryCacheKey key = (MemoryCacheKey) x;
            MemoryCacheKey key2 = (MemoryCacheKey) y;
            return (string.Compare(key.Key, key2.Key, StringComparison.Ordinal) == 0);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            MemoryCacheKey key = (MemoryCacheKey) obj;
            return key.Hash;
        }
    }
}

