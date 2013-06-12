namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableDoubleAverageAggregationOperator : InlinedAggregationOperator<double?, Pair<double, long>, double?>
    {
        internal NullableDoubleAverageAggregationOperator(IEnumerable<double?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<Pair<double, long>, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<double?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableDoubleAverageAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override double? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<Pair<double, long>> enumerator = this.GetEnumerator(3, true))
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                Pair<double, long> current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    Pair<double, long> pair2 = enumerator.Current;
                    current.First += pair2.First;
                    Pair<double, long> pair3 = enumerator.Current;
                    current.Second += pair3.Second;
                }
                return new double?(current.First / ((double) current.Second));
            }
        }

        private class NullableDoubleAverageAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<Pair<double, long>>
        {
            private QueryOperatorEnumerator<double?, TKey> m_source;

            internal NullableDoubleAverageAggregationOperatorEnumerator(QueryOperatorEnumerator<double?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref Pair<double, long> currentElement)
            {
                double first = 0.0;
                long second = 0L;
                QueryOperatorEnumerator<double?, TKey> source = this.m_source;
                double? nullable = null;
                TKey currentKey = default(TKey);
                int num3 = 0;
                while (source.MoveNext(ref nullable, ref currentKey))
                {
                    if (nullable.HasValue)
                    {
                        if ((num3++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        first += nullable.GetValueOrDefault();
                        second += 1L;
                    }
                }
                currentElement = new Pair<double, long>(first, second);
                return (second > 0L);
            }
        }
    }
}

