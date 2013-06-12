namespace System.Linq
{
    using System;
    using System.Collections.Generic;

    internal class OrderedEnumerable<TElement, TKey> : OrderedEnumerable<TElement>
    {
        internal IComparer<TKey> comparer;
        internal bool descending;
        internal Func<TElement, TKey> keySelector;
        internal OrderedEnumerable<TElement> parent;

        internal OrderedEnumerable(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (keySelector == null)
            {
                throw Error.ArgumentNull("keySelector");
            }
            base.source = source;
            this.parent = null;
            this.keySelector = keySelector;
            this.comparer = (comparer != null) ? comparer : ((IComparer<TKey>) Comparer<TKey>.Default);
            this.descending = descending;
        }

        internal override EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next)
        {
            EnumerableSorter<TElement> enumerableSorter = new EnumerableSorter<TElement, TKey>(this.keySelector, this.comparer, this.descending, next);
            if (this.parent != null)
            {
                enumerableSorter = this.parent.GetEnumerableSorter(enumerableSorter);
            }
            return enumerableSorter;
        }
    }
}

