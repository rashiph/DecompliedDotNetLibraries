namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal class ListQueryResults<T> : QueryResults<T>
    {
        private int m_partitionCount;
        private IList<T> m_source;
        private bool m_useStriping;

        internal ListQueryResults(IList<T> source, int partitionCount, bool useStriping)
        {
            this.m_source = source;
            this.m_partitionCount = partitionCount;
            this.m_useStriping = useStriping;
        }

        internal override T GetElement(int index)
        {
            return this.m_source[index];
        }

        internal PartitionedStream<T, int> GetPartitionedStream()
        {
            return ExchangeUtilities.PartitionDataSource<T>(this.m_source, this.m_partitionCount, this.m_useStriping);
        }

        internal override void GivePartitionedStream(IPartitionedStreamRecipient<T> recipient)
        {
            PartitionedStream<T, int> partitionedStream = this.GetPartitionedStream();
            recipient.Receive<int>(partitionedStream);
        }

        internal override int ElementsCount
        {
            get
            {
                return this.m_source.Count;
            }
        }

        internal override bool IsIndexible
        {
            get
            {
                return true;
            }
        }
    }
}

