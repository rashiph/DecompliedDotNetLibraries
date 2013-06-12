namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableDecimalMinMaxAggregationOperator : InlinedAggregationOperator<decimal?, decimal?, decimal?>
    {
        private readonly int m_sign;

        internal NullableDecimalMinMaxAggregationOperator(IEnumerable<decimal?> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<decimal?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<decimal?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableDecimalMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override decimal? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            decimal? nullable4;
            using (IEnumerator<decimal?> enumerator = this.GetEnumerator(3, true))
            {
                decimal? nullable3;
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                decimal? current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_00C8;
                }
                while (enumerator.MoveNext())
                {
                    decimal? nullable2 = enumerator.Current;
                    if (current.HasValue)
                    {
                        decimal? nullable6 = nullable2;
                        decimal? nullable7 = current;
                        if ((nullable6.GetValueOrDefault() >= nullable7.GetValueOrDefault()) || !(nullable6.HasValue & nullable7.HasValue))
                        {
                            continue;
                        }
                    }
                    current = nullable2;
                }
                goto Label_00D0;
            Label_0087:
                nullable3 = enumerator.Current;
                if (current.HasValue)
                {
                    decimal? nullable8 = nullable3;
                    decimal? nullable9 = current;
                    if ((nullable8.GetValueOrDefault() <= nullable9.GetValueOrDefault()) || !(nullable8.HasValue & nullable9.HasValue))
                    {
                        goto Label_00C8;
                    }
                }
                current = nullable3;
            Label_00C8:
                if (enumerator.MoveNext())
                {
                    goto Label_0087;
                }
            Label_00D0:
                nullable4 = current;
            }
            return nullable4;
        }

        private class NullableDecimalMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<decimal?>
        {
            private int m_sign;
            private QueryOperatorEnumerator<decimal?, TKey> m_source;

            internal NullableDecimalMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<decimal?, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref decimal? currentElement)
            {
                QueryOperatorEnumerator<decimal?, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign != -1)
                {
                    decimal? nullable2 = null;
                    while (source.MoveNext(ref nullable2, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (currentElement.HasValue)
                        {
                            decimal? nullable5 = nullable2;
                            decimal? nullable6 = currentElement;
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
                    decimal? nullable = null;
                    while (source.MoveNext(ref nullable, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (currentElement.HasValue)
                        {
                            decimal? nullable3 = nullable;
                            decimal? nullable4 = currentElement;
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

