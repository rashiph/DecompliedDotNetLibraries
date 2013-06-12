namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class OrderPreservingMergeHelper<TInputOutput, TKey> : IMergeHelper<TInputOutput>
    {
        private PartitionedStream<TInputOutput, TKey> m_partitions;
        private System.Linq.Parallel.Shared<TInputOutput[]> m_results;
        private QueryTaskGroupState m_taskGroupState;
        private TaskScheduler m_taskScheduler;

        internal OrderPreservingMergeHelper(PartitionedStream<TInputOutput, TKey> partitions, TaskScheduler taskScheduler, CancellationState cancellationState, int queryId)
        {
            this.m_taskGroupState = new QueryTaskGroupState(cancellationState, queryId);
            this.m_partitions = partitions;
            this.m_results = new System.Linq.Parallel.Shared<TInputOutput[]>(null);
            this.m_taskScheduler = taskScheduler;
        }

        public TInputOutput[] GetResultsAsArray()
        {
            return this.m_results.Value;
        }

        void IMergeHelper<TInputOutput>.Execute()
        {
            OrderPreservingSpoolingTask<TInputOutput, TKey>.Spool(this.m_taskGroupState, this.m_partitions, this.m_results, this.m_taskScheduler);
        }

        IEnumerator<TInputOutput> IMergeHelper<TInputOutput>.GetEnumerator()
        {
            return ((IEnumerable<TInputOutput>) this.m_results.Value).GetEnumerator();
        }
    }
}

