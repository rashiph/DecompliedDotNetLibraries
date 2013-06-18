namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class WrappedDispatcherException : SystemException
    {
        public WrappedDispatcherException()
        {
        }

        public WrappedDispatcherException(string message) : base(message)
        {
        }

        public WrappedDispatcherException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WrappedDispatcherException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

