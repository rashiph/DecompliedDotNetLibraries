namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class IntMinMaxAggregationOperator : InlinedAggregationOperator<int, int, int>
    {
        private readonly int m_sign;

        internal IntMinMaxAggregationOperator(IEnumerable<int> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<int, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<int, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new IntMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override int InternalAggregate(ref Exception singularExceptionToThrow)
        {
            int num4;
            using (IEnumerator<int> enumerator = this.GetEnumerator(3, true))
            {
                int num3;
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    return 0;
                }
                int current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_0062;
                }
                while (enumerator.MoveNext())
                {
                    int num2 = enumerator.Current;
                    if (num2 < current)
                    {
                        current = num2;
                    }
                }
                goto Label_006A;
            Label_0055:
                num3 = enumerator.Current;
                if (num3 > current)
                {
                    current = num3;
                }
            Label_0062:
                if (enumerator.MoveNext())
                {
                    goto Label_0055;
                }
            Label_006A:
                num4 = current;
            }
            return num4;
        }

        private class IntMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<int>
        {
            private readonly int m_sign;
            private readonly QueryOperatorEnumerator<int, TKey> m_source;

            internal IntMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<int, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref int currentElement)
            {
                QueryOperatorEnumerator<int, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign == -1)
                {
                    int num2 = 0;
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
                    int num3 = 0;
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

