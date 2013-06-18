namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ResolveNextArgumentWorkItem : ActivityExecutionWorkItem
    {
        [DataMember(EmitDefaultValue=false)]
        private IDictionary<string, object> argumentValueOverrides;
        [DataMember(EmitDefaultValue=false)]
        private int nextArgumentIndex;
        [DataMember(EmitDefaultValue=false)]
        private Location resultLocation;

        public ResolveNextArgumentWorkItem()
        {
            base.IsPooled = true;
        }

        public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            base.ActivityInstance.ResolveArguments(executor, this.argumentValueOverrides, this.resultLocation, this.nextArgumentIndex);
            return true;
        }

        public void Initialize(System.Activities.ActivityInstance activityInstance, int nextArgumentIndex, IDictionary<string, object> argumentValueOverrides, Location resultLocation)
        {
            base.Reinitialize(activityInstance);
            this.nextArgumentIndex = nextArgumentIndex;
            this.argumentValueOverrides = argumentValueOverrides;
            this.resultLocation = resultLocation;
        }

        protected override void ReleaseToPool(ActivityExecutor executor)
        {
            base.ClearForReuse();
            this.nextArgumentIndex = 0;
            this.resultLocation = null;
            this.argumentValueOverrides = null;
            executor.ResolveNextArgumentWorkItemPool.Release(this);
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

