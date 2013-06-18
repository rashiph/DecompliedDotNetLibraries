namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidChannelBindingException : Exception
    {
        public InvalidChannelBindingException()
        {
        }

        public InvalidChannelBindingException(string message) : base(message)
        {
        }

        protected InvalidChannelBindingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidChannelBindingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

