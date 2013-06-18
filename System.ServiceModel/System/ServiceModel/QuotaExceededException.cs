namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class QuotaExceededException : SystemException
    {
        public QuotaExceededException()
        {
        }

        public QuotaExceededException(string message) : base(message)
        {
        }

        protected QuotaExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public QuotaExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

