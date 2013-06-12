namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableLongSumAggregationOperator : InlinedAggregationOperator<long?, long?, long?>
    {
        internal NullableLongSumAggregationOperator(IEnumerable<long?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<long?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<long?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableLongSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override long? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<long?> enumerator = this.GetEnumerator(3, true))
            {
                long num = 0L;
                while (enumerator.MoveNext())
                {
                    long? current = enumerator.Current;
                    num += current.GetValueOrDefault();
                }
                return new long?(num);
            }
        }

        private class NullableLongSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<long?>
        {
            private readonly QueryOperatorEnumerator<long?, TKey> m_source;

            internal NullableLongSumAggregationOperatorEnumerator(QueryOperatorEnumerator<long?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref long? currentElement)
            {
                long? nullable = null;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<long?, TKey> source = this.m_source;
                if (!source.MoveNext(ref nullable, ref currentKey))
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
                    num += nullable.GetValueOrDefault();
                }
                while (source.MoveNext(ref nullable, ref currentKey));
                currentElement = new long?(num);
                return true;
            }
        }
    }
}

