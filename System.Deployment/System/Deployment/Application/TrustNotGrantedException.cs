namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TrustNotGrantedException : DeploymentException
    {
        public TrustNotGrantedException() : this(Resources.GetString("Ex_TrustNotGrantedException"))
        {
        }

        public TrustNotGrantedException(string message) : base(message)
        {
        }

        internal TrustNotGrantedException(ExceptionTypes exceptionType, string message) : base(exceptionType, message)
        {
        }

        protected TrustNotGrantedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public TrustNotGrantedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal TrustNotGrantedException(ExceptionTypes exceptionType, string message, Exception innerException) : base(exceptionType, message, innerException)
        {
        }
    }
}

