namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableIntAverageAggregationOperator : InlinedAggregationOperator<int?, Pair<long, long>, double?>
    {
        internal NullableIntAverageAggregationOperator(IEnumerable<int?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<Pair<long, long>, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<int?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableIntAverageAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override double? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<Pair<long, long>> enumerator = this.GetEnumerator(3, true))
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                Pair<long, long> current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    Pair<long, long> pair2 = enumerator.Current;
                    current.First += pair2.First;
                    Pair<long, long> pair3 = enumerator.Current;
                    current.Second += pair3.Second;
                }
                return new double?(((double) current.First) / ((double) current.Second));
            }
        }

        private class NullableIntAverageAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<Pair<long, long>>
        {
            private QueryOperatorEnumerator<int?, TKey> m_source;

            internal NullableIntAverageAggregationOperatorEnumerator(QueryOperatorEnumerator<int?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref Pair<long, long> currentElement)
            {
                long first = 0L;
                long second = 0L;
                QueryOperatorEnumerator<int?, TKey> source = this.m_source;
                int? nullable = null;
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
                        first += (long) nullable.GetValueOrDefault();
                        second += 1L;
                    }
                }
                currentElement = new Pair<long, long>(first, second);
                return (second > 0L);
            }
        }
    }
}

