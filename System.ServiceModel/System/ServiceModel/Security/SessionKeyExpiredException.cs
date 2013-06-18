namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class SessionKeyExpiredException : MessageSecurityException
    {
        public SessionKeyExpiredException()
        {
        }

        public SessionKeyExpiredException(string message) : base(message)
        {
        }

        protected SessionKeyExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SessionKeyExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

