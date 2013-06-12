namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ParallelForReplicatingTask : Task
    {
        private int m_replicationDownCount;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal ParallelForReplicatingTask(ParallelOptions parallelOptions, Action action, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions) : base(action, null, Task.InternalCurrent, CancellationToken.None, creationOptions, internalOptions | InternalTaskOptions.SelfReplicating, null)
        {
            this.m_replicationDownCount = parallelOptions.EffectiveMaxConcurrencyLevel;
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        internal override Task CreateReplicaTask(Action<object> taskReplicaDelegate, object stateObject, Task parentTask, TaskScheduler taskScheduler, TaskCreationOptions creationOptionsForReplica, InternalTaskOptions internalOptionsForReplica)
        {
            return new ParallelForReplicaTask(taskReplicaDelegate, stateObject, parentTask, taskScheduler, creationOptionsForReplica, internalOptionsForReplica);
        }

        internal override bool ShouldReplicate()
        {
            if (this.m_replicationDownCount == -1)
            {
                return true;
            }
            if (this.m_replicationDownCount > 0)
            {
                this.m_replicationDownCount--;
                return true;
            }
            return false;
        }
    }
}

