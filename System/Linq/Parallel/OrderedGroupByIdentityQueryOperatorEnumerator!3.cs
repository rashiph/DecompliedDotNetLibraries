namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class OrderedGroupByIdentityQueryOperatorEnumerator<TSource, TGroupKey, TOrderKey> : OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>
    {
        internal OrderedGroupByIdentityQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TSource, TGroupKey>, TOrderKey> source, Func<TSource, TGroupKey> keySelector, IEqualityComparer<TGroupKey> keyComparer, IComparer<TOrderKey> orderComparer, CancellationToken cancellationToken) : base(source, keySelector, keyComparer, orderComparer, cancellationToken)
        {
        }

        protected override HashLookup<Wrapper<TGroupKey>, OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>.GroupKeyData> BuildHashLookup()
        {
            HashLookup<Wrapper<TGroupKey>, OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>.GroupKeyData> lookup = new HashLookup<Wrapper<TGroupKey>, OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>.GroupKeyData>(new WrapperEqualityComparer<TGroupKey>(base.m_keyComparer));
            Pair<TSource, TGroupKey> currentElement = new Pair<TSource, TGroupKey>();
            TOrderKey currentKey = default(TOrderKey);
            int num = 0;
            while (base.m_source.MoveNext(ref currentElement, ref currentKey))
            {
                if ((num++ & 0x3f) == 0)
                {
                    CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                }
                Wrapper<TGroupKey> key = new Wrapper<TGroupKey>(currentElement.Second);
                OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>.GroupKeyData data = null;
                if (lookup.TryGetValue(key, ref data))
                {
                    if (base.m_orderComparer.Compare(currentKey, data.m_orderKey) < 0)
                    {
                        data.m_orderKey = currentKey;
                    }
                }
                else
                {
                    data = new OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>.GroupKeyData(currentKey, key.Value, base.m_orderComparer);
                    lookup.Add(key, data);
                }
                data.m_grouping.Add(currentElement.First, currentKey);
            }
            for (int i = 0; i < lookup.Count; i++)
            {
                KeyValuePair<Wrapper<TGroupKey>, OrderedGroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>.GroupKeyData> pair2 = lookup[i];
                pair2.Value.m_grouping.DoneAdding();
            }
            return lookup;
        }
    }
}

