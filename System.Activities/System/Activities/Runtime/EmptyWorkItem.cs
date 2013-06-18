namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal class EmptyWorkItem : ActivityExecutionWorkItem
    {
        public EmptyWorkItem()
        {
            base.IsPooled = true;
            base.IsEmpty = true;
        }

        public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            return true;
        }

        public void Initialize(System.Activities.ActivityInstance activityInstance)
        {
            base.Reinitialize(activityInstance);
        }

        protected override void ReleaseToPool(ActivityExecutor executor)
        {
            base.ClearForReuse();
            executor.EmptyWorkItemPool.Release(this);
        }

        public override void TraceCompleted()
        {
            base.TraceRuntimeWorkItemCompleted();
        }

        public override void TraceScheduled()
        {
            base.TraceRuntimeWorkItemScheduled();
        }

        public override void TraceStarting()
        {
            base.TraceRuntimeWorkItemStarting();
        }
    }
}

