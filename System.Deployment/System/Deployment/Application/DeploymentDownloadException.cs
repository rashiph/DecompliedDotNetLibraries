namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DeploymentDownloadException : DeploymentException
    {
        public DeploymentDownloadException() : this(Resources.GetString("Ex_DeploymentDownloadException"))
        {
        }

        public DeploymentDownloadException(string message) : base(message)
        {
        }

        internal DeploymentDownloadException(ExceptionTypes exceptionType, string message) : base(exceptionType, message)
        {
        }

        protected DeploymentDownloadException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public DeploymentDownloadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal DeploymentDownloadException(ExceptionTypes exceptionType, string message, Exception innerException) : base(exceptionType, message, innerException)
        {
        }
    }
}

