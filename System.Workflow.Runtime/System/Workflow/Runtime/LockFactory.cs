namespace System.Workflow.Runtime
{
    using System;

    internal static class LockFactory
    {
        internal static InstanceLock CreateWorkflowExecutorLock(Guid id)
        {
            return new InstanceLock(id, "Workflow Executor Lock: " + id.ToString(), 50, LockPriorityOperator.GreaterThanOrReentrant);
        }

        internal static InstanceLock CreateWorkflowMessageDeliveryLock(Guid id)
        {
            return new InstanceLock(id, "Workflow Message Delivery Lock: " + id.ToString(), 0x23, LockPriorityOperator.GreaterThanOrReentrant);
        }

        internal static InstanceLock CreateWorkflowSchedulerLock(Guid id)
        {
            return new InstanceLock(id, "Workflow Scheduler Lock: " + id.ToString(), 40, LockPriorityOperator.GreaterThan);
        }
    }
}

