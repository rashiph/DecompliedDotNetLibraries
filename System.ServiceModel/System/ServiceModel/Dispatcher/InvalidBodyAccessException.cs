namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public abstract class InvalidBodyAccessException : SystemException
    {
        protected InvalidBodyAccessException(string message) : this(message, null)
        {
        }

        protected InvalidBodyAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected InvalidBodyAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

