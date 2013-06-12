namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class GroupByElementSelectorQueryOperatorEnumerator<TSource, TGroupKey, TElement, TOrderKey> : GroupByQueryOperatorEnumerator<TSource, TGroupKey, TElement, TOrderKey>
    {
        private readonly Func<TSource, TElement> m_elementSelector;

        internal GroupByElementSelectorQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TSource, TGroupKey>, TOrderKey> source, IEqualityComparer<TGroupKey> keyComparer, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken) : base(source, keyComparer, cancellationToken)
        {
            this.m_elementSelector = elementSelector;
        }

        protected override HashLookup<Wrapper<TGroupKey>, ListChunk<TElement>> BuildHashLookup()
        {
            HashLookup<Wrapper<TGroupKey>, ListChunk<TElement>> lookup = new HashLookup<Wrapper<TGroupKey>, ListChunk<TElement>>(new WrapperEqualityComparer<TGroupKey>(base.m_keyComparer));
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
                ListChunk<TElement> chunk = null;
                if (!lookup.TryGetValue(key, ref chunk))
                {
                    chunk = new ListChunk<TElement>(2);
                    lookup.Add(key, chunk);
                }
                chunk.Add(this.m_elementSelector(currentElement.First));
            }
            return lookup;
        }
    }
}

