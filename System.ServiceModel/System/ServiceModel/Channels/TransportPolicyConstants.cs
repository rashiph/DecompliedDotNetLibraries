namespace System.ServiceModel.Channels
{
    using System;

    internal static class TransportPolicyConstants
    {
        public const string BasicHttpAuthenticationName = "BasicAuthentication";
        public const string CompositeDuplex = "CompositeDuplex";
        public const string CompositeDuplexNamespace = "http://schemas.microsoft.com/net/2006/06/duplex";
        public const string CompositeDuplexPrefix = "cdp";
        public const string DigestHttpAuthenticationName = "DigestAuthentication";
        public const string DotNetFramingNamespace = "http://schemas.microsoft.com/ws/2006/05/framing/policy";
        public const string DotNetFramingPrefix = "msf";
        public const string HttpAuthNamespace = "http://schemas.microsoft.com/ws/06/2004/policy/http";
        public const string HttpAuthPrefix = "http";
        public const string HttpTransportUri = "http://schemas.xmlsoap.org/soap/http";
        public const string MsmqAuthenticated = "Authenticated";
        public const string MsmqBestEffort = "MsmqBestEffort";
        public const string MsmqSession = "MsmqSession";
        public const string MsmqTransportNamespace = "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq";
        public const string MsmqTransportPrefix = "msmq";
        public const string MsmqTransportUri = "http://schemas.microsoft.com/soap/msmq";
        public const string MsmqVolatile = "MsmqVolatile";
        public const string MsmqWindowsDomain = "WindowsDomain";
        public const string NamedPipeTransportUri = "http://schemas.microsoft.com/soap/named-pipe";
        public const string NegotiateHttpAuthenticationName = "NegotiateAuthentication";
        public const string NtlmHttpAuthenticationName = "NtlmAuthentication";
        public const string PeerTransportUri = "http://schemas.microsoft.com/soap/peer";
        public const string ProtectionLevelName = "ProtectionLevel";
        public const string RequireClientCertificateName = "RequireClientCertificate";
        public const string SslTransportSecurityName = "SslTransportSecurity";
        public const string StreamedName = "Streamed";
        public const string TcpTransportUri = "http://schemas.microsoft.com/soap/tcp";
        public const string WindowsTransportSecurityName = "WindowsTransportSecurity";
    }
}

