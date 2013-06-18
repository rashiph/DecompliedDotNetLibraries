namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal static class HttpTransportDefaults
    {
        internal const bool AllowCookies = false;
        internal const AuthenticationSchemes AuthenticationScheme = AuthenticationSchemes.Anonymous;
        internal const bool BypassProxyOnLocal = false;
        internal const bool DecompressionEnabled = true;
        internal const System.ServiceModel.HostNameComparisonMode HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        internal const bool KeepAliveEnabled = true;
        internal const Uri ProxyAddress = null;
        internal const AuthenticationSchemes ProxyAuthenticationScheme = AuthenticationSchemes.Anonymous;
        internal const string Realm = "";
        internal const System.ServiceModel.TransferMode TransferMode = System.ServiceModel.TransferMode.Buffered;
        internal const bool UnsafeConnectionNtlmAuthentication = false;
        internal const bool UseDefaultWebProxy = true;

        internal static MessageEncoderFactory GetDefaultMessageEncoderFactory()
        {
            return new TextMessageEncoderFactory(MessageVersion.Default, TextEncoderDefaults.Encoding, 0x40, 0x10, EncoderDefaults.ReaderQuotas);
        }

        internal static SecurityAlgorithmSuite MessageSecurityAlgorithmSuite
        {
            get
            {
                return SecurityAlgorithmSuite.Default;
            }
        }
    }
}

