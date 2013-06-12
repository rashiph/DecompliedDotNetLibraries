namespace System.Linq.Parallel
{
    using System;
    using System.Threading;

    internal abstract class InlinedAggregationOperatorEnumerator<TIntermediate> : QueryOperatorEnumerator<TIntermediate, int>
    {
        protected CancellationToken m_cancellationToken;
        private int m_partitionIndex;

        internal InlinedAggregationOperatorEnumerator(int partitionIndex, CancellationToken cancellationToken)
        {
            this.m_partitionIndex = partitionIndex;
            this.m_cancellationToken = cancellationToken;
        }

        internal sealed override bool MoveNext(ref TIntermediate currentElement, ref int currentKey)
        {
            if (this.MoveNextCore(ref currentElement))
            {
                currentKey = this.m_partitionIndex;
                return true;
            }
            return false;
        }

        protected abstract bool MoveNextCore(ref TIntermediate currentElement);
    }
}

