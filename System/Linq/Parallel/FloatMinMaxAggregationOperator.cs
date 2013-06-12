namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class FloatMinMaxAggregationOperator : InlinedAggregationOperator<float, float, float>
    {
        private readonly int m_sign;

        internal FloatMinMaxAggregationOperator(IEnumerable<float> child, int sign) : base(child)
        {
            this.m_sign = sign;
        }

        protected override QueryOperatorEnumerator<float, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<float, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new FloatMinMaxAggregationOperatorEnumerator<TKey>(source, index, this.m_sign, cancellationToken);
        }

        protected override float InternalAggregate(ref Exception singularExceptionToThrow)
        {
            float num4;
            using (IEnumerator<float> enumerator = this.GetEnumerator(3, true))
            {
                float num3;
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    return 0f;
                }
                float current = enumerator.Current;
                if (this.m_sign != -1)
                {
                    goto Label_0076;
                }
                while (enumerator.MoveNext())
                {
                    float f = enumerator.Current;
                    if ((f < current) || float.IsNaN(f))
                    {
                        current = f;
                    }
                }
                goto Label_007E;
            Label_0061:
                num3 = enumerator.Current;
                if ((num3 > current) || float.IsNaN(current))
                {
                    current = num3;
                }
            Label_0076:
                if (enumerator.MoveNext())
                {
                    goto Label_0061;
                }
            Label_007E:
                num4 = current;
            }
            return num4;
        }

        private class FloatMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<float>
        {
            private int m_sign;
            private QueryOperatorEnumerator<float, TKey> m_source;

            internal FloatMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<float, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken) : base(partitionIndex, cancellationToken)
            {
                this.m_source = source;
                this.m_sign = sign;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            protected override bool MoveNextCore(ref float currentElement)
            {
                QueryOperatorEnumerator<float, TKey> source = this.m_source;
                TKey currentKey = default(TKey);
                if (!source.MoveNext(ref currentElement, ref currentKey))
                {
                    return false;
                }
                int num = 0;
                if (this.m_sign == -1)
                {
                    float num2 = 0f;
                    while (source.MoveNext(ref num2, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if ((num2 < currentElement) || float.IsNaN(num2))
                        {
                            currentElement = num2;
                        }
                    }
                }
                else
                {
                    float num3 = 0f;
                    while (source.MoveNext(ref num3, ref currentKey))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(base.m_cancellationToken);
                        }
                        if ((num3 > currentElement) || float.IsNaN(currentElement))
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

