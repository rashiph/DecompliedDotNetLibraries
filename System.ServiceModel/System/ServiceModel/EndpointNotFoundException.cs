namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class EndpointNotFoundException : CommunicationException
    {
        public EndpointNotFoundException()
        {
        }

        public EndpointNotFoundException(string message) : base(message)
        {
        }

        protected EndpointNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public EndpointNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

