namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class IntSumAggregationOperator : InlinedAggregationOperator<int, int, int>
    {
        internal IntSumAggregationOperator(IEnumerable<int> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<int, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<int, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new IntSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override int InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<int> enumerator = this.GetEnumerator(3, true))
            {
                int num = 0;
                while (enumerator.MoveNext())
                {
                    num += enumerator.Current;
                }
                return num;
            }
        }

        private class IntSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<int>
        {
            private readonly QueryOperatorEnumerator<int, TKey> m_source;

            internal IntSumAggregationOperatorEnumerator(QueryOperatorEnumerator<int, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref int currentElement)
            {
                int num = 0;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<int, TKey> source = this.m_source;
                if (!source.MoveNext(ref num, ref currentKey))
                {
                    return false;
                }
                int num2 = 0;
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

