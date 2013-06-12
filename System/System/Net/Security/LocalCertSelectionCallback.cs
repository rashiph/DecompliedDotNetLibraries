namespace System.Net.Security
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;

    internal delegate X509Certificate LocalCertSelectionCallback(string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers);
}

