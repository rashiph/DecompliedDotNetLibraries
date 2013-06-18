namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal class EmptyWithCancelationCheckWorkItem : ActivityExecutionWorkItem
    {
        [DataMember]
        private System.Activities.ActivityInstance completedInstance;

        public EmptyWithCancelationCheckWorkItem(System.Activities.ActivityInstance activityInstance, System.Activities.ActivityInstance completedInstance) : base(activityInstance)
        {
            this.completedInstance = completedInstance;
            base.IsEmpty = true;
        }

        public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            return true;
        }

        public override void PostProcess(ActivityExecutor executor)
        {
            if ((this.completedInstance.State != ActivityInstanceState.Closed) && base.ActivityInstance.IsPerformingDefaultCancelation)
            {
                base.ActivityInstance.MarkCanceled();
            }
            base.PostProcess(executor);
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

