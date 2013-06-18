namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowApplicationCompletedException : WorkflowApplicationException
    {
        public WorkflowApplicationCompletedException()
        {
        }

        public WorkflowApplicationCompletedException(string message) : base(message)
        {
        }

        protected WorkflowApplicationCompletedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WorkflowApplicationCompletedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WorkflowApplicationCompletedException(string message, Guid instanceId) : base(message, instanceId)
        {
        }

        public WorkflowApplicationCompletedException(string message, Guid instanceId, Exception innerException) : base(message, instanceId, innerException)
        {
        }
    }
}

