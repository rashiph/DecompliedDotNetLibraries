namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class DownloadCancelledException : DeploymentDownloadException
    {
        public DownloadCancelledException() : this(Resources.GetString("Ex_DownloadCancelledException"))
        {
        }

        public DownloadCancelledException(string message) : base(message)
        {
        }

        protected DownloadCancelledException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public DownloadCancelledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

