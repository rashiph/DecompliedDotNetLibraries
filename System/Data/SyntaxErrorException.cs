namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SyntaxErrorException : InvalidExpressionException
    {
        public SyntaxErrorException()
        {
        }

        public SyntaxErrorException(string s) : base(s)
        {
        }

        protected SyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

