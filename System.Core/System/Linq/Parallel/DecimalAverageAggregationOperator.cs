namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class DecimalAverageAggregationOperator : InlinedAggregationOperator<decimal, Pair<decimal, long>, decimal>
    {
        internal DecimalAverageAggregationOperator(IEnumerable<decimal> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<Pair<decimal, long>, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<decimal, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new DecimalAverageAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override decimal InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<Pair<decimal, long>> enumerator = this.GetEnumerator(3, true))
            {
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    return 0M;
                }
                Pair<decimal, long> current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    Pair<decimal, long> pair2 = enumerator.Current;
                    current.First += pair2.First;
                    Pair<decimal, long> pair3 = enumerator.Current;
                    current.Second += pair3.Second;
                }
                return (current.First / current.Second);
            }
        }

        private class DecimalAverageAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<Pair<decimal, long>>
        {
            private QueryOperatorEnumerator<decimal, TKey> m_source;

            internal DecimalAverageAggregationOperatorEnumerator(QueryOperatorEnumerator<decimal, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
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
                QueryOperatorEnumerator<decimal, TKey> source = this.m_source;
                decimal num3 = 0M;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref num3, ref currentKey))
                {
                    return false;
                }
                int num4 = 0;
                do
                {
                    if ((num4++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                    }
                    first += num3;
                    second += 1L;
                }
                while (source.MoveNext(ref num3, ref currentKey));
                currentElement = new Pair<decimal, long>(first, second);
                return true;
            }
        }
    }
}

