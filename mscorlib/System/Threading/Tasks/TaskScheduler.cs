namespace System.Threading.Tasks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [DebuggerTypeProxy(typeof(TaskScheduler.SystemThreadingTasks_TaskSchedulerDebugView)), DebuggerDisplay("Id={Id}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public abstract class TaskScheduler
    {
        private static object _unobservedTaskExceptionLockObject = new object();
        private int m_taskSchedulerId;
        internal WeakReference m_weakReferenceToSelf;
        private static ConcurrentDictionary<WeakReference, object> s_activeTaskSchedulers;
        private static TaskScheduler s_defaultTaskScheduler = new ThreadPoolTaskScheduler();
        internal static int s_taskSchedulerIdCounter;

        private static  event EventHandler<UnobservedTaskExceptionEventArgs> _unobservedTaskException;

        public static  event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException
        {
            [SecurityCritical] add
            {
                if (value != null)
                {
                    RuntimeHelpers.PrepareContractedDelegate(value);
                    lock (_unobservedTaskExceptionLockObject)
                    {
                        _unobservedTaskException += value;
                    }
                }
            }
            [SecurityCritical] remove
            {
                lock (_unobservedTaskExceptionLockObject)
                {
                    _unobservedTaskException -= value;
                }
            }
        }

        protected TaskScheduler()
        {
            this.m_weakReferenceToSelf = new WeakReference(this);
            RegisterTaskScheduler(this);
        }

        ~TaskScheduler()
        {
            UnregisterTaskScheduler(this);
        }

        public static TaskScheduler FromCurrentSynchronizationContext()
        {
            return new SynchronizationContextTaskScheduler();
        }

        [SecurityCritical]
        protected abstract IEnumerable<Task> GetScheduledTasks();
        [SecurityCritical]
        internal Task[] GetScheduledTasksForDebugger()
        {
            IEnumerable<Task> scheduledTasks = this.GetScheduledTasks();
            if (scheduledTasks == null)
            {
                return null;
            }
            Task[] taskArray = scheduledTasks as Task[];
            if (taskArray == null)
            {
                taskArray = new List<Task>(scheduledTasks).ToArray();
            }
            foreach (Task task in taskArray)
            {
                int id = task.Id;
            }
            return taskArray;
        }

        [SecurityCritical]
        internal static TaskScheduler[] GetTaskSchedulersForDebugger()
        {
            TaskScheduler[] schedulerArray = new TaskScheduler[s_activeTaskSchedulers.Count];
            IEnumerator<KeyValuePair<WeakReference, object>> enumerator = s_activeTaskSchedulers.GetEnumerator();
            int num = 0;
            while (enumerator.MoveNext())
            {
                KeyValuePair<WeakReference, object> current = enumerator.Current;
                TaskScheduler target = current.Key.Target as TaskScheduler;
                if (target != null)
                {
                    schedulerArray[num++] = target;
                    int id = target.Id;
                }
            }
            return schedulerArray;
        }

        [SecuritySafeCritical]
        internal virtual object GetThreadStatics()
        {
            return null;
        }

        internal virtual void NotifyWorkItemProgress()
        {
        }

        internal static void PublishUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs ueea)
        {
            lock (_unobservedTaskExceptionLockObject)
            {
                EventHandler<UnobservedTaskExceptionEventArgs> handler = _unobservedTaskException;
                if (handler != null)
                {
                    handler(sender, ueea);
                }
            }
        }

        [SecurityCritical]
        protected internal abstract void QueueTask(Task task);
        internal static void RegisterTaskScheduler(TaskScheduler ts)
        {
            LazyInitializer.EnsureInitialized<ConcurrentDictionary<WeakReference, object>>(ref s_activeTaskSchedulers);
            s_activeTaskSchedulers.TryAdd(ts.m_weakReferenceToSelf, null);
        }

        [SecurityCritical]
        protected internal virtual bool TryDequeue(Task task)
        {
            return false;
        }

        [SecurityCritical]
        protected bool TryExecuteTask(Task task)
        {
            if (task.ExecutingTaskScheduler != this)
            {
                throw new InvalidOperationException(Environment.GetResourceString("TaskScheduler_ExecuteTask_WrongTaskScheduler"));
            }
            return task.ExecuteEntry(true);
        }

        [SecurityCritical]
        protected abstract bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued);
        [SecuritySafeCritical]
        internal bool TryRunInline(Task task, bool taskWasPreviouslyQueued)
        {
            return this.TryRunInline(task, taskWasPreviouslyQueued, this.GetThreadStatics());
        }

        [SecuritySafeCritical]
        internal bool TryRunInline(Task task, bool taskWasPreviouslyQueued, object threadStatics)
        {
            TaskScheduler executingTaskScheduler = task.ExecutingTaskScheduler;
            if ((executingTaskScheduler != this) && (executingTaskScheduler != null))
            {
                return executingTaskScheduler.TryRunInline(task, taskWasPreviouslyQueued);
            }
            if (((executingTaskScheduler == null) || (task.m_action == null)) || ((task.IsDelegateInvoked || task.IsCanceled) || !Task.CurrentStackGuard.TryBeginInliningScope()))
            {
                return false;
            }
            bool flag = false;
            try
            {
                flag = this.TryExecuteTaskInline(task, taskWasPreviouslyQueued);
            }
            finally
            {
                Task.CurrentStackGuard.EndInliningScope();
            }
            if ((flag && !task.IsDelegateInvoked) && !task.IsCanceled)
            {
                throw new InvalidOperationException(Environment.GetResourceString("TaskScheduler_InconsistentStateAfterTryExecuteTaskInline"));
            }
            return flag;
        }

        internal static void UnregisterTaskScheduler(TaskScheduler ts)
        {
            object obj2;
            s_activeTaskSchedulers.TryRemove(ts.m_weakReferenceToSelf, out obj2);
        }

        public static TaskScheduler Current
        {
            get
            {
                Task internalCurrent = Task.InternalCurrent;
                if (internalCurrent != null)
                {
                    return internalCurrent.ExecutingTaskScheduler;
                }
                return Default;
            }
        }

        public static TaskScheduler Default
        {
            get
            {
                return s_defaultTaskScheduler;
            }
        }

        public int Id
        {
            get
            {
                if (this.m_taskSchedulerId == 0)
                {
                    int num = 0;
                    do
                    {
                        num = Interlocked.Increment(ref s_taskSchedulerIdCounter);
                    }
                    while (num == 0);
                    Interlocked.CompareExchange(ref this.m_taskSchedulerId, num, 0);
                }
                return this.m_taskSchedulerId;
            }
        }

        public virtual int MaximumConcurrencyLevel
        {
            get
            {
                return 0x7fffffff;
            }
        }

        internal virtual bool RequiresAtomicStartTransition
        {
            get
            {
                return true;
            }
        }

        internal sealed class SystemThreadingTasks_TaskSchedulerDebugView
        {
            private readonly TaskScheduler m_taskScheduler;

            public SystemThreadingTasks_TaskSchedulerDebugView(TaskScheduler scheduler)
            {
                this.m_taskScheduler = scheduler;
            }

            public int Id
            {
                get
                {
                    return this.m_taskScheduler.Id;
                }
            }

            public IEnumerable<Task> ScheduledTasks
            {
                [SecurityCritical]
                get
                {
                    return this.m_taskScheduler.GetScheduledTasks();
                }
            }
        }
    }
}

