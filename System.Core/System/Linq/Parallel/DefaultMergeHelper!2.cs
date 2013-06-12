namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class DefaultMergeHelper<TInputOutput, TIgnoreKey> : IMergeHelper<TInputOutput>
    {
        private AsynchronousChannel<TInputOutput>[] m_asyncChannels;
        private IEnumerator<TInputOutput> m_channelEnumerator;
        private bool m_ignoreOutput;
        private PartitionedStream<TInputOutput, TIgnoreKey> m_partitions;
        private SynchronousChannel<TInputOutput>[] m_syncChannels;
        private QueryTaskGroupState m_taskGroupState;
        private TaskScheduler m_taskScheduler;

        internal DefaultMergeHelper(PartitionedStream<TInputOutput, TIgnoreKey> partitions, bool ignoreOutput, ParallelMergeOptions options, TaskScheduler taskScheduler, CancellationState cancellationState, int queryId)
        {
            this.m_taskGroupState = new QueryTaskGroupState(cancellationState, queryId);
            this.m_partitions = partitions;
            this.m_taskScheduler = taskScheduler;
            this.m_ignoreOutput = ignoreOutput;
            if (!ignoreOutput)
            {
                if (options != ParallelMergeOptions.FullyBuffered)
                {
                    if (partitions.PartitionCount > 1)
                    {
                        this.m_asyncChannels = MergeExecutor<TInputOutput>.MakeAsynchronousChannels(partitions.PartitionCount, options, cancellationState.MergedCancellationToken);
                        this.m_channelEnumerator = new AsynchronousChannelMergeEnumerator<TInputOutput>(this.m_taskGroupState, this.m_asyncChannels);
                    }
                    else
                    {
                        this.m_channelEnumerator = ExceptionAggregator.WrapQueryEnumerator<TInputOutput, TIgnoreKey>(partitions[0], this.m_taskGroupState.CancellationState).GetEnumerator();
                    }
                }
                else
                {
                    this.m_syncChannels = MergeExecutor<TInputOutput>.MakeSynchronousChannels(partitions.PartitionCount);
                    this.m_channelEnumerator = new SynchronousChannelMergeEnumerator<TInputOutput>(this.m_taskGroupState, this.m_syncChannels);
                }
            }
        }

        public TInputOutput[] GetResultsAsArray()
        {
            if (this.m_syncChannels != null)
            {
                int num = 0;
                for (int i = 0; i < this.m_syncChannels.Length; i++)
                {
                    num += this.m_syncChannels[i].Count;
                }
                TInputOutput[] array = new TInputOutput[num];
                int arrayIndex = 0;
                for (int j = 0; j < this.m_syncChannels.Length; j++)
                {
                    this.m_syncChannels[j].CopyTo(array, arrayIndex);
                    arrayIndex += this.m_syncChannels[j].Count;
                }
                return array;
            }
            List<TInputOutput> list = new List<TInputOutput>();
            using (IEnumerator<TInputOutput> enumerator = ((IMergeHelper<TInputOutput>) this).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
            }
            return list.ToArray();
        }

        void IMergeHelper<TInputOutput>.Execute()
        {
            if (this.m_asyncChannels != null)
            {
                SpoolingTask.SpoolPipeline<TInputOutput, TIgnoreKey>(this.m_taskGroupState, this.m_partitions, this.m_asyncChannels, this.m_taskScheduler);
            }
            else if (this.m_syncChannels != null)
            {
                SpoolingTask.SpoolStopAndGo<TInputOutput, TIgnoreKey>(this.m_taskGroupState, this.m_partitions, this.m_syncChannels, this.m_taskScheduler);
            }
            else if (this.m_ignoreOutput)
            {
                SpoolingTask.SpoolForAll<TInputOutput, TIgnoreKey>(this.m_taskGroupState, this.m_partitions, this.m_taskScheduler);
            }
        }

        IEnumerator<TInputOutput> IMergeHelper<TInputOutput>.GetEnumerator()
        {
            return this.m_channelEnumerator;
        }
    }
}

