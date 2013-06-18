namespace System.Activities.Statements
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowTerminatedException : Exception
    {
        public WorkflowTerminatedException() : base(System.Activities.SR.WorkflowTerminatedExceptionDefaultMessage)
        {
        }

        public WorkflowTerminatedException(string message) : base(message)
        {
        }

        protected WorkflowTerminatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WorkflowTerminatedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

