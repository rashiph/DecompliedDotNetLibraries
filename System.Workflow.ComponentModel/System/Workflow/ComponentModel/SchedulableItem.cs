namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    [Serializable]
    internal abstract class SchedulableItem
    {
        private string activityId;
        private int contextId = -1;

        protected SchedulableItem(int contextId, string activityId)
        {
            this.contextId = contextId;
            this.activityId = activityId;
        }

        public abstract bool Run(IWorkflowCoreRuntime workflowCoreRuntime);

        public string ActivityId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityId;
            }
        }

        public int ContextId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.contextId;
            }
        }
    }
}

