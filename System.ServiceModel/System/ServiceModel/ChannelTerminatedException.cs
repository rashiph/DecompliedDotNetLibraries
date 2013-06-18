namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ChannelTerminatedException : CommunicationException
    {
        public ChannelTerminatedException()
        {
        }

        public ChannelTerminatedException(string message) : base(message)
        {
        }

        protected ChannelTerminatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ChannelTerminatedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

