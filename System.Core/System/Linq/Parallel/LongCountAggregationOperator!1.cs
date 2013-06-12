namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class LongCountAggregationOperator<TSource> : InlinedAggregationOperator<TSource, long, long>
    {
        internal LongCountAggregationOperator(IEnumerable<TSource> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<long, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<TSource, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new LongCountAggregationOperatorEnumerator<TSource, TKey>(source, index, cancellationToken);
        }

        protected override long InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<long> enumerator = this.GetEnumerator(3, true))
            {
                long num = 0L;
                while (enumerator.MoveNext())
                {
                    num += enumerator.Current;
                }
                return num;
            }
        }

        private class LongCountAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<long>
        {
            private readonly QueryOperatorEnumerator<TSource, TKey> m_source;

            internal LongCountAggregationOperatorEnumerator(QueryOperatorEnumerator<TSource, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref long currentElement)
            {
                TSource local = default(TSource);
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<TSource, TKey> source = this.m_source;
                if (!source.MoveNext(ref local, ref currentKey))
                {
                    return false;
                }
                long num = 0L;
                int num2 = 0;
                do
                {
                    if ((num2++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                    }
                    num += 1L;
                }
                while (source.MoveNext(ref local, ref currentKey));
                currentElement = num;
                return true;
            }
        }
    }
}

