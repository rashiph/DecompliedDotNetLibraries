namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableLongMinMaxAggregationOperator : InlinedAggregationOperator<long?, long?, long?>
    {
        private readonly int m_sign;

        internal NullableLongMinMaxAggregationOperator(IEnumerable<long?> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<long?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<long?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableLongMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override long? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            long? nullable4;
            using (IEnumerator<long?> enumerator = this.GetEnumerator(3, true))
            {
                long? nullable3;
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                long? current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_00BE;
                }
                while (enumerator.MoveNext())
                {
                    long? nullable2 = enumerator.Current;
                    if (current.HasValue)
                    {
                        long? nullable6 = nullable2;
                        long? nullable7 = current;
                        if ((nullable6.GetValueOrDefault() >= nullable7.GetValueOrDefault()) || !(nullable6.HasValue & nullable7.HasValue))
                        {
                            continue;
                        }
                    }
                    current = nullable2;
                }
                goto Label_00C6;
            Label_0082:
                nullable3 = enumerator.Current;
                if (current.HasValue)
                {
                    long? nullable8 = nullable3;
                    long? nullable9 = current;
                    if ((nullable8.GetValueOrDefault() <= nullable9.GetValueOrDefault()) || !(nullable8.HasValue & nullable9.HasValue))
                    {
                        goto Label_00BE;
                    }
                }
                current = nullable3;
            Label_00BE:
                if (enumerator.MoveNext())
                {
                    goto Label_0082;
                }
            Label_00C6:
                nullable4 = current;
            }
            return nullable4;
        }

        private class NullableLongMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<long?>
        {
            private int m_sign;
            private QueryOperatorEnumerator<long?, TKey> m_source;

            internal NullableLongMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<long?, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref long? currentElement)
            {
                QueryOperatorEnumerator<long?, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign != -1)
                {
                    long? nullable2 = null;
                    while (source.MoveNext(ref nullable2, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (currentElement.HasValue)
                        {
                            long? nullable5 = nullable2;
                            long? nullable6 = currentElement;
                            if ((nullable5.GetValueOrDefault() <= nullable6.GetValueOrDefault()) || !(nullable5.HasValue & nullable6.HasValue))
                            {
                                continue;
                            }
                        }
                        currentElement = nullable2;
                    }
                }
                else
                {
                    long? nullable = null;
                    while (source.MoveNext(ref nullable, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (currentElement.HasValue)
                        {
                            long? nullable3 = nullable;
                            long? nullable4 = currentElement;
                            if ((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue))
                            {
                                continue;
                            }
                        }
                        currentElement = nullable;
                    }
                }
                return true;
            }
        }
    }
}

