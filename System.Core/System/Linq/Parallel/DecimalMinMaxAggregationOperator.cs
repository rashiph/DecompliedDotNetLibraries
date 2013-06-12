namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class DecimalMinMaxAggregationOperator : InlinedAggregationOperator<decimal, decimal, decimal>
    {
        private readonly int m_sign;

        internal DecimalMinMaxAggregationOperator(IEnumerable<decimal> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<decimal, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<decimal, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new DecimalMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override decimal InternalAggregate(ref Exception singularExceptionToThrow)
        {
            decimal num4;
            using (IEnumerator<decimal> enumerator = this.GetEnumerator(3, true))
            {
                decimal num3;
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    return 0M;
                }
                decimal current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_0071;
                }
                while (enumerator.MoveNext())
                {
                    decimal num2 = enumerator.Current;
                    if (num2 < current)
                    {
                        current = num2;
                    }
                }
                goto Label_0079;
            Label_005F:
                num3 = enumerator.Current;
                if (num3 > current)
                {
                    current = num3;
                }
            Label_0071:
                if (enumerator.MoveNext())
                {
                    goto Label_005F;
                }
            Label_0079:
                num4 = current;
            }
            return num4;
        }

        private class DecimalMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<decimal>
        {
            private int m_sign;
            private QueryOperatorEnumerator<decimal, TKey> m_source;

            internal DecimalMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<decimal, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref decimal currentElement)
            {
                QueryOperatorEnumerator<decimal, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign == -1)
                {
                    decimal num2 = 0M;
                    while (source.MoveNext(ref num2, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (num2 < currentElement)
                        {
                            currentElement = num2;
                        }
                    }
                }
                else
                {
                    decimal num3 = 0M;
                    while (source.MoveNext(ref num3, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (num3 > currentElement)
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

