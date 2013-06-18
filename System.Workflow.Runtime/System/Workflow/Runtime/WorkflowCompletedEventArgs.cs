namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;
    using System.Workflow.ComponentModel;

    public class WorkflowCompletedEventArgs : WorkflowEventArgs
    {
        private Activity _originalWorkflowDefinition;
        private Dictionary<string, object> _outputParameters;
        private Activity _workflowDefinition;

        internal WorkflowCompletedEventArgs(WorkflowInstance instance, Activity workflowDefinition) : base(instance)
        {
            this._outputParameters = new Dictionary<string, object>();
            this._originalWorkflowDefinition = workflowDefinition;
            this._workflowDefinition = null;
        }

        public Dictionary<string, object> OutputParameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._outputParameters;
            }
        }

        public Activity WorkflowDefinition
        {
            get
            {
                if (this._workflowDefinition == null)
                {
                    using (new WorkflowDefinitionLock(this._originalWorkflowDefinition))
                    {
                        if (this._workflowDefinition == null)
                        {
                            Activity activity = this._originalWorkflowDefinition.Clone();
                            Thread.MemoryBarrier();
                            this._workflowDefinition = activity;
                        }
                    }
                }
                return this._workflowDefinition;
            }
        }
    }
}

