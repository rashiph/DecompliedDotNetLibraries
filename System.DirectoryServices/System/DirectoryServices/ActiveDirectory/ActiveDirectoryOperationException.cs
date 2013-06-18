namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ActiveDirectoryOperationException : Exception, ISerializable
    {
        private int errorCode;

        public ActiveDirectoryOperationException() : base(Res.GetString("DSUnknownFailure"))
        {
        }

        public ActiveDirectoryOperationException(string message) : base(message)
        {
        }

        protected ActiveDirectoryOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ActiveDirectoryOperationException(string message, Exception inner) : base(message, inner)
        {
        }

        public ActiveDirectoryOperationException(string message, int errorCode) : base(message)
        {
            this.errorCode = errorCode;
        }

        public ActiveDirectoryOperationException(string message, Exception inner, int errorCode) : base(message, inner)
        {
            this.errorCode = errorCode;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }
    }
}

