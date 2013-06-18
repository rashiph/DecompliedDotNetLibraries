namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    [Serializable]
    public class TimerEventSubscription
    {
        private DateTime expiresAt;
        private IComparable queueName;
        private Guid subscriptionId;
        private Guid workflowInstanceId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TimerEventSubscription()
        {
        }

        public TimerEventSubscription(Guid workflowInstanceId, DateTime expiresAt) : this(Guid.NewGuid(), workflowInstanceId, expiresAt)
        {
        }

        public TimerEventSubscription(Guid timerId, Guid workflowInstanceId, DateTime expiresAt)
        {
            this.queueName = timerId;
            this.workflowInstanceId = workflowInstanceId;
            this.subscriptionId = timerId;
            this.expiresAt = expiresAt;
        }

        public virtual DateTime ExpiresAt
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.expiresAt;
            }
        }

        public virtual IComparable QueueName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.queueName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected set
            {
                this.queueName = value;
            }
        }

        public virtual Guid SubscriptionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subscriptionId;
            }
        }

        public virtual Guid WorkflowInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.workflowInstanceId;
            }
        }
    }
}

