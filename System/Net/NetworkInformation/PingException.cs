namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PingException : InvalidOperationException
    {
        internal PingException()
        {
        }

        public PingException(string message) : base(message)
        {
        }

        protected PingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public PingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

