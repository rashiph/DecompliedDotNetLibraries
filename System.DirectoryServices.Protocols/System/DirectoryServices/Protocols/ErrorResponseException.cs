namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ErrorResponseException : DirectoryException, ISerializable
    {
        private DsmlErrorResponse errorResponse;

        public ErrorResponseException()
        {
        }

        public ErrorResponseException(DsmlErrorResponse response) : this(response, Res.GetString("ErrorResponse"), null)
        {
        }

        public ErrorResponseException(string message) : base(message)
        {
        }

        public ErrorResponseException(DsmlErrorResponse response, string message) : this(response, message, null)
        {
        }

        protected ErrorResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ErrorResponseException(string message, Exception inner) : base(message, inner)
        {
        }

        public ErrorResponseException(DsmlErrorResponse response, string message, Exception inner) : base(message, inner)
        {
            this.errorResponse = response;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public DsmlErrorResponse Response
        {
            get
            {
                return this.errorResponse;
            }
        }
    }
}

