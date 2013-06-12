namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableIntSumAggregationOperator : InlinedAggregationOperator<int?, int?, int?>
    {
        internal NullableIntSumAggregationOperator(IEnumerable<int?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<int?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<int?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableIntSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override int? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<int?> enumerator = this.GetEnumerator(3, true))
            {
                int num = 0;
                while (enumerator.MoveNext())
                {
                    int? current = enumerator.Current;
                    num += current.GetValueOrDefault();
                }
                return new int?(num);
            }
        }

        private class NullableIntSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<int?>
        {
            private QueryOperatorEnumerator<int?, TKey> m_source;

            internal NullableIntSumAggregationOperatorEnumerator(QueryOperatorEnumerator<int?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref int? currentElement)
            {
                int? nullable = null;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<int?, TKey> source = this.m_source;
                if (!source.MoveNext(ref nullable, ref currentKey))
                {
                    return false;
                }
                int num = 0;
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
                currentElement = new int?(num);
                return true;
            }
        }
    }
}

