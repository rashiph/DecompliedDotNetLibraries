namespace System.ServiceModel.Configuration
{
    using System;

    internal static class AuthenticationModeHelper
    {
        public static bool IsDefined(AuthenticationMode value)
        {
            if ((((((value != AuthenticationMode.AnonymousForCertificate) && (value != AuthenticationMode.AnonymousForSslNegotiated)) && ((value != AuthenticationMode.CertificateOverTransport) && (value != AuthenticationMode.IssuedToken))) && (((value != AuthenticationMode.IssuedTokenForCertificate) && (value != AuthenticationMode.IssuedTokenForSslNegotiated)) && ((value != AuthenticationMode.IssuedTokenOverTransport) && (value != AuthenticationMode.Kerberos)))) && ((((value != AuthenticationMode.KerberosOverTransport) && (value != AuthenticationMode.MutualCertificate)) && ((value != AuthenticationMode.MutualCertificateDuplex) && (value != AuthenticationMode.MutualSslNegotiated))) && (((value != AuthenticationMode.SecureConversation) && (value != AuthenticationMode.SspiNegotiated)) && ((value != AuthenticationMode.UserNameForCertificate) && (value != AuthenticationMode.UserNameForSslNegotiated))))) && (value != AuthenticationMode.UserNameOverTransport))
            {
                return (value == AuthenticationMode.SspiNegotiatedOverTransport);
            }
            return true;
        }
    }
}

