namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.ServiceModel;

    [Serializable]
    public class RetryException : CommunicationException
    {
        public RetryException() : this((string) null, (Exception) null)
        {
        }

        public RetryException(string message) : this(message, null)
        {
        }

        [SecurityCritical]
        protected RetryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RetryException(string message, Exception innerException) : base(message ?? System.ServiceModel.SR.GetString("RetryGenericMessage"), innerException)
        {
        }
    }
}

