namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidDeploymentException : DeploymentException
    {
        public InvalidDeploymentException() : this(Resources.GetString("Ex_InvalidDeploymentException"))
        {
        }

        public InvalidDeploymentException(string message) : base(message)
        {
        }

        internal InvalidDeploymentException(ExceptionTypes exceptionType, string message) : base(exceptionType, message)
        {
        }

        protected InvalidDeploymentException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public InvalidDeploymentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal InvalidDeploymentException(ExceptionTypes exceptionType, string message, Exception innerException) : base(exceptionType, message, innerException)
        {
        }
    }
}

