namespace System.Linq.Parallel
{
    using System;
    using System.Threading.Tasks;

    internal static class SpoolingTask
    {
        internal static void SpoolForAll<TInputOutput, TIgnoreKey>(QueryTaskGroupState groupState, PartitionedStream<TInputOutput, TIgnoreKey> partitions, TaskScheduler taskScheduler)
        {
            Task rootTask = new Task(delegate {
                int taskIndex = partitions.PartitionCount - 1;
                for (int j = 0; j < taskIndex; j++)
                {
                    new ForAllSpoolingTask<TInputOutput, TIgnoreKey>(j, groupState, partitions[j]).RunAsynchronously(taskScheduler);
                }
                new ForAllSpoolingTask<TInputOutput, TIgnoreKey>(taskIndex, groupState, partitions[taskIndex]).RunSynchronously(taskScheduler);
            });
            groupState.QueryBegin(rootTask);
            rootTask.RunSynchronously(taskScheduler);
            groupState.QueryEnd(false);
        }

        internal static void SpoolPipeline<TInputOutput, TIgnoreKey>(QueryTaskGroupState groupState, PartitionedStream<TInputOutput, TIgnoreKey> partitions, AsynchronousChannel<TInputOutput>[] channels, TaskScheduler taskScheduler)
        {
            Task rootTask = new Task(delegate {
                for (int j = 0; j < partitions.PartitionCount; j++)
                {
                    new PipelineSpoolingTask<TInputOutput, TIgnoreKey>(j, groupState, partitions[j], channels[j]).RunAsynchronously(taskScheduler);
                }
            });
            groupState.QueryBegin(rootTask);
            rootTask.Start(taskScheduler);
        }

        internal static void SpoolStopAndGo<TInputOutput, TIgnoreKey>(QueryTaskGroupState groupState, PartitionedStream<TInputOutput, TIgnoreKey> partitions, SynchronousChannel<TInputOutput>[] channels, TaskScheduler taskScheduler)
        {
            Task rootTask = new Task(delegate {
                int taskIndex = partitions.PartitionCount - 1;
                for (int j = 0; j < taskIndex; j++)
                {
                    new StopAndGoSpoolingTask<TInputOutput, TIgnoreKey>(j, groupState, partitions[j], channels[j]).RunAsynchronously(taskScheduler);
                }
                new StopAndGoSpoolingTask<TInputOutput, TIgnoreKey>(taskIndex, groupState, partitions[taskIndex], channels[taskIndex]).RunSynchronously(taskScheduler);
            });
            groupState.QueryBegin(rootTask);
            rootTask.RunSynchronously(taskScheduler);
            groupState.QueryEnd(false);
        }
    }
}

