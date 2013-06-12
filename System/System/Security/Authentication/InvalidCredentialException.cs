namespace System.Security.Authentication
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidCredentialException : AuthenticationException
    {
        public InvalidCredentialException()
        {
        }

        public InvalidCredentialException(string message) : base(message)
        {
        }

        protected InvalidCredentialException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public InvalidCredentialException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

