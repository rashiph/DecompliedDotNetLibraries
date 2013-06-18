namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;

    public delegate bool VerifyServerCertificateCallback(LdapConnection connection, X509Certificate certificate);
}

