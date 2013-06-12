namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class NullableFloatMinMaxAggregationOperator : InlinedAggregationOperator<float?, float?, float?>
    {
        private readonly int m_sign;

        internal NullableFloatMinMaxAggregationOperator(IEnumerable<float?> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<float?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<float?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableFloatMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override float? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            float? nullable4;
            using (IEnumerator<float?> enumerator = this.GetEnumerator(3, true))
            {
                float? nullable3;
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                float? current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_00F0;
                }
                while (enumerator.MoveNext())
                {
                    float? nullable2 = enumerator.Current;
                    if (nullable2.HasValue)
                    {
                        if (current.HasValue)
                        {
                            float? nullable6 = nullable2;
                            float? nullable7 = current;
                            if (((nullable6.GetValueOrDefault() >= nullable7.GetValueOrDefault()) || !(nullable6.HasValue & nullable7.HasValue)) && !float.IsNaN(nullable2.GetValueOrDefault()))
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
                        float? nullable8 = nullable3;
                        float? nullable9 = current;
                        if (((nullable8.GetValueOrDefault() <= nullable9.GetValueOrDefault()) || !(nullable8.HasValue & nullable9.HasValue)) && !float.IsNaN(current.GetValueOrDefault()))
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

        private class NullableFloatMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<float?>
        {
            private int m_sign;
            private QueryOperatorEnumerator<float?, TKey> m_source;

            internal NullableFloatMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<float?, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref float? currentElement)
            {
                QueryOperatorEnumerator<float?, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign != -1)
                {
                    float? nullable2 = null;
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
                                float? nullable5 = nullable2;
                                float? nullable6 = currentElement;
                                if (((nullable5.GetValueOrDefault() <= nullable6.GetValueOrDefault()) || !(nullable5.HasValue & nullable6.HasValue)) && !float.IsNaN(currentElement.GetValueOrDefault()))
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
                    float? nullable = null;
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
                                float? nullable3 = nullable;
                                float? nullable4 = currentElement;
                                if (((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue)) && !float.IsNaN(nullable.GetValueOrDefault()))
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

