namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class DecimalSumAggregationOperator : InlinedAggregationOperator<decimal, decimal, decimal>
    {
        internal DecimalSumAggregationOperator(IEnumerable<decimal> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<decimal, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<decimal, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new DecimalSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override decimal InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<decimal> enumerator = this.GetEnumerator(3, true))
            {
                decimal num = 0.0M;
                while (enumerator.MoveNext())
                {
                    num += enumerator.Current;
                }
                return num;
            }
        }

        private class DecimalSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<decimal>
        {
            private QueryOperatorEnumerator<decimal, TKey> m_source;

            internal DecimalSumAggregationOperatorEnumerator(QueryOperatorEnumerator<decimal, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref decimal currentElement)
            {
                decimal num = 0M;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<decimal, TKey> source = this.m_source;
                if (!source.MoveNext(ref num, ref currentKey))
                {
                    return false;
                }
                decimal num2 = 0.0M;
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

