namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class TrackingWorkflowChangedEventArgs : EventArgs
    {
        private IList<WorkflowChangeAction> _changes;
        private Activity _def;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal TrackingWorkflowChangedEventArgs(IList<WorkflowChangeAction> changes, Activity definition)
        {
            this._def = definition;
            this._changes = changes;
        }

        public IList<WorkflowChangeAction> Changes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._changes;
            }
        }

        public Activity Definition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._def;
            }
        }
    }
}

