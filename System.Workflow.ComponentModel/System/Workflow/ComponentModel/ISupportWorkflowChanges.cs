namespace System.Workflow.ComponentModel
{
    using System;

    internal interface ISupportWorkflowChanges
    {
        void OnActivityAdded(ActivityExecutionContext rootContext, Activity addedActivity);
        void OnActivityRemoved(ActivityExecutionContext rootContext, Activity removedActivity);
        void OnWorkflowChangesCompleted(ActivityExecutionContext rootContext);
    }
}

