namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal static class Util
    {
        private static FastDateTimeComparer s_fastDateTimeComparer = new FastDateTimeComparer();
        private static FastDoubleComparer s_fastDoubleComparer = new FastDoubleComparer();
        private static FastFloatComparer s_fastFloatComparer = new FastFloatComparer();
        private static FastIntComparer s_fastIntComparer = new FastIntComparer();
        private static FastLongComparer s_fastLongComparer = new FastLongComparer();

        internal static Comparer<TKey> GetDefaultComparer<TKey>()
        {
            if (typeof(TKey) == typeof(int))
            {
                return (Comparer<TKey>) s_fastIntComparer;
            }
            if (typeof(TKey) == typeof(long))
            {
                return (Comparer<TKey>) s_fastLongComparer;
            }
            if (typeof(TKey) == typeof(float))
            {
                return (Comparer<TKey>) s_fastFloatComparer;
            }
            if (typeof(TKey) == typeof(double))
            {
                return (Comparer<TKey>) s_fastDoubleComparer;
            }
            if (typeof(TKey) == typeof(DateTime))
            {
                return (Comparer<TKey>) s_fastDateTimeComparer;
            }
            return Comparer<TKey>.Default;
        }

        internal static int Sign(int x)
        {
            if (x < 0)
            {
                return -1;
            }
            if (x != 0)
            {
                return 1;
            }
            return 0;
        }

        private class FastDateTimeComparer : Comparer<DateTime>
        {
            public override int Compare(DateTime x, DateTime y)
            {
                return x.CompareTo(y);
            }
        }

        private class FastDoubleComparer : Comparer<double>
        {
            public override int Compare(double x, double y)
            {
                return x.CompareTo(y);
            }
        }

        private class FastFloatComparer : Comparer<float>
        {
            public override int Compare(float x, float y)
            {
                return x.CompareTo(y);
            }
        }

        private class FastIntComparer : Comparer<int>
        {
            public override int Compare(int x, int y)
            {
                return x.CompareTo(y);
            }
        }

        private class FastLongComparer : Comparer<long>
        {
            public override int Compare(long x, long y)
            {
                return x.CompareTo(y);
            }
        }
    }
}

