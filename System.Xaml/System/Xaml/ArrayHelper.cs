namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    internal static class ArrayHelper
    {
        internal static S[] ConvertArrayType<R, S>(ICollection<R> src, Func<R, S> f)
        {
            if (src == null)
            {
                return null;
            }
            int count = src.Count;
            int num2 = 0;
            S[] localArray = new S[count];
            foreach (R local in src)
            {
                localArray[num2++] = f(local);
            }
            return localArray;
        }

        internal static void ForAll<R>(R[] src, Action<R> f)
        {
            foreach (R local in src)
            {
                f(local);
            }
        }

        internal static List<T> ToList<T>(IEnumerable<T> src)
        {
            if (src == null)
            {
                return null;
            }
            return new List<T>(src);
        }
    }
}

