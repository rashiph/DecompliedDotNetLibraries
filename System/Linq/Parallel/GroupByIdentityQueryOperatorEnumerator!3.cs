namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class GroupByIdentityQueryOperatorEnumerator<TSource, TGroupKey, TOrderKey> : GroupByQueryOperatorEnumerator<TSource, TGroupKey, TSource, TOrderKey>
    {
        internal GroupByIdentityQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TSource, TGroupKey>, TOrderKey> source, IEqualityComparer<TGroupKey> keyComparer, CancellationToken cancellationToken) : base(source, keyComparer, cancellationToken)
        {
        }

        protected override HashLookup<Wrapper<TGroupKey>, ListChunk<TSource>> BuildHashLookup()
        {
            HashLookup<Wrapper<TGroupKey>, ListChunk<TSource>> lookup = new HashLookup<Wrapper<TGroupKey>, ListChunk<TSource>>(new WrapperEqualityComparer<TGroupKey>(base.m_keyComparer));
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
                ListChunk<TSource> chunk = null;
                if (!lookup.TryGetValue(key, ref chunk))
                {
                    chunk = new ListChunk<TSource>(2);
                    lookup.Add(key, chunk);
                }
                chunk.Add(currentElement.First);
            }
            return lookup;
        }
    }
}

