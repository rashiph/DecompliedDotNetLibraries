namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [Serializable]
    public class SecurityAccessDeniedException : CommunicationException
    {
        public SecurityAccessDeniedException()
        {
        }

        public SecurityAccessDeniedException(string message) : base(message)
        {
        }

        protected SecurityAccessDeniedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SecurityAccessDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

