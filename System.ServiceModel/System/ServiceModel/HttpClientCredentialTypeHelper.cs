namespace System.ServiceModel
{
    using System;
    using System.Net;

    internal static class HttpClientCredentialTypeHelper
    {
        internal static bool IsDefined(HttpClientCredentialType value)
        {
            if ((((value != HttpClientCredentialType.None) && (value != HttpClientCredentialType.Basic)) && ((value != HttpClientCredentialType.Digest) && (value != HttpClientCredentialType.Ntlm))) && (value != HttpClientCredentialType.Windows))
            {
                return (value == HttpClientCredentialType.Certificate);
            }
            return true;
        }

        internal static AuthenticationSchemes MapToAuthenticationScheme(HttpClientCredentialType clientCredentialType)
        {
            switch (clientCredentialType)
            {
                case HttpClientCredentialType.None:
                case HttpClientCredentialType.Certificate:
                    return AuthenticationSchemes.Anonymous;

                case HttpClientCredentialType.Basic:
                    return AuthenticationSchemes.Basic;

                case HttpClientCredentialType.Digest:
                    return AuthenticationSchemes.Digest;

                case HttpClientCredentialType.Ntlm:
                    return AuthenticationSchemes.Ntlm;

                case HttpClientCredentialType.Windows:
                    return AuthenticationSchemes.Negotiate;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        internal static HttpClientCredentialType MapToClientCredentialType(AuthenticationSchemes authenticationSchemes)
        {
            switch (authenticationSchemes)
            {
                case AuthenticationSchemes.Digest:
                    return HttpClientCredentialType.Digest;

                case AuthenticationSchemes.Negotiate:
                    return HttpClientCredentialType.Windows;

                case AuthenticationSchemes.Ntlm:
                    return HttpClientCredentialType.Ntlm;

                case AuthenticationSchemes.Basic:
                    return HttpClientCredentialType.Basic;

                case AuthenticationSchemes.Anonymous:
                    return HttpClientCredentialType.None;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }
    }
}

