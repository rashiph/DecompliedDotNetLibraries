namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [Serializable]
    internal sealed class CorrelationTokenInvalidatedHandler : IActivityEventListener<CorrelationTokenEventArgs>
    {
        private IActivityEventListener<QueueEventArgs> eventHandler;
        private string followerOperation;
        private Guid instanceId;
        private Type interfaceType;
        private bool queueCreator;
        private EventQueueName queueName;
        private Guid subscriptionId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CorrelationTokenInvalidatedHandler(Type interfaceType, string operation, IActivityEventListener<QueueEventArgs> eventHandler, Guid instanceId)
        {
            this.eventHandler = eventHandler;
            this.interfaceType = interfaceType;
            this.followerOperation = operation;
            this.instanceId = instanceId;
        }

        private void CreateSubscription(Guid instanceId, ActivityExecutionContext context, ICollection<CorrelationProperty> correlationValues)
        {
            WorkflowQueuingService service = context.GetService<WorkflowQueuingService>();
            EventQueueName queueName = new EventQueueName(this.interfaceType, this.followerOperation, correlationValues);
            WorkflowQueue workflowQueue = null;
            if (!service.Exists(queueName))
            {
                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "CorrelationTokenInvalidatedHandler: creating q {0} ", new object[] { queueName.GetHashCode() });
                workflowQueue = service.CreateWorkflowQueue(queueName, true);
                this.queueCreator = true;
            }
            else
            {
                workflowQueue = service.GetWorkflowQueue(queueName);
            }
            if (this.eventHandler != null)
            {
                workflowQueue.RegisterForQueueItemAvailable(this.eventHandler);
            }
            WorkflowSubscriptionService service2 = (WorkflowSubscriptionService) context.GetService(typeof(WorkflowSubscriptionService));
            MessageEventSubscription subscription = new MessageEventSubscription(queueName, instanceId);
            this.queueName = queueName;
            this.subscriptionId = subscription.SubscriptionId;
            subscription.InterfaceType = this.interfaceType;
            subscription.MethodName = this.followerOperation;
            this.interfaceType = null;
            this.followerOperation = null;
            if (correlationValues != null)
            {
                foreach (CorrelationProperty property in correlationValues)
                {
                    subscription.CorrelationProperties.Add(property);
                }
            }
            if ((this.eventHandler == null) && (service2 != null))
            {
                service2.CreateSubscription(subscription);
            }
        }

        private void DeleteSubscription(ActivityExecutionContext context)
        {
            if (this.queueName != null)
            {
                WorkflowQueuingService service = context.GetService<WorkflowQueuingService>();
                if (this.queueCreator)
                {
                    service.DeleteWorkflowQueue(this.queueName);
                }
                if (this.eventHandler == null)
                {
                    WorkflowSubscriptionService service2 = context.GetService<WorkflowSubscriptionService>();
                    if (service2 != null)
                    {
                        service2.DeleteSubscription(this.subscriptionId);
                    }
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "CorrelationTokenInvalidatedHandler subscription deleted SubId {0} QueueId {1}", new object[] { this.subscriptionId, this.queueName });
                }
            }
        }

        void IActivityEventListener<CorrelationTokenEventArgs>.OnEvent(object sender, CorrelationTokenEventArgs dataChangeEventArgs)
        {
            if (sender == null)
            {
                throw new ArgumentException("sender");
            }
            if (dataChangeEventArgs == null)
            {
                throw new ArgumentException("dataChangeEventArgs");
            }
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            Activity activity = context.Activity;
            ICollection<CorrelationProperty> properties = dataChangeEventArgs.CorrelationToken.Properties;
            if (dataChangeEventArgs.IsInitializing)
            {
                this.CreateSubscription(this.instanceId, context, properties);
            }
            else
            {
                if ((this.queueName != null) && !CorrelationResolver.IsInitializingMember(this.queueName.InterfaceType, this.queueName.MethodName, (properties == null) ? null : new object[] { properties }))
                {
                    this.DeleteSubscription(context);
                }
                dataChangeEventArgs.CorrelationToken.UnsubscribeFromCorrelationTokenInitializedEvent(activity, this);
            }
        }
    }
}

