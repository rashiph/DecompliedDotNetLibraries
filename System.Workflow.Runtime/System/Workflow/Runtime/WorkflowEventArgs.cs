namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    public class WorkflowEventArgs : EventArgs
    {
        private System.Workflow.Runtime.WorkflowInstance _instance;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowEventArgs(System.Workflow.Runtime.WorkflowInstance instance)
        {
            this._instance = instance;
        }

        public System.Workflow.Runtime.WorkflowInstance WorkflowInstance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._instance;
            }
        }
    }
}

