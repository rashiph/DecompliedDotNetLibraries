namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class LongSumAggregationOperator : InlinedAggregationOperator<long, long, long>
    {
        internal LongSumAggregationOperator(IEnumerable<long> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<long, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<long, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new LongSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
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

        private class LongSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<long>
        {
            private readonly QueryOperatorEnumerator<long, TKey> m_source;

            internal LongSumAggregationOperatorEnumerator(QueryOperatorEnumerator<long, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref long currentElement)
            {
                long num = 0L;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<long, TKey> source = this.m_source;
                if (!source.MoveNext(ref num, ref currentKey))
                {
                    return false;
                }
                long num2 = 0L;
                int num3 = 0;
                do
                {
                    if ((num3++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                    }
                    num2 += num;
                }
                while (source.MoveNext(ref num, ref currentKey));
                currentElement = num2;
                return true;
            }
        }
    }
}

