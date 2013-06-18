namespace System.IdentityModel.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SecurityTokenValidationException : SecurityTokenException
    {
        public SecurityTokenValidationException()
        {
        }

        public SecurityTokenValidationException(string message) : base(message)
        {
        }

        protected SecurityTokenValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SecurityTokenValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

