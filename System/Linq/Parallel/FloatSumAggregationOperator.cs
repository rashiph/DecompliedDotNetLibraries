namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class FloatSumAggregationOperator : InlinedAggregationOperator<float, double, float>
    {
        internal FloatSumAggregationOperator(IEnumerable<float> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<double, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<float, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new FloatSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override float InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<double> enumerator = this.GetEnumerator(3, true))
            {
                double num = 0.0;
                while (enumerator.MoveNext())
                {
                    num += enumerator.Current;
                }
                return (float) num;
            }
        }

        private class FloatSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<double>
        {
            private readonly QueryOperatorEnumerator<float, TKey> m_source;

            internal FloatSumAggregationOperatorEnumerator(QueryOperatorEnumerator<float, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref double currentElement)
            {
                float num = 0f;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<float, TKey> source = this.m_source;
                if (!source.MoveNext(ref num, ref currentKey))
                {
                    return false;
                }
                double num2 = 0.0;
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

