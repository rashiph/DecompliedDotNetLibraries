namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime.DurableInstancing;

    internal class DetectActivatableWorkflowsTask : PersistenceTask
    {
        public DetectActivatableWorkflowsTask(SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, TimeSpan taskInterval) : base(store, storeLock, new DetectActivatableWorkflowsCommand(), taskInterval, false)
        {
        }

        protected override void HandleError(Exception exception)
        {
            if (TD.RunnableInstancesDetectionErrorIsEnabled())
            {
                TD.RunnableInstancesDetectionError(exception);
            }
            base.ResetTimer(false);
        }

        public override void ResetTimer(bool fireImmediately, TimeSpan? taskIntervalOverride)
        {
            InstanceOwner owner;
            if (base.Store.FindEvent(InstancePersistenceEvent<HasActivatableWorkflowEvent>.Value, out owner) != null)
            {
                base.ResetTimer(fireImmediately, taskIntervalOverride);
            }
        }
    }
}

