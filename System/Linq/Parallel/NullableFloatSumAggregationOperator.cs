namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableFloatSumAggregationOperator : InlinedAggregationOperator<float?, double?, float?>
    {
        internal NullableFloatSumAggregationOperator(IEnumerable<float?> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<double?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<float?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableFloatSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override float? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<double?> enumerator = this.GetEnumerator(3, true))
            {
                double num = 0.0;
                while (enumerator.MoveNext())
                {
                    double? current = enumerator.Current;
                    num += current.GetValueOrDefault();
                }
                return new float?((float) num);
            }
        }

        private class NullableFloatSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<double?>
        {
            private readonly QueryOperatorEnumerator<float?, TKey> m_source;

            internal NullableFloatSumAggregationOperatorEnumerator(QueryOperatorEnumerator<float?, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref double? currentElement)
            {
                float? nullable = null;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<float?, TKey> source = this.m_source;
                if (!source.MoveNext(ref nullable, ref currentKey))
                {
                    return false;
                }
                float num = 0f;
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
                currentElement = new double?((double) num);
                return true;
            }
        }
    }
}

