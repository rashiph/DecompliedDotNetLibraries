namespace System.Workflow.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Runtime;

    [Serializable]
    public sealed class ActivityExecutionStatusChangedEventArgs : EventArgs
    {
        private ActivityExecutionResult activityExecutionResult;
        private string activityQualifiedName;
        private int stateId = -1;
        private ActivityExecutionStatus status;
        [NonSerialized]
        private IWorkflowCoreRuntime workflowCoreRuntime;

        internal ActivityExecutionStatusChangedEventArgs(ActivityExecutionStatus executionStatus, ActivityExecutionResult executionResult, System.Workflow.ComponentModel.Activity activity)
        {
            this.status = executionStatus;
            this.activityExecutionResult = executionResult;
            this.activityQualifiedName = activity.QualifiedName;
            this.stateId = activity.ContextActivity.ContextId;
        }

        public override string ToString()
        {
            return ("ActivityStatusChange('(" + this.stateId.ToString(CultureInfo.CurrentCulture) + ")" + this.activityQualifiedName + "', " + System.Workflow.ComponentModel.Activity.ActivityExecutionStatusEnumToString(this.ExecutionStatus) + ", " + System.Workflow.ComponentModel.Activity.ActivityExecutionResultEnumToString(this.ExecutionResult) + ")");
        }

        public System.Workflow.ComponentModel.Activity Activity
        {
            get
            {
                System.Workflow.ComponentModel.Activity activityByName = null;
                if (this.workflowCoreRuntime != null)
                {
                    System.Workflow.ComponentModel.Activity contextActivityForId = this.workflowCoreRuntime.GetContextActivityForId(this.stateId);
                    if (contextActivityForId != null)
                    {
                        activityByName = contextActivityForId.GetActivityByName(this.activityQualifiedName);
                    }
                }
                return activityByName;
            }
        }

        internal IWorkflowCoreRuntime BaseExecutor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.workflowCoreRuntime = value;
            }
        }

        public ActivityExecutionResult ExecutionResult
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityExecutionResult;
            }
        }

        public ActivityExecutionStatus ExecutionStatus
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.status;
            }
        }
    }
}

