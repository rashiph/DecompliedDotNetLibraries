namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CommunicationObjectFaultedException : CommunicationException
    {
        public CommunicationObjectFaultedException()
        {
        }

        public CommunicationObjectFaultedException(string message) : base(message)
        {
        }

        protected CommunicationObjectFaultedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CommunicationObjectFaultedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

