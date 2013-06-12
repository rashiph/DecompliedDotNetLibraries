namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class DoubleMinMaxAggregationOperator : InlinedAggregationOperator<double, double, double>
    {
        private readonly int m_sign;

        internal DoubleMinMaxAggregationOperator(IEnumerable<double> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<double, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<double, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new DoubleMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override double InternalAggregate(ref Exception singularExceptionToThrow)
        {
            double num4;
            using (IEnumerator<double> enumerator = this.GetEnumerator(3, true))
            {
                double num3;
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    return 0.0;
                }
                double current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_007A;
                }
                while (enumerator.MoveNext())
                {
                    double d = enumerator.Current;
                    if ((d < current) || double.IsNaN(d))
                    {
                        current = d;
                    }
                }
                goto Label_0082;
            Label_0065:
                num3 = enumerator.Current;
                if ((num3 > current) || double.IsNaN(current))
                {
                    current = num3;
                }
            Label_007A:
                if (enumerator.MoveNext())
                {
                    goto Label_0065;
                }
            Label_0082:
                num4 = current;
            }
            return num4;
        }

        private class DoubleMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<double>
        {
            private int m_sign;
            private QueryOperatorEnumerator<double, TKey> m_source;

            internal DoubleMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<double, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref double currentElement)
            {
                QueryOperatorEnumerator<double, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign == -1)
                {
                    double num2 = 0.0;
                    while (source.MoveNext(ref num2, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if ((num2 < currentElement) || double.IsNaN(num2))
                        {
                            currentElement = num2;
                        }
                    }
                }
                else
                {
                    double num3 = 0.0;
                    while (source.MoveNext(ref num3, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if ((num3 > currentElement) || double.IsNaN(currentElement))
                        {
                            currentElement = num3;
                        }
                    }
                }
                return true;
            }
        }
    }
}

