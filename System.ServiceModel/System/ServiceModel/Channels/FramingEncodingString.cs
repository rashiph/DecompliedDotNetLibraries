namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal static class FramingEncodingString
    {
        public const string Binary = "application/soap+msbin1";
        public const string BinarySession = "application/soap+msbinsession1";
        public const string ConnectionDispatchFailedFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/ConnectionDispatchFailed";
        public const string ContentTypeInvalidFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/ContentTypeInvalid";
        public const string ContentTypeTooLongFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/ContentTypeTooLong";
        public const string EndpointNotFoundFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/EndpointNotFound";
        public const string EndpointUnavailableFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/EndpointUnavailable";
        private const string ExceptionKey = "FramingEncodingString";
        private const string FaultBaseUri = "http://schemas.microsoft.com/ws/2006/05/framing/faults/";
        public const string MaxMessageSizeExceededFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/MaxMessageSizeExceededFault";
        public const string MTOM = "multipart/related";
        public const string NamespaceUri = "http://schemas.microsoft.com/ws/2006/05/framing";
        public const string ServerTooBusyFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/ServerTooBusy";
        public const string ServiceActivationFailedFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/ServiceActivationFailed";
        public const string Soap11Utf16 = "text/xml; charset=utf16";
        public const string Soap11Utf16FFFE = "text/xml; charset=unicodeFFFE";
        public const string Soap11Utf8 = "text/xml; charset=utf-8";
        public const string Soap12Utf16 = "application/soap+xml; charset=utf16";
        public const string Soap12Utf16FFFE = "application/soap+xml; charset=unicodeFFFE";
        public const string Soap12Utf8 = "application/soap+xml; charset=utf-8";
        public const string UnsupportedModeFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedMode";
        public const string UnsupportedVersionFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedVersion";
        public const string UpgradeInvalidFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/UpgradeInvalid";
        public const string ViaTooLongFault = "http://schemas.microsoft.com/ws/2006/05/framing/faults/ViaTooLong";

        public static void AddFaultString(Exception exception, string framingFault)
        {
            exception.Data["FramingEncodingString"] = framingFault;
        }

        public static bool TryGetFaultString(Exception exception, out string framingFault)
        {
            framingFault = null;
            if (exception.Data.Contains("FramingEncodingString"))
            {
                framingFault = exception.Data["FramingEncodingString"] as string;
                if (framingFault != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

