namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class WorkflowAuthorizationException : SystemException
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowAuthorizationException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowAuthorizationException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected WorkflowAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowAuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WorkflowAuthorizationException(string activityName, string principalName) : base(SR.GetString("WorkflowAuthorizationException", new object[] { activityName, principalName }))
        {
        }
    }
}

