namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal abstract class OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TElement, TOrderKey> : QueryOperatorEnumerator<IGrouping<TGroupKey, TElement>, TOrderKey>
    {
        protected readonly CancellationToken m_cancellationToken;
        protected readonly IEqualityComparer<TGroupKey> m_keyComparer;
        private readonly Func<TSource, TGroupKey> m_keySelector;
        private Mutables<TSource, TGroupKey, TElement, TOrderKey> m_mutables;
        protected readonly IComparer<TOrderKey> m_orderComparer;
        protected readonly QueryOperatorEnumerator<Pair<TSource, TGroupKey>, TOrderKey> m_source;

        protected OrderedGroupByQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TSource, TGroupKey>, TOrderKey> source, Func<TSource, TGroupKey> keySelector, IEqualityComparer<TGroupKey> keyComparer, IComparer<TOrderKey> orderComparer, CancellationToken cancellationToken)
        {
            this.m_source = source;
            this.m_keySelector = keySelector;
            this.m_keyComparer = keyComparer;
            this.m_orderComparer = orderComparer;
            this.m_cancellationToken = cancellationToken;
        }

        protected abstract HashLookup<Wrapper<TGroupKey>, GroupKeyData<TSource, TGroupKey, TElement, TOrderKey>> BuildHashLookup();
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
                KeyValuePair<Wrapper<TGroupKey>, GroupKeyData<TSource, TGroupKey, TElement, TOrderKey>> pair = mutables.m_hashLookup[mutables.m_hashLookupIndex];
                GroupKeyData<TSource, TGroupKey, TElement, TOrderKey> data = pair.Value;
                currentElement = data.m_grouping;
                currentKey = data.m_orderKey;
                return true;
            }
            return false;
        }

        protected class GroupKeyData
        {
            internal OrderedGroupByGrouping<TGroupKey, TOrderKey, TElement> m_grouping;
            internal TOrderKey m_orderKey;

            internal GroupKeyData(TOrderKey orderKey, TGroupKey hashKey, IComparer<TOrderKey> orderComparer)
            {
                this.m_orderKey = orderKey;
                this.m_grouping = new OrderedGroupByGrouping<TGroupKey, TOrderKey, TElement>(hashKey, orderComparer);
            }
        }

        private class Mutables
        {
            internal HashLookup<Wrapper<TGroupKey>, OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TElement, TOrderKey>.GroupKeyData> m_hashLookup;
            internal int m_hashLookupIndex;
        }
    }
}

