namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class AddressAlreadyInUseException : CommunicationException
    {
        public AddressAlreadyInUseException()
        {
        }

        public AddressAlreadyInUseException(string message) : base(message)
        {
        }

        protected AddressAlreadyInUseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AddressAlreadyInUseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

