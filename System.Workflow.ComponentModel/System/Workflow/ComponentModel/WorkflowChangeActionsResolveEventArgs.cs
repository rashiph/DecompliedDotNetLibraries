namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    internal sealed class WorkflowChangeActionsResolveEventArgs : EventArgs
    {
        private string workflowChangesMarkup;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowChangeActionsResolveEventArgs(string workflowChangesMarkup)
        {
            this.workflowChangesMarkup = workflowChangesMarkup;
        }

        public string WorkflowChangesMarkup
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.workflowChangesMarkup;
            }
        }
    }
}

