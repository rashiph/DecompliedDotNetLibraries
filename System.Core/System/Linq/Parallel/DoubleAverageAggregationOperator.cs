namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class DoubleAverageAggregationOperator : InlinedAggregationOperator<double, Pair<double, long>, double>
    {
        internal DoubleAverageAggregationOperator(IEnumerable<double> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<Pair<double, long>, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<double, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new DoubleAverageAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override double InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<Pair<double, long>> enumerator = this.GetEnumerator(3, true))
            {
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    return 0.0;
                }
                Pair<double, long> current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    Pair<double, long> pair2 = enumerator.Current;
                    current.First += pair2.First;
                    Pair<double, long> pair3 = enumerator.Current;
                    current.Second += pair3.Second;
                }
                return (current.First / ((double) current.Second));
            }
        }

        private class DoubleAverageAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<Pair<double, long>>
        {
            private QueryOperatorEnumerator<double, TKey> m_source;

            internal DoubleAverageAggregationOperatorEnumerator(QueryOperatorEnumerator<double, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
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
                QueryOperatorEnumerator<double, TKey> source = this.m_source;
                double num3 = 0.0;
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
                currentElement = new Pair<double, long>(first, second);
                return true;
            }
        }
    }
}

