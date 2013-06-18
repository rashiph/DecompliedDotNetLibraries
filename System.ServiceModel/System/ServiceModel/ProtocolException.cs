namespace System.ServiceModel
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;

    [Serializable]
    public class ProtocolException : CommunicationException
    {
        public ProtocolException()
        {
        }

        public ProtocolException(string message) : base(message)
        {
        }

        protected ProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ProtocolException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal static ProtocolException OneWayOperationReturnedNonNull(Message message)
        {
            if (message.IsFault)
            {
                try
                {
                    FaultReasonText matchingTranslation = MessageFault.CreateFault(message, 0x10000).Reason.GetMatchingTranslation(CultureInfo.CurrentCulture);
                    return new ProtocolException(System.ServiceModel.SR.GetString("OneWayOperationReturnedFault", new object[] { matchingTranslation.Text }));
                }
                catch (QuotaExceededException)
                {
                    return new ProtocolException(System.ServiceModel.SR.GetString("OneWayOperationReturnedLargeFault", new object[] { message.Headers.Action }));
                }
            }
            return new ProtocolException(System.ServiceModel.SR.GetString("OneWayOperationReturnedMessage", new object[] { message.Headers.Action }));
        }

        internal static ProtocolException ReceiveShutdownReturnedNonNull(Message message)
        {
            if (message.IsFault)
            {
                try
                {
                    FaultReasonText matchingTranslation = MessageFault.CreateFault(message, 0x10000).Reason.GetMatchingTranslation(CultureInfo.CurrentCulture);
                    return new ProtocolException(System.ServiceModel.SR.GetString("ReceiveShutdownReturnedFault", new object[] { matchingTranslation.Text }));
                }
                catch (QuotaExceededException)
                {
                    return new ProtocolException(System.ServiceModel.SR.GetString("ReceiveShutdownReturnedLargeFault", new object[] { message.Headers.Action }));
                }
            }
            return new ProtocolException(System.ServiceModel.SR.GetString("ReceiveShutdownReturnedMessage", new object[] { message.Headers.Action }));
        }
    }
}

