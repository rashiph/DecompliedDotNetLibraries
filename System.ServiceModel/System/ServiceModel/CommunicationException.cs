namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CommunicationException : SystemException
    {
        public CommunicationException()
        {
        }

        public CommunicationException(string message) : base(message)
        {
        }

        protected CommunicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

