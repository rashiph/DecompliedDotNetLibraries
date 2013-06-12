namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class EvaluateException : InvalidExpressionException
    {
        public EvaluateException()
        {
        }

        public EvaluateException(string s) : base(s)
        {
        }

        protected EvaluateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public EvaluateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

