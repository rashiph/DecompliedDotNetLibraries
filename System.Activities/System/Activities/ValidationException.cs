namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ValidationException : SystemException
    {
        public ValidationException()
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

