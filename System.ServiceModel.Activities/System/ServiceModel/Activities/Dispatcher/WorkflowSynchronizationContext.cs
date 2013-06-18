namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Threading;

    internal class WorkflowSynchronizationContext : SynchronizationContext
    {
        private static WorkflowSynchronizationContext singletonInstance;

        private WorkflowSynchronizationContext()
        {
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            this.Send(d, state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            base.Send(d, state);
        }

        public static WorkflowSynchronizationContext Instance
        {
            get
            {
                if (singletonInstance == null)
                {
                    singletonInstance = new WorkflowSynchronizationContext();
                }
                return singletonInstance;
            }
        }
    }
}

