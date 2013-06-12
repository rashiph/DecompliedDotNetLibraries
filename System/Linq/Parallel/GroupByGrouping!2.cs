namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class GroupByGrouping<TGroupKey, TElement> : IGrouping<TGroupKey, TElement>, IEnumerable<TElement>, IEnumerable
    {
        private KeyValuePair<Wrapper<TGroupKey>, ListChunk<TElement>> m_keyValues;

        internal GroupByGrouping(TGroupKey key)
        {
            this.m_keyValues = new KeyValuePair<Wrapper<TGroupKey>, ListChunk<TElement>>(new Wrapper<TGroupKey>(key), new ListChunk<TElement>(2));
        }

        internal GroupByGrouping(KeyValuePair<Wrapper<TGroupKey>, ListChunk<TElement>> keyValues)
        {
            this.m_keyValues = keyValues;
        }

        internal void Add(TElement element)
        {
            this.m_keyValues.Value.Add(element);
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return this.m_keyValues.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TElement>) this).GetEnumerator();
        }

        TGroupKey IGrouping<TGroupKey, TElement>.Key
        {
            get
            {
                return this.m_keyValues.Key.Value;
            }
        }
    }
}

