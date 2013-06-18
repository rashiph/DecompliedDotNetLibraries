namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class AddressAccessDeniedException : CommunicationException
    {
        public AddressAccessDeniedException()
        {
        }

        public AddressAccessDeniedException(string message) : base(message)
        {
        }

        protected AddressAccessDeniedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AddressAccessDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

