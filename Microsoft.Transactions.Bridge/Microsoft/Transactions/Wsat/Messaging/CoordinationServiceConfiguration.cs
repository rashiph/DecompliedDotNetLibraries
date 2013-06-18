namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CoordinationServiceConfiguration
    {
        public CoordinationServiceMode Mode;
        public string HostName;
        public string BasePath;
        public int HttpsPort;
        public TimeSpan OperationTimeout;
        public X509Certificate2 X509Certificate;
        public bool SupportingTokensEnabled;
        public bool RemoteClientsEnabled;
        public List<string> GlobalAclWindowsIdentities;
        public List<string> GlobalAclX509CertificateThumbprints;
    }
}

