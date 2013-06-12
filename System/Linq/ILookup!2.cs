namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
    {
        bool Contains(TKey key);

        int Count { get; }

        IEnumerable<TElement> this[TKey key] { get; }
    }
}

