namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;

    [Serializable]
    public class ActionNotSupportedException : CommunicationException
    {
        public ActionNotSupportedException()
        {
        }

        public ActionNotSupportedException(string message) : base(message)
        {
        }

        protected ActionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ActionNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal Message ProvideFault(MessageVersion messageVersion)
        {
            FaultCode faultCode = FaultCode.CreateSenderFaultCode("ActionNotSupported", messageVersion.Addressing.Namespace);
            string message = this.Message;
            return Message.CreateMessage(messageVersion, faultCode, message, messageVersion.Addressing.FaultAction);
        }
    }
}

