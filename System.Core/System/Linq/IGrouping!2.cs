namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IGrouping<out TKey, out TElement> : IEnumerable<TElement>, IEnumerable
    {
        TKey Key { get; }
    }
}

