namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class DependentPlatformMissingException : DeploymentException
    {
        private Uri _supportUrl;

        public DependentPlatformMissingException() : this(Resources.GetString("Ex_DependentPlatformMissingException"))
        {
        }

        public DependentPlatformMissingException(string message) : base(message)
        {
        }

        internal DependentPlatformMissingException(ExceptionTypes exceptionType, string message) : base(exceptionType, message)
        {
        }

        protected DependentPlatformMissingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this._supportUrl = (Uri) serializationInfo.GetValue("_supportUrl", typeof(Uri));
        }

        public DependentPlatformMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DependentPlatformMissingException(string message, Uri supportUrl) : base(message)
        {
            this._supportUrl = supportUrl;
        }

        internal DependentPlatformMissingException(ExceptionTypes exceptionType, string message, Exception innerException) : base(exceptionType, message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_supportUrl", this._supportUrl);
        }

        public Uri SupportUrl
        {
            get
            {
                return this._supportUrl;
            }
        }
    }
}

