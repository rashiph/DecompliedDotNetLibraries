namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [Serializable]
    public class SecurityNegotiationException : CommunicationException
    {
        public SecurityNegotiationException()
        {
        }

        public SecurityNegotiationException(string message) : base(message)
        {
        }

        protected SecurityNegotiationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SecurityNegotiationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

