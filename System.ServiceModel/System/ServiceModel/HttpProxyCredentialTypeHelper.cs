namespace System.ServiceModel
{
    using System;
    using System.Net;

    internal static class HttpProxyCredentialTypeHelper
    {
        internal static bool IsDefined(HttpProxyCredentialType value)
        {
            if (((value != HttpProxyCredentialType.None) && (value != HttpProxyCredentialType.Basic)) && ((value != HttpProxyCredentialType.Digest) && (value != HttpProxyCredentialType.Ntlm)))
            {
                return (value == HttpProxyCredentialType.Windows);
            }
            return true;
        }

        internal static AuthenticationSchemes MapToAuthenticationScheme(HttpProxyCredentialType proxyCredentialType)
        {
            switch (proxyCredentialType)
            {
                case HttpProxyCredentialType.None:
                    return AuthenticationSchemes.Anonymous;

                case HttpProxyCredentialType.Basic:
                    return AuthenticationSchemes.Basic;

                case HttpProxyCredentialType.Digest:
                    return AuthenticationSchemes.Digest;

                case HttpProxyCredentialType.Ntlm:
                    return AuthenticationSchemes.Ntlm;

                case HttpProxyCredentialType.Windows:
                    return AuthenticationSchemes.Negotiate;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        internal static HttpProxyCredentialType MapToProxyCredentialType(AuthenticationSchemes authenticationSchemes)
        {
            switch (authenticationSchemes)
            {
                case AuthenticationSchemes.Digest:
                    return HttpProxyCredentialType.Digest;

                case AuthenticationSchemes.Negotiate:
                    return HttpProxyCredentialType.Windows;

                case AuthenticationSchemes.Ntlm:
                    return HttpProxyCredentialType.Ntlm;

                case AuthenticationSchemes.Basic:
                    return HttpProxyCredentialType.Basic;

                case AuthenticationSchemes.Anonymous:
                    return HttpProxyCredentialType.None;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }
    }
}

