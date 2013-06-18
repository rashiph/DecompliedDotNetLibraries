namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class DirectoryOperationException : DirectoryException, ISerializable
    {
        internal DirectoryResponse response;

        public DirectoryOperationException()
        {
        }

        public DirectoryOperationException(DirectoryResponse response) : base(Res.GetString("DefaultOperationsError"))
        {
            this.response = response;
        }

        public DirectoryOperationException(string message) : base(message)
        {
        }

        public DirectoryOperationException(DirectoryResponse response, string message) : base(message)
        {
            this.response = response;
        }

        protected DirectoryOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DirectoryOperationException(string message, Exception inner) : base(message, inner)
        {
        }

        public DirectoryOperationException(DirectoryResponse response, string message, Exception inner) : base(message, inner)
        {
            this.response = response;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public DirectoryResponse Response
        {
            get
            {
                return this.response;
            }
        }
    }
}

