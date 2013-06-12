namespace System.Net.Security
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;

    internal delegate bool RemoteCertValidationCallback(string host, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);
}

