namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PoisonMessageException : CommunicationException
    {
        public PoisonMessageException()
        {
        }

        public PoisonMessageException(string message) : base(message)
        {
        }

        protected PoisonMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PoisonMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

