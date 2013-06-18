namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ExpiredSecurityTokenException : MessageSecurityException
    {
        public ExpiredSecurityTokenException()
        {
        }

        public ExpiredSecurityTokenException(string message) : base(message)
        {
        }

        protected ExpiredSecurityTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExpiredSecurityTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

