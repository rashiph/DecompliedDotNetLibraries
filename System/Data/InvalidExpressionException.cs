namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidExpressionException : DataException
    {
        public InvalidExpressionException()
        {
        }

        public InvalidExpressionException(string s) : base(s)
        {
        }

        protected InvalidExpressionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidExpressionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

