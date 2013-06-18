namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CommunicationObjectAbortedException : CommunicationException
    {
        public CommunicationObjectAbortedException()
        {
        }

        public CommunicationObjectAbortedException(string message) : base(message)
        {
        }

        protected CommunicationObjectAbortedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CommunicationObjectAbortedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

