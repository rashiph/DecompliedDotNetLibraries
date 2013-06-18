namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class SupportedRuntimeMissingException : DependentPlatformMissingException
    {
        private string _supportedRuntimeVersion;

        public SupportedRuntimeMissingException() : this(Resources.GetString("Ex_SupportedRuntimeMissingException"))
        {
        }

        public SupportedRuntimeMissingException(string message) : base(message)
        {
        }

        protected SupportedRuntimeMissingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this._supportedRuntimeVersion = (string) serializationInfo.GetValue("_supportedRuntimeVersion", typeof(string));
        }

        public SupportedRuntimeMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal SupportedRuntimeMissingException(string message, Uri supportUrl, string supportedRuntimeVersion) : base(message, supportUrl)
        {
            this._supportedRuntimeVersion = supportedRuntimeVersion;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_supportedRuntimeVersion", this._supportedRuntimeVersion);
        }

        public string SupportedRuntimeVersion
        {
            get
            {
                return this._supportedRuntimeVersion;
            }
        }
    }
}

