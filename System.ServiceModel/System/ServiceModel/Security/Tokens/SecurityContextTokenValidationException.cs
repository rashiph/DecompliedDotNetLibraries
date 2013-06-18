namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Runtime.Serialization;

    [Serializable]
    internal class SecurityContextTokenValidationException : SecurityTokenValidationException
    {
        public SecurityContextTokenValidationException()
        {
        }

        public SecurityContextTokenValidationException(string message) : base(message)
        {
        }

        protected SecurityContextTokenValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SecurityContextTokenValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

