namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class StrongTypingException : DataException
    {
        public StrongTypingException()
        {
            base.HResult = -2146232021;
        }

        public StrongTypingException(string message) : base(message)
        {
            base.HResult = -2146232021;
        }

        protected StrongTypingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public StrongTypingException(string s, Exception innerException) : base(s, innerException)
        {
            base.HResult = -2146232021;
        }
    }
}

