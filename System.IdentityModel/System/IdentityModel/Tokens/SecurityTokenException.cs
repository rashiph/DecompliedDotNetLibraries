namespace System.IdentityModel.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SecurityTokenException : SystemException
    {
        public SecurityTokenException()
        {
        }

        public SecurityTokenException(string message) : base(message)
        {
        }

        protected SecurityTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SecurityTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

