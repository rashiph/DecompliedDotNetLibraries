namespace System.Linq.Parallel
{
    using System;
    using System.Threading.Tasks;

    internal class OrderPreservingSpoolingTask<TInputOutput, TKey> : SpoolingTaskBase
    {
        private System.Linq.Parallel.Shared<TInputOutput[]> m_results;
        private SortHelper<TInputOutput> m_sortHelper;

        private OrderPreservingSpoolingTask(int taskIndex, QueryTaskGroupState groupState, System.Linq.Parallel.Shared<TInputOutput[]> results, SortHelper<TInputOutput> sortHelper) : base(taskIndex, groupState)
        {
            this.m_results = results;
            this.m_sortHelper = sortHelper;
        }

        internal static void Spool(QueryTaskGroupState groupState, PartitionedStream<TInputOutput, TKey> partitions, System.Linq.Parallel.Shared<TInputOutput[]> results, TaskScheduler taskScheduler)
        {
            int maxToRunInParallel = partitions.PartitionCount - 1;
            SortHelper<TInputOutput, TKey>[] sortHelpers = SortHelper<TInputOutput, TKey>.GenerateSortHelpers(partitions, groupState);
            Task rootTask = new Task(delegate {
                for (int k = 0; k < maxToRunInParallel; k++)
                {
                    new OrderPreservingSpoolingTask<TInputOutput, TKey>(k, groupState, results, sortHelpers[k]).RunAsynchronously(taskScheduler);
                }
                new OrderPreservingSpoolingTask<TInputOutput, TKey>(maxToRunInParallel, groupState, results, sortHelpers[maxToRunInParallel]).RunSynchronously(taskScheduler);
            });
            groupState.QueryBegin(rootTask);
            rootTask.RunSynchronously(taskScheduler);
            for (int j = 0; j < sortHelpers.Length; j++)
            {
                sortHelpers[j].Dispose();
            }
            groupState.QueryEnd(false);
        }

        protected override void SpoolingWork()
        {
            TInputOutput[] localArray = this.m_sortHelper.Sort();
            if (!base.m_groupState.CancellationState.MergedCancellationToken.IsCancellationRequested && (base.m_taskIndex == 0))
            {
                this.m_results.Value = localArray;
            }
        }
    }
}

