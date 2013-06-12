namespace System.Threading.Tasks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal sealed class ThreadPoolTaskScheduler : TaskScheduler
    {
        private static ParameterizedThreadStart s_longRunningThreadWork = new ParameterizedThreadStart(ThreadPoolTaskScheduler.LongRunningThreadWork);

        internal ThreadPoolTaskScheduler()
        {
        }

        private IEnumerable<Task> FilterTasksFromWorkItems(IEnumerable<IThreadPoolWorkItem> tpwItems)
        {
            foreach (IThreadPoolWorkItem iteratorVariable0 in tpwItems)
            {
                if (iteratorVariable0 is Task)
                {
                    yield return (Task) iteratorVariable0;
                }
            }
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this.FilterTasksFromWorkItems(ThreadPool.GetQueuedWorkItems());
        }

        private static void LongRunningThreadWork(object obj)
        {
            (obj as Task).ExecuteEntry(false);
        }

        internal override void NotifyWorkItemProgress()
        {
            ThreadPool.NotifyWorkItemProgress();
        }

        [SecurityCritical]
        protected internal override void QueueTask(Task task)
        {
            if (TplEtwProvider.Log.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                Task internalCurrent = Task.InternalCurrent;
                Task parent = task.m_parent;
                TplEtwProvider.Log.TaskScheduled(base.Id, (internalCurrent == null) ? 0 : internalCurrent.Id, task.Id, (parent == null) ? 0 : parent.Id, (int) task.Options);
            }
            if ((task.Options & TaskCreationOptions.LongRunning) != TaskCreationOptions.None)
            {
                new Thread(s_longRunningThreadWork) { IsBackground = true }.Start(task);
            }
            else
            {
                bool forceGlobal = (task.Options & TaskCreationOptions.PreferFairness) != TaskCreationOptions.None;
                ThreadPool.UnsafeQueueCustomWorkItem(task, forceGlobal);
            }
        }

        [SecurityCritical]
        protected internal override bool TryDequeue(Task task)
        {
            return ThreadPool.TryPopCustomWorkItem(task);
        }

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued && !ThreadPool.TryPopCustomWorkItem(task))
            {
                return false;
            }
            bool flag = false;
            try
            {
                flag = task.ExecuteEntry(false);
            }
            finally
            {
                if (taskWasPreviouslyQueued)
                {
                    this.NotifyWorkItemProgress();
                }
            }
            return flag;
        }

        internal override bool RequiresAtomicStartTransition
        {
            get
            {
                return false;
            }
        }

    }
}

