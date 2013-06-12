namespace System
{
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class InsufficientMemoryException : OutOfMemoryException
    {
        public InsufficientMemoryException() : base(Exception.GetMessageFromNativeResources(Exception.ExceptionMessageKind.OutOfMemory))
        {
            base.SetErrorCode(-2146233027);
        }

        public InsufficientMemoryException(string message) : base(message)
        {
            base.SetErrorCode(-2146233027);
        }

        private InsufficientMemoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InsufficientMemoryException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233027);
        }
    }
}

