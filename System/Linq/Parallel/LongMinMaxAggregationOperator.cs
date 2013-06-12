namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class LongMinMaxAggregationOperator : InlinedAggregationOperator<long, long, long>
    {
        private readonly int m_sign;

        internal LongMinMaxAggregationOperator(IEnumerable<long> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<long, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<long, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new LongMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override long InternalAggregate(ref Exception singularExceptionToThrow)
        {
            long num4;
            using (IEnumerator<long> enumerator = this.GetEnumerator(3, true))
            {
                long num3;
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    return 0L;
                }
                long current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_0063;
                }
                while (enumerator.MoveNext())
                {
                    long num2 = enumerator.Current;
                    if (num2 < current)
                    {
                        current = num2;
                    }
                }
                goto Label_006B;
            Label_0056:
                num3 = enumerator.Current;
                if (num3 > current)
                {
                    current = num3;
                }
            Label_0063:
                if (enumerator.MoveNext())
                {
                    goto Label_0056;
                }
            Label_006B:
                num4 = current;
            }
            return num4;
        }

        private class LongMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<long>
        {
            private int m_sign;
            private QueryOperatorEnumerator<long, TKey> m_source;

            internal LongMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<long, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref long currentElement)
            {
                QueryOperatorEnumerator<long, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign == -1)
                {
                    long num2 = 0L;
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
                    long num3 = 0L;
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

