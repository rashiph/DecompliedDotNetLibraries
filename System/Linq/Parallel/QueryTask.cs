namespace System.Linq.Parallel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class QueryTask
    {
        protected QueryTaskGroupState m_groupState;
        protected int m_taskIndex;
        private static Action<object> s_baseWorkDelegate;
        private static Action<object> s_runTaskSynchronouslyDelegate = new Action<object>(QueryTask.RunTaskSynchronously);

        static QueryTask()
        {
            s_baseWorkDelegate = o => ((QueryTask) o).BaseWork(null);
        }

        protected QueryTask(int taskIndex, QueryTaskGroupState groupState)
        {
            this.m_taskIndex = taskIndex;
            this.m_groupState = groupState;
        }

        private void BaseWork(object unused)
        {
            PlinqEtwProvider.Log.ParallelQueryFork(this.m_groupState.QueryId);
            try
            {
                this.Work();
            }
            finally
            {
                PlinqEtwProvider.Log.ParallelQueryJoin(this.m_groupState.QueryId);
            }
        }

        internal Task RunAsynchronously(TaskScheduler taskScheduler)
        {
            return Task.Factory.StartNew(s_baseWorkDelegate, this, new CancellationToken(), TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, taskScheduler);
        }

        internal Task RunSynchronously(TaskScheduler taskScheduler)
        {
            Task task = new Task(s_runTaskSynchronouslyDelegate, this, TaskCreationOptions.AttachedToParent);
            task.RunSynchronously(taskScheduler);
            return task;
        }

        private static void RunTaskSynchronously(object o)
        {
            ((QueryTask) o).BaseWork(null);
        }

        protected abstract void Work();
    }
}

