namespace System.Workflow.Runtime
{
    using System;

    internal class ScheduleWork : IDisposable
    {
        protected ScheduleInfo oldValue;
        [ThreadStatic]
        protected static ScheduleInfo scheduleInfo;

        public ScheduleWork(WorkflowExecutor executor)
        {
            this.oldValue = scheduleInfo;
            scheduleInfo = new ScheduleInfo(executor, false);
        }

        public ScheduleWork(WorkflowExecutor executor, bool suppress)
        {
            this.oldValue = scheduleInfo;
            scheduleInfo = new ScheduleInfo(executor, suppress);
        }

        public virtual void Dispose()
        {
            if (scheduleInfo.scheduleWork && !scheduleInfo.suppress)
            {
                scheduleInfo.executor.RequestHostingService();
            }
            scheduleInfo = this.oldValue;
        }

        public static WorkflowExecutor Executor
        {
            set
            {
                scheduleInfo.executor = value;
            }
        }

        public static bool NeedsService
        {
            set
            {
                scheduleInfo.scheduleWork = value;
            }
        }

        internal class ScheduleInfo
        {
            public WorkflowExecutor executor;
            public bool scheduleWork;
            public bool suppress;

            public ScheduleInfo(WorkflowExecutor executor, bool suppress)
            {
                this.suppress = suppress;
                this.scheduleWork = false;
                this.executor = executor;
            }
        }
    }
}

