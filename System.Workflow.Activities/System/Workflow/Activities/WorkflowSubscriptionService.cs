namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    public abstract class WorkflowSubscriptionService
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowSubscriptionService()
        {
        }

        public abstract void CreateSubscription(MessageEventSubscription subscription);
        public abstract void DeleteSubscription(Guid subscriptionId);
    }
}

