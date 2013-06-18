namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class CompatibleFrameworkMissingException : DependentPlatformMissingException
    {
        private System.Deployment.Application.CompatibleFrameworks _compatibleFrameworks;

        public CompatibleFrameworkMissingException() : this(Resources.GetString("Ex_CompatibleFrameworkMissingException"))
        {
        }

        public CompatibleFrameworkMissingException(string message) : base(message)
        {
        }

        protected CompatibleFrameworkMissingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this._compatibleFrameworks = (System.Deployment.Application.CompatibleFrameworks) serializationInfo.GetValue("_compatibleFrameworks", typeof(System.Deployment.Application.CompatibleFrameworks));
        }

        public CompatibleFrameworkMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal CompatibleFrameworkMissingException(string message, Uri supportUrl, System.Deployment.Application.CompatibleFrameworks compatibleFrameworks) : base(message, supportUrl)
        {
            this._compatibleFrameworks = compatibleFrameworks;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_compatibleFrameworks", this._compatibleFrameworks);
        }

        public System.Deployment.Application.CompatibleFrameworks CompatibleFrameworks
        {
            get
            {
                return this._compatibleFrameworks;
            }
        }
    }
}

