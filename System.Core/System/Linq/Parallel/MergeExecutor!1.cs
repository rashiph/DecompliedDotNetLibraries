namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MergeExecutor<TInputOutput> : IEnumerable<TInputOutput>, IEnumerable
    {
        private IMergeHelper<TInputOutput> m_mergeHelper;

        private MergeExecutor()
        {
        }

        private void Execute()
        {
            this.m_mergeHelper.Execute();
        }

        internal static MergeExecutor<TInputOutput> Execute<TKey>(PartitionedStream<TInputOutput, TKey> partitions, bool ignoreOutput, ParallelMergeOptions options, TaskScheduler taskScheduler, bool isOrdered, CancellationState cancellationState, int queryId)
        {
            MergeExecutor<TInputOutput> executor = new MergeExecutor<TInputOutput>();
            if (isOrdered && !ignoreOutput)
            {
                if ((options != ParallelMergeOptions.FullyBuffered) && !partitions.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing))
                {
                    bool autoBuffered = options == ParallelMergeOptions.AutoBuffered;
                    if (partitions.PartitionCount > 1)
                    {
                        executor.m_mergeHelper = new OrderPreservingPipeliningMergeHelper<TInputOutput>((PartitionedStream<TInputOutput, int>) partitions, taskScheduler, cancellationState, autoBuffered, queryId);
                    }
                    else
                    {
                        executor.m_mergeHelper = new DefaultMergeHelper<TInputOutput, TKey>(partitions, false, options, taskScheduler, cancellationState, queryId);
                    }
                }
                else
                {
                    executor.m_mergeHelper = new OrderPreservingMergeHelper<TInputOutput, TKey>(partitions, taskScheduler, cancellationState, queryId);
                }
            }
            else
            {
                executor.m_mergeHelper = new DefaultMergeHelper<TInputOutput, TKey>(partitions, ignoreOutput, options, taskScheduler, cancellationState, queryId);
            }
            executor.Execute();
            return executor;
        }

        public IEnumerator<TInputOutput> GetEnumerator()
        {
            return this.m_mergeHelper.GetEnumerator();
        }

        internal TInputOutput[] GetResultsAsArray()
        {
            return this.m_mergeHelper.GetResultsAsArray();
        }

        internal static AsynchronousChannel<TInputOutput>[] MakeAsynchronousChannels(int partitionCount, ParallelMergeOptions options, CancellationToken cancellationToken)
        {
            AsynchronousChannel<TInputOutput>[] channelArray = new AsynchronousChannel<TInputOutput>[partitionCount];
            int chunkSize = 0;
            if (options == ParallelMergeOptions.NotBuffered)
            {
                chunkSize = 1;
            }
            for (int i = 0; i < channelArray.Length; i++)
            {
                channelArray[i] = new AsynchronousChannel<TInputOutput>(chunkSize, cancellationToken);
            }
            return channelArray;
        }

        internal static SynchronousChannel<TInputOutput>[] MakeSynchronousChannels(int partitionCount)
        {
            SynchronousChannel<TInputOutput>[] channelArray = new SynchronousChannel<TInputOutput>[partitionCount];
            for (int i = 0; i < channelArray.Length; i++)
            {
                channelArray[i] = new SynchronousChannel<TInputOutput>();
            }
            return channelArray;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

