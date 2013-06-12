namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class RepeatEnumerable<TResult> : ParallelQuery<TResult>, IParallelPartitionable<TResult>
    {
        private int m_count;
        private TResult m_element;

        internal RepeatEnumerable(TResult element, int count) : base(QuerySettings.Empty)
        {
            this.m_element = element;
            this.m_count = count;
        }

        public override IEnumerator<TResult> GetEnumerator()
        {
            return new RepeatEnumerator<TResult>(this.m_element, this.m_count, 0).AsClassicEnumerator();
        }

        public QueryOperatorEnumerator<TResult, int>[] GetPartitions(int partitionCount)
        {
            int count = ((this.m_count + partitionCount) - 1) / partitionCount;
            QueryOperatorEnumerator<TResult, int>[] enumeratorArray = new QueryOperatorEnumerator<TResult, int>[partitionCount];
            int index = 0;
            for (int i = 0; index < partitionCount; i += count)
            {
                if ((i + count) > this.m_count)
                {
                    enumeratorArray[index] = new RepeatEnumerator<TResult>(this.m_element, (i < this.m_count) ? (this.m_count - i) : 0, i);
                }
                else
                {
                    enumeratorArray[index] = new RepeatEnumerator<TResult>(this.m_element, count, i);
                }
                index++;
            }
            return enumeratorArray;
        }

        private class RepeatEnumerator : QueryOperatorEnumerator<TResult, int>
        {
            private readonly int m_count;
            private Shared<int> m_currentIndex;
            private readonly TResult m_element;
            private readonly int m_indexOffset;

            internal RepeatEnumerator(TResult element, int count, int indexOffset)
            {
                this.m_element = element;
                this.m_count = count;
                this.m_indexOffset = indexOffset;
            }

            internal override bool MoveNext(ref TResult currentElement, ref int currentKey)
            {
                if (this.m_currentIndex == null)
                {
                    this.m_currentIndex = new Shared<int>(-1);
                }
                if (this.m_currentIndex.Value < (this.m_count - 1))
                {
                    this.m_currentIndex.Value += 1;
                    currentElement = this.m_element;
                    currentKey = this.m_currentIndex.Value + this.m_indexOffset;
                    return true;
                }
                return false;
            }

            internal override void Reset()
            {
                this.m_currentIndex = null;
            }
        }
    }
}

