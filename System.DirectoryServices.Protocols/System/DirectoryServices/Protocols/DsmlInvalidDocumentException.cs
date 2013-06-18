namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DsmlInvalidDocumentException : DirectoryException
    {
        public DsmlInvalidDocumentException() : base(Res.GetString("InvalidDocument"))
        {
        }

        public DsmlInvalidDocumentException(string message) : base(message)
        {
        }

        protected DsmlInvalidDocumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DsmlInvalidDocumentException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

