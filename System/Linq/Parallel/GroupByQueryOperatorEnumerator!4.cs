namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal abstract class GroupByQueryOperatorEnumerator<TSource, TGroupKey, TElement, TOrderKey> : QueryOperatorEnumerator<IGrouping<TGroupKey, TElement>, TOrderKey>
    {
        protected readonly CancellationToken m_cancellationToken;
        protected readonly IEqualityComparer<TGroupKey> m_keyComparer;
        private Mutables<TSource, TGroupKey, TElement, TOrderKey> m_mutables;
        protected readonly QueryOperatorEnumerator<Pair<TSource, TGroupKey>, TOrderKey> m_source;

        protected GroupByQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TSource, TGroupKey>, TOrderKey> source, IEqualityComparer<TGroupKey> keyComparer, CancellationToken cancellationToken)
        {
            this.m_source = source;
            this.m_keyComparer = keyComparer;
            this.m_cancellationToken = cancellationToken;
        }

        protected abstract HashLookup<Wrapper<TGroupKey>, ListChunk<TElement>> BuildHashLookup();
        protected override void Dispose(bool disposing)
        {
            this.m_source.Dispose();
        }

        internal override bool MoveNext(ref IGrouping<TGroupKey, TElement> currentElement, ref TOrderKey currentKey)
        {
            Mutables<TSource, TGroupKey, TElement, TOrderKey> mutables = this.m_mutables;
            if (mutables == null)
            {
                mutables = this.m_mutables = new Mutables<TSource, TGroupKey, TElement, TOrderKey>();
                mutables.m_hashLookup = this.BuildHashLookup();
                mutables.m_hashLookupIndex = -1;
            }
            if (++mutables.m_hashLookupIndex < mutables.m_hashLookup.Count)
            {
                currentElement = new GroupByGrouping<TGroupKey, TElement>(mutables.m_hashLookup[mutables.m_hashLookupIndex]);
                return true;
            }
            return false;
        }

        private class Mutables
        {
            internal HashLookup<Wrapper<TGroupKey>, ListChunk<TElement>> m_hashLookup;
            internal int m_hashLookupIndex;
        }
    }
}

