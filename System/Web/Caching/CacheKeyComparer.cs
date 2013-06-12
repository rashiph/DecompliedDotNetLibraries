namespace System.Web.Caching
{
    using System;
    using System.Collections;

    internal sealed class CacheKeyComparer : IEqualityComparer
    {
        private static CacheKeyComparer s_comparerInstance;

        private CacheKeyComparer()
        {
        }

        private int Compare(object x, object y)
        {
            CacheKey key = (CacheKey) x;
            CacheKey key2 = (CacheKey) y;
            if (key.IsPublic)
            {
                if (key2.IsPublic)
                {
                    return string.Compare(key.Key, key2.Key, StringComparison.Ordinal);
                }
                return 1;
            }
            if (!key2.IsPublic)
            {
                return string.Compare(key.Key, key2.Key, StringComparison.Ordinal);
            }
            return -1;
        }

        internal static CacheKeyComparer GetInstance()
        {
            if (s_comparerInstance == null)
            {
                s_comparerInstance = new CacheKeyComparer();
            }
            return s_comparerInstance;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return (this.Compare(x, y) == 0);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            CacheKey key = (CacheKey) obj;
            return key.GetHashCode();
        }
    }
}

