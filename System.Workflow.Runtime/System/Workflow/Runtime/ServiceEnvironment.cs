namespace System.Workflow.Runtime
{
    using System;
    using System.Workflow.ComponentModel;

    internal sealed class ServiceEnvironment : AmbientEnvironment
    {
        internal static readonly Guid debuggerThreadGuid = new Guid("54D747AE-5CC6-4171-95C8-0A8C40443915");

        internal ServiceEnvironment(Activity currentActivity) : base(currentActivity)
        {
            GC.SuppressFinalize(this);
        }

        internal static bool IsInServiceThread(Guid instanceId)
        {
            return ((WorkflowInstanceId == instanceId) || DebuggerThreadMarker.IsInDebuggerThread());
        }

        private static Activity CurrentActivity
        {
            get
            {
                return (AmbientEnvironment.Retrieve() as Activity);
            }
        }

        internal static WorkflowQueuingService QueuingService
        {
            get
            {
                Activity currentActivity = CurrentActivity;
                IWorkflowCoreRuntime runtime = null;
                if (currentActivity != null)
                {
                    runtime = ContextActivityUtils.RetrieveWorkflowExecutor(currentActivity);
                }
                while (currentActivity != null)
                {
                    if (currentActivity == runtime.CurrentAtomicActivity)
                    {
                        TransactionalProperties properties = (TransactionalProperties) currentActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                        if ((properties != null) && (properties.LocalQueuingService != null))
                        {
                            return properties.LocalQueuingService;
                        }
                    }
                    currentActivity = currentActivity.Parent;
                }
                return null;
            }
        }

        internal static IWorkBatch WorkBatch
        {
            get
            {
                Activity currentActivity = CurrentActivity;
                if (currentActivity == null)
                {
                    return null;
                }
                return (IWorkBatch) currentActivity.GetValue(WorkflowExecutor.TransientBatchProperty);
            }
        }

        internal static Guid WorkflowInstanceId
        {
            get
            {
                Activity currentActivity = CurrentActivity;
                if (currentActivity == null)
                {
                    return Guid.Empty;
                }
                return (Guid) ContextActivityUtils.RootContextActivity(currentActivity).GetValue(WorkflowExecutor.WorkflowInstanceIdProperty);
            }
        }
    }
}

