namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class MessageEventSubscription
    {
        private Type interfaceType;
        private string operation;
        private List<CorrelationProperty> predicates;
        private IComparable queueName;
        private Guid subscriptionId;
        private Guid workflowInstanceId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected MessageEventSubscription()
        {
        }

        public MessageEventSubscription(IComparable queueName, Guid instanceId) : this(queueName, instanceId, Guid.NewGuid())
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MessageEventSubscription(IComparable queueName, Guid instanceId, Guid subscriptionId) : this(queueName, instanceId, null, null, subscriptionId)
        {
        }

        public MessageEventSubscription(IComparable queueName, Guid subscriptionId, Type interfaceType, string operation) : this(queueName, Guid.Empty, interfaceType, operation, subscriptionId)
        {
        }

        public MessageEventSubscription(IComparable queueName, Guid instanceId, Type interfaceType, string operation, Guid subscriptionId)
        {
            this.queueName = queueName;
            this.workflowInstanceId = instanceId;
            this.subscriptionId = subscriptionId;
            this.interfaceType = interfaceType;
            this.operation = operation;
            this.predicates = new List<CorrelationProperty>();
        }

        public virtual ICollection<CorrelationProperty> CorrelationProperties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.predicates;
            }
        }

        public virtual Type InterfaceType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interfaceType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.interfaceType = value;
            }
        }

        public virtual string MethodName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.operation;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.operation = value;
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

