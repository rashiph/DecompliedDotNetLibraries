namespace System.Deployment.Application
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class DeploymentException : SystemException
    {
        private ExceptionTypes _type;

        public DeploymentException() : this(Resources.GetString("Ex_DeploymentException"))
        {
        }

        public DeploymentException(string message) : base(message)
        {
            this._type = ExceptionTypes.Unknown;
        }

        internal DeploymentException(ExceptionTypes exceptionType, string message) : base(message)
        {
            this._type = exceptionType;
        }

        protected DeploymentException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this._type = (ExceptionTypes) serializationInfo.GetValue("_type", typeof(ExceptionTypes));
        }

        public DeploymentException(string message, Exception innerException) : base(message, innerException)
        {
            this._type = ExceptionTypes.Unknown;
        }

        internal DeploymentException(ExceptionTypes exceptionType, string message, Exception innerException) : base(message, innerException)
        {
            this._type = exceptionType;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_type", this._type);
        }

        internal ExceptionTypes SubType
        {
            get
            {
                return this._type;
            }
        }
    }
}

