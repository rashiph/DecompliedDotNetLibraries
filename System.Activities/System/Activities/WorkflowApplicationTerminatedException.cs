namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowApplicationTerminatedException : WorkflowApplicationCompletedException
    {
        public WorkflowApplicationTerminatedException()
        {
        }

        public WorkflowApplicationTerminatedException(string message) : base(message)
        {
        }

        protected WorkflowApplicationTerminatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WorkflowApplicationTerminatedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WorkflowApplicationTerminatedException(string message, Guid instanceId) : base(message, instanceId)
        {
        }

        public WorkflowApplicationTerminatedException(string message, Guid instanceId, Exception innerException) : base(message, instanceId, innerException)
        {
        }
    }
}

