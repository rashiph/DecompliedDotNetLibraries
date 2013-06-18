namespace System.Xaml.Context
{
    using System;
    using System.Collections.Generic;

    internal class HashSet<T> : Dictionary<T, object>
    {
        public HashSet()
        {
        }

        public HashSet(IDictionary<T, object> other) : base(other)
        {
        }

        public HashSet(IEqualityComparer<T> comparer) : base(comparer)
        {
        }

        public void Add(T item)
        {
            base.Add(item, null);
        }
    }
}

