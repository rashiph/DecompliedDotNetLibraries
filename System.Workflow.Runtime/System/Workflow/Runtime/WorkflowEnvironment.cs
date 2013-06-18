namespace System.Workflow.Runtime
{
    using System;

    public static class WorkflowEnvironment
    {
        public static IWorkBatch WorkBatch
        {
            get
            {
                IWorkBatch workBatch = ServiceEnvironment.WorkBatch;
                if (workBatch == null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.WorkBatchNotFound);
                }
                return workBatch;
            }
        }

        public static Guid WorkflowInstanceId
        {
            get
            {
                Guid workflowInstanceId = ServiceEnvironment.WorkflowInstanceId;
                if (workflowInstanceId == Guid.Empty)
                {
                    throw new InvalidOperationException(ExecutionStringManager.InstanceIDNotFound);
                }
                return workflowInstanceId;
            }
        }
    }
}

