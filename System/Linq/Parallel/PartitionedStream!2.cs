namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class PartitionedStream<TElement, TKey>
    {
        private readonly System.Linq.Parallel.OrdinalIndexState m_indexState;
        private readonly IComparer<TKey> m_keyComparer;
        protected QueryOperatorEnumerator<TElement, TKey>[] m_partitions;

        internal PartitionedStream(int partitionCount, IComparer<TKey> keyComparer, System.Linq.Parallel.OrdinalIndexState indexState)
        {
            this.m_partitions = new QueryOperatorEnumerator<TElement, TKey>[partitionCount];
            this.m_keyComparer = keyComparer;
            this.m_indexState = indexState;
        }

        internal QueryOperatorEnumerator<TElement, TKey> this[int index]
        {
            get
            {
                return this.m_partitions[index];
            }
            set
            {
                this.m_partitions[index] = value;
            }
        }

        internal IComparer<TKey> KeyComparer
        {
            get
            {
                return this.m_keyComparer;
            }
        }

        internal System.Linq.Parallel.OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return this.m_indexState;
            }
        }

        public int PartitionCount
        {
            get
            {
                return this.m_partitions.Length;
            }
        }
    }
}

