namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [Serializable]
    internal sealed class FollowerQueueCreator : IActivityEventListener<QueueEventArgs>
    {
        private string followerOperation;
        private object sync = new object();

        internal FollowerQueueCreator(string operation)
        {
            this.followerOperation = operation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            FollowerQueueCreator creator = obj as FollowerQueueCreator;
            return (this.followerOperation == creator.followerOperation);
        }

        public override int GetHashCode()
        {
            return this.followerOperation.GetHashCode();
        }

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs args)
        {
            lock (this.sync)
            {
                WorkflowQueue queue = (WorkflowQueue) sender;
                EventQueueName queueName = (EventQueueName) queue.QueueName;
                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "FollowerQueueCreator: initialized on operation {0} for follower {1}", new object[] { queueName.InterfaceType.Name + queueName.MethodName, this.followerOperation });
                IMethodMessage message = queue.Peek() as IMethodMessage;
                ICollection<CorrelationProperty> propertyValues = CorrelationResolver.ResolveCorrelationValues(queueName.InterfaceType, queueName.MethodName, message.Args, false);
                EventQueueName name2 = new EventQueueName(queueName.InterfaceType, this.followerOperation, propertyValues);
                if (!queue.QueuingService.Exists(name2))
                {
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "FollowerQueueCreator::CreateQueue creating q {0}", new object[] { name2.GetHashCode() });
                    queue.QueuingService.CreateWorkflowQueue(name2, true);
                }
            }
        }
    }
}

