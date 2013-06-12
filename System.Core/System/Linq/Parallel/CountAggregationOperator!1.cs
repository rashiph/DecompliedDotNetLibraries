namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class CountAggregationOperator<TSource> : InlinedAggregationOperator<TSource, int, int>
    {
        internal CountAggregationOperator(IEnumerable<TSource> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<int, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<TSource, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new CountAggregationOperatorEnumerator<TSource, TKey>(source, index, cancellationToken);
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

        private class CountAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<int>
        {
            private readonly QueryOperatorEnumerator<TSource, TKey> m_source;

            internal CountAggregationOperatorEnumerator(QueryOperatorEnumerator<TSource, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref int currentElement)
            {
                TSource local = default(TSource);
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<TSource, TKey> source = this.m_source;
                if (!source.MoveNext(ref local, ref currentKey))
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
                    num++;
                }
                while (source.MoveNext(ref local, ref currentKey));
                currentElement = num;
                return true;
            }
        }
    }
}

