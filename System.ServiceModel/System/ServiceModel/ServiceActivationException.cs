namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ServiceActivationException : CommunicationException
    {
        public ServiceActivationException()
        {
        }

        public ServiceActivationException(string message) : base(message)
        {
        }

        protected ServiceActivationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ServiceActivationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

