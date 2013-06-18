namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidWorkflowException : Exception
    {
        public InvalidWorkflowException() : base(System.Activities.SR.DefaultInvalidWorkflowExceptionMessage)
        {
        }

        public InvalidWorkflowException(string message) : base(message)
        {
        }

        protected InvalidWorkflowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidWorkflowException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

