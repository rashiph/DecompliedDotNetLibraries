namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;

    public delegate X509Certificate QueryClientCertificateCallback(LdapConnection connection, byte[][] trustedCAs);
}

