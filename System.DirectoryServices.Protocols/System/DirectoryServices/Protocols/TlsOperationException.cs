namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TlsOperationException : DirectoryOperationException
    {
        public TlsOperationException()
        {
        }

        public TlsOperationException(DirectoryResponse response) : base(response)
        {
        }

        public TlsOperationException(string message) : base(message)
        {
        }

        public TlsOperationException(DirectoryResponse response, string message) : base(response, message)
        {
        }

        protected TlsOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TlsOperationException(string message, Exception inner) : base(message, inner)
        {
        }

        public TlsOperationException(DirectoryResponse response, string message, Exception inner) : base(response, message, inner)
        {
        }
    }
}

