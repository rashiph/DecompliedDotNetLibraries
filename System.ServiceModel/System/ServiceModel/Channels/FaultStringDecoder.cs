namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class FaultStringDecoder : StringDecoder
    {
        internal const int FaultSizeQuota = 0x100;

        public FaultStringDecoder() : base(0x100)
        {
        }

        public static Exception GetFaultException(string faultString, string via, string contentType)
        {
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/EndpointNotFound")
            {
                return new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/ContentTypeInvalid")
            {
                return new ProtocolException(System.ServiceModel.SR.GetString("FramingContentTypeMismatch", new object[] { contentType, via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/ServiceActivationFailed")
            {
                return new ServiceActivationException(System.ServiceModel.SR.GetString("Hosting_ServiceActivationFailed", new object[] { via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/ConnectionDispatchFailed")
            {
                return new CommunicationException(System.ServiceModel.SR.GetString("Sharing_ConnectionDispatchFailed", new object[] { via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/EndpointUnavailable")
            {
                return new EndpointNotFoundException(System.ServiceModel.SR.GetString("Sharing_EndpointUnavailable", new object[] { via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/MaxMessageSizeExceededFault")
            {
                Exception innerException = new QuotaExceededException(System.ServiceModel.SR.GetString("FramingMaxMessageSizeExceeded"));
                return new CommunicationException(innerException.Message, innerException);
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedMode")
            {
                return new ProtocolException(System.ServiceModel.SR.GetString("FramingModeNotSupportedFault", new object[] { via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedVersion")
            {
                return new ProtocolException(System.ServiceModel.SR.GetString("FramingVersionNotSupportedFault", new object[] { via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/ContentTypeTooLong")
            {
                Exception exception2 = new QuotaExceededException(System.ServiceModel.SR.GetString("FramingContentTypeTooLongFault", new object[] { contentType }));
                return new CommunicationException(exception2.Message, exception2);
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/ViaTooLong")
            {
                Exception exception3 = new QuotaExceededException(System.ServiceModel.SR.GetString("FramingViaTooLongFault", new object[] { via }));
                return new CommunicationException(exception3.Message, exception3);
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/ServerTooBusy")
            {
                return new ServerTooBusyException(System.ServiceModel.SR.GetString("ServerTooBusy", new object[] { via }));
            }
            if (faultString == "http://schemas.microsoft.com/ws/2006/05/framing/faults/UpgradeInvalid")
            {
                return new ProtocolException(System.ServiceModel.SR.GetString("FramingUpgradeInvalid", new object[] { via }));
            }
            return new ProtocolException(System.ServiceModel.SR.GetString("FramingFaultUnrecognized", new object[] { faultString }));
        }

        protected override Exception OnSizeQuotaExceeded(int size)
        {
            return new InvalidDataException(System.ServiceModel.SR.GetString("FramingFaultTooLong", new object[] { size }));
        }
    }
}

