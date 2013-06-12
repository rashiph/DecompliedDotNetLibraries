namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class GroupedEnumerable<TSource, TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
    {
        private IEqualityComparer<TKey> comparer;
        private Func<TSource, TElement> elementSelector;
        private Func<TSource, TKey> keySelector;
        private IEnumerable<TSource> source;

        public GroupedEnumerable(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (keySelector == null)
            {
                throw Error.ArgumentNull("keySelector");
            }
            if (elementSelector == null)
            {
                throw Error.ArgumentNull("elementSelector");
            }
            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return Lookup<TKey, TElement>.Create<TSource>(this.source, this.keySelector, this.elementSelector, this.comparer).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

