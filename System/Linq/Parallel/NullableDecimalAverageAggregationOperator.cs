namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableDecimalAverageAggregationOperator : InlinedAggregationOperator<decimal?, Pair<decimal, long>, decimal?>
    {
        internal NullableDecimalAverageAggregationOperator(IEnumerable<decimal?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<Pair<decimal, long>, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<decimal?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableDecimalAverageAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override decimal? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<Pair<decimal, long>> enumerator = this.GetEnumerator(3, true))
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                Pair<decimal, long> current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    Pair<decimal, long> pair2 = enumerator.Current;
                    current.First += pair2.First;
                    Pair<decimal, long> pair3 = enumerator.Current;
                    current.Second += pair3.Second;
                }
                return new decimal?(current.First / current.Second);
            }
        }

        private class NullableDecimalAverageAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<Pair<decimal, long>>
        {
            private QueryOperatorEnumerator<decimal?, TKey> m_source;

            internal NullableDecimalAverageAggregationOperatorEnumerator(QueryOperatorEnumerator<decimal?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref Pair<decimal, long> currentElement)
            {
                decimal first = 0.0M;
                long second = 0L;
                QueryOperatorEnumerator<decimal?, TKey> source = this.m_source;
                decimal? nullable = null;
                TKey currentKey = default(TKey);
                int num3 = 0;
                while (source.MoveNext(ref nullable, ref currentKey))
                {
                    if ((num3++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                    }
                    if (nullable.HasValue)
                    {
                        first += nullable.GetValueOrDefault();
                        second += 1L;
                    }
                }
                currentElement = new Pair<decimal, long>(first, second);
                return (second > 0L);
            }
        }
    }
}

