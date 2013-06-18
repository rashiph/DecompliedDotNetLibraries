namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Data.SqlTypes;
    using System.Runtime;
    using System.Workflow.Runtime;

    public class SqlPersistenceWorkflowInstanceDescription
    {
        private bool isBlocked;
        private SqlDateTime nextTimerExpiration;
        private WorkflowStatus status;
        private string suspendOrTerminateDescription;
        private Guid workflowInstanceId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SqlPersistenceWorkflowInstanceDescription(Guid workflowInstanceId, WorkflowStatus status, bool isBlocked, string suspendOrTerminateDescription, SqlDateTime nextTimerExpiration)
        {
            this.workflowInstanceId = workflowInstanceId;
            this.status = status;
            this.isBlocked = isBlocked;
            this.suspendOrTerminateDescription = suspendOrTerminateDescription;
            this.nextTimerExpiration = nextTimerExpiration;
        }

        public bool IsBlocked
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isBlocked;
            }
        }

        public SqlDateTime NextTimerExpiration
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.nextTimerExpiration;
            }
        }

        public WorkflowStatus Status
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.status;
            }
        }

        public string SuspendOrTerminateDescription
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.suspendOrTerminateDescription;
            }
        }

        public Guid WorkflowInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.workflowInstanceId;
            }
        }
    }
}

