namespace System.Threading.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;

    internal sealed class SynchronizationContextTaskScheduler : TaskScheduler
    {
        private SynchronizationContext m_synchronizationContext;
        private static SendOrPostCallback s_postCallback = new SendOrPostCallback(SynchronizationContextTaskScheduler.PostCallback);

        internal SynchronizationContextTaskScheduler()
        {
            SynchronizationContext current = SynchronizationContext.Current;
            if (current == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("TaskScheduler_FromCurrentSynchronizationContext_NoCurrent"));
            }
            this.m_synchronizationContext = current;
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        private static void PostCallback(object obj)
        {
            ((Task) obj).ExecuteEntry(true);
        }

        [SecurityCritical]
        protected internal override void QueueTask(Task task)
        {
            this.m_synchronizationContext.Post(s_postCallback, task);
        }

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return ((SynchronizationContext.Current == this.m_synchronizationContext) && base.TryExecuteTask(task));
        }

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return 1;
            }
        }
    }
}

