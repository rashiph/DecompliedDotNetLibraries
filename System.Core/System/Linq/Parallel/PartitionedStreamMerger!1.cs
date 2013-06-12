namespace System.Linq.Parallel
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    internal class PartitionedStreamMerger<TOutput> : IPartitionedStreamRecipient<TOutput>
    {
        private CancellationState m_cancellationState;
        private bool m_forEffectMerge;
        private bool m_isOrdered;
        private MergeExecutor<TOutput> m_mergeExecutor;
        private ParallelMergeOptions m_mergeOptions;
        private int m_queryId;
        private TaskScheduler m_taskScheduler;

        internal PartitionedStreamMerger(bool forEffectMerge, ParallelMergeOptions mergeOptions, TaskScheduler taskScheduler, bool outputOrdered, CancellationState cancellationState, int queryId)
        {
            this.m_forEffectMerge = forEffectMerge;
            this.m_mergeOptions = mergeOptions;
            this.m_isOrdered = outputOrdered;
            this.m_taskScheduler = taskScheduler;
            this.m_cancellationState = cancellationState;
            this.m_queryId = queryId;
        }

        public void Receive<TKey>(PartitionedStream<TOutput, TKey> partitionedStream)
        {
            this.m_mergeExecutor = MergeExecutor<TOutput>.Execute<TKey>(partitionedStream, this.m_forEffectMerge, this.m_mergeOptions, this.m_taskScheduler, this.m_isOrdered, this.m_cancellationState, this.m_queryId);
        }

        internal MergeExecutor<TOutput> MergeExecutor
        {
            get
            {
                return this.m_mergeExecutor;
            }
        }
    }
}

