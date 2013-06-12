namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableDoubleSumAggregationOperator : InlinedAggregationOperator<double?, double?, double?>
    {
        internal NullableDoubleSumAggregationOperator(IEnumerable<double?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<double?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<double?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableDoubleSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override double? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<double?> enumerator = this.GetEnumerator(3, true))
            {
                double num = 0.0;
                while (enumerator.MoveNext())
                {
                    double? current = enumerator.Current;
                    num += current.GetValueOrDefault();
                }
                return new double?(num);
            }
        }

        private class NullableDoubleSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<double?>
        {
            private readonly QueryOperatorEnumerator<double?, TKey> m_source;

            internal NullableDoubleSumAggregationOperatorEnumerator(QueryOperatorEnumerator<double?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref double? currentElement)
            {
                double? nullable = null;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<double?, TKey> source = this.m_source;
                if (!source.MoveNext(ref nullable, ref currentKey))
                {
                    return false;
                }
                double num = 0.0;
                int num2 = 0;
                do
                {
                    if ((num2++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                    }
                    num += nullable.GetValueOrDefault();
                }
                while (source.MoveNext(ref nullable, ref currentKey));
                currentElement = new double?(num);
                return true;
            }
        }
    }
}

