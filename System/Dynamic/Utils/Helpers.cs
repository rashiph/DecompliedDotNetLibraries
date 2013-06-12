namespace System.Dynamic.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal static class Helpers
    {
        internal static T CommonNode<T>(T first, T second, Func<T, T> parent) where T: class
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            if (comparer.Equals(first, second))
            {
                return first;
            }
            Set<T> set = new Set<T>(comparer);
            for (T local = first; local != null; local = parent(local))
            {
                set.Add(local);
            }
            for (T local2 = second; local2 != null; local2 = parent(local2))
            {
                if (set.Contains(local2))
                {
                    return local2;
                }
            }
            return default(T);
        }

        internal static void IncrementCount<T>(T key, Dictionary<T, int> dict)
        {
            int num;
            dict.TryGetValue(key, out num);
            dict[key] = num + 1;
        }
    }
}

