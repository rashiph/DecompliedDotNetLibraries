namespace System.ServiceModel.Configuration
{
    using System;

    public enum AuthenticationMode
    {
        AnonymousForCertificate,
        AnonymousForSslNegotiated,
        CertificateOverTransport,
        IssuedToken,
        IssuedTokenForCertificate,
        IssuedTokenForSslNegotiated,
        IssuedTokenOverTransport,
        Kerberos,
        KerberosOverTransport,
        MutualCertificate,
        MutualCertificateDuplex,
        MutualSslNegotiated,
        SecureConversation,
        SspiNegotiated,
        UserNameForCertificate,
        UserNameForSslNegotiated,
        UserNameOverTransport,
        SspiNegotiatedOverTransport
    }
}

