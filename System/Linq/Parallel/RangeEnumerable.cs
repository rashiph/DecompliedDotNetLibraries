namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class RangeEnumerable : ParallelQuery<int>, IParallelPartitionable<int>
    {
        private int m_count;
        private int m_from;

        internal RangeEnumerable(int from, int count) : base(QuerySettings.Empty)
        {
            this.m_from = from;
            this.m_count = count;
        }

        public override IEnumerator<int> GetEnumerator()
        {
            return new RangeEnumerator(this.m_from, this.m_count, 0).AsClassicEnumerator();
        }

        public QueryOperatorEnumerator<int, int>[] GetPartitions(int partitionCount)
        {
            int num = this.m_count / partitionCount;
            int num2 = this.m_count % partitionCount;
            int initialIndex = 0;
            QueryOperatorEnumerator<int, int>[] enumeratorArray = new QueryOperatorEnumerator<int, int>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
            {
                int count = (i < num2) ? (num + 1) : num;
                enumeratorArray[i] = new RangeEnumerator(this.m_from + initialIndex, count, initialIndex);
                initialIndex += count;
            }
            return enumeratorArray;
        }

        private class RangeEnumerator : QueryOperatorEnumerator<int, int>
        {
            private readonly int m_count;
            private Shared<int> m_currentCount;
            private readonly int m_from;
            private readonly int m_initialIndex;

            internal RangeEnumerator(int from, int count, int initialIndex)
            {
                this.m_from = from;
                this.m_count = count;
                this.m_initialIndex = initialIndex;
            }

            internal override bool MoveNext(ref int currentElement, ref int currentKey)
            {
                if (this.m_currentCount == null)
                {
                    this.m_currentCount = new Shared<int>(-1);
                }
                int num = this.m_currentCount.Value + 1;
                if (num < this.m_count)
                {
                    this.m_currentCount.Value = num;
                    currentElement = num + this.m_from;
                    currentKey = num + this.m_initialIndex;
                    return true;
                }
                return false;
            }

            internal override void Reset()
            {
                this.m_currentCount = null;
            }
        }
    }
}

