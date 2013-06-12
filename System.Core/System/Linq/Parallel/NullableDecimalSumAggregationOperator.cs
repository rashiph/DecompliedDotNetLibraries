namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableDecimalSumAggregationOperator : InlinedAggregationOperator<decimal?, decimal?, decimal?>
    {
        internal NullableDecimalSumAggregationOperator(IEnumerable<decimal?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<decimal?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<decimal?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableDecimalSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override decimal? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<decimal?> enumerator = this.GetEnumerator(3, true))
            {
                decimal num = 0.0M;
                while (enumerator.MoveNext())
                {
                    decimal? current = enumerator.Current;
                    num += current.GetValueOrDefault();
                }
                return new decimal?(num);
            }
        }

        private class NullableDecimalSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<decimal?>
        {
            private readonly QueryOperatorEnumerator<decimal?, TKey> m_source;

            internal NullableDecimalSumAggregationOperatorEnumerator(QueryOperatorEnumerator<decimal?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref decimal? currentElement)
            {
                decimal? nullable = null;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<decimal?, TKey> source = this.m_source;
                if (!source.MoveNext(ref nullable, ref currentKey))
                {
                    return false;
                }
                decimal num = 0.0M;
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
                currentElement = new decimal?(num);
                return true;
            }
        }
    }
}

