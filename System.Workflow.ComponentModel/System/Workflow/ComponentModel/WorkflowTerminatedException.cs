namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class WorkflowTerminatedException : Exception
    {
        public WorkflowTerminatedException() : base(SR.GetString("Error_WorkflowTerminated"))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowTerminatedException(string message) : base(message)
        {
        }

        private WorkflowTerminatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowTerminatedException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}

