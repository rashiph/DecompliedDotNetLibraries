namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class DoubleSumAggregationOperator : InlinedAggregationOperator<double, double, double>
    {
        internal DoubleSumAggregationOperator(IEnumerable<double> child) : base(child)
        {
        }

        protected override QueryOperatorEnumerator<double, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<double, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new DoubleSumAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        protected override double InternalAggregate(ref Exception singularExceptionToThrow)
        {
            using (IEnumerator<double> enumerator = this.GetEnumerator(3, true))
            {
                double num = 0.0;
                while (enumerator.MoveNext())
                {
                    num += enumerator.Current;
                }
                return num;
            }
        }

        private class DoubleSumAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<double>
        {
            private readonly QueryOperatorEnumerator<double, TKey> m_source;

            internal DoubleSumAggregationOperatorEnumerator(QueryOperatorEnumerator<double, TKey> source, int partitionIndex, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref double currentElement)
            {
                double num = 0.0;
                TKey currentKey = default(TKey);
                QueryOperatorEnumerator<double, TKey> source = this.m_source;
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

