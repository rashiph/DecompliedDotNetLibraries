namespace System.Workflow.Runtime
{
    using System;
    using System.Workflow.ComponentModel;

    internal static class ContextActivityUtils
    {
        internal static Activity ContextActivity(Activity activity)
        {
            Activity parent = activity;
            while ((parent != null) && (parent.GetValue(Activity.ActivityExecutionContextInfoProperty) == null))
            {
                parent = parent.Parent;
            }
            return parent;
        }

        internal static int ContextId(Activity activity)
        {
            return ((ActivityExecutionContextInfo) ContextActivity(activity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextId;
        }

        internal static Activity ParentContextActivity(Activity activity)
        {
            ActivityExecutionContextInfo info = (ActivityExecutionContextInfo) ContextActivity(activity).GetValue(Activity.ActivityExecutionContextInfoProperty);
            if (info.ParentContextId == -1)
            {
                return null;
            }
            return RetrieveWorkflowExecutor(activity).GetContextActivityForId(info.ParentContextId);
        }

        internal static IWorkflowCoreRuntime RetrieveWorkflowExecutor(Activity activity)
        {
            IWorkflowCoreRuntime runtime = null;
            Activity parent = activity;
            while ((parent != null) && (parent.Parent != null))
            {
                parent = parent.Parent;
            }
            if (parent != null)
            {
                runtime = (IWorkflowCoreRuntime) parent.GetValue(WorkflowExecutor.WorkflowExecutorProperty);
            }
            return runtime;
        }

        internal static Activity RootContextActivity(Activity activity)
        {
            return RetrieveWorkflowExecutor(activity).RootActivity;
        }
    }
}

