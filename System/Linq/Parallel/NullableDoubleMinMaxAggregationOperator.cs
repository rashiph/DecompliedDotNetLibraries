namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableDoubleMinMaxAggregationOperator : InlinedAggregationOperator<double?, double?, double?>
    {
        private readonly int m_sign;

        internal NullableDoubleMinMaxAggregationOperator(IEnumerable<double?> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<double?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<double?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableDoubleMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override double? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            double? nullable4;
            using (IEnumerator<double?> enumerator = this.GetEnumerator(3, true))
            {
                double? nullable3;
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                double? current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_00F0;
                }
                while (enumerator.MoveNext())
                {
                    double? nullable2 = enumerator.Current;
                    if (nullable2.HasValue)
                    {
                        if (current.HasValue)
                        {
                            double? nullable6 = nullable2;
                            double? nullable7 = current;
                            if (((nullable6.GetValueOrDefault() >= nullable7.GetValueOrDefault()) || !(nullable6.HasValue & nullable7.HasValue)) && !double.IsNaN(nullable2.GetValueOrDefault()))
                            {
                                continue;
                            }
                        }
                        current = nullable2;
                    }
                }
                goto Label_00F8;
            Label_009B:
                nullable3 = enumerator.Current;
                if (nullable3.HasValue)
                {
                    if (current.HasValue)
                    {
                        double? nullable8 = nullable3;
                        double? nullable9 = current;
                        if (((nullable8.GetValueOrDefault() <= nullable9.GetValueOrDefault()) || !(nullable8.HasValue & nullable9.HasValue)) && !double.IsNaN(current.GetValueOrDefault()))
                        {
                            goto Label_00F0;
                        }
                    }
                    current = nullable3;
                }
            Label_00F0:
                if (enumerator.MoveNext())
                {
                    goto Label_009B;
                }
            Label_00F8:
                nullable4 = current;
            }
            return nullable4;
        }

        private class NullableDoubleMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<double?>
        {
            private int m_sign;
            private QueryOperatorEnumerator<double?, TKey> m_source;

            internal NullableDoubleMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<double?, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref double? currentElement)
            {
                QueryOperatorEnumerator<double?, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign != -1)
                {
                    double? nullable2 = null;
                    while (source.MoveNext(ref nullable2, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (nullable2.HasValue)
                        {
                            if (currentElement.HasValue)
                            {
                                double? nullable5 = nullable2;
                                double? nullable6 = currentElement;
                                if (((nullable5.GetValueOrDefault() <= nullable6.GetValueOrDefault()) || !(nullable5.HasValue & nullable6.HasValue)) && !double.IsNaN(currentElement.GetValueOrDefault()))
                                {
                                    continue;
                                }
                            }
                            currentElement = nullable2;
                        }
                    }
                }
                else
                {
                    double? nullable = null;
                    while (source.MoveNext(ref nullable, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if (nullable.HasValue)
                        {
                            if (currentElement.HasValue)
                            {
                                double? nullable3 = nullable;
                                double? nullable4 = currentElement;
                                if (((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue)) && !double.IsNaN(nullable.GetValueOrDefault()))
                                {
                                    continue;
                                }
                            }
                            currentElement = nullable;
                        }
                    }
                }
                return true;
            }
        }
    }
}

