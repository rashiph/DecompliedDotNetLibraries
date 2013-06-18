namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.ServiceModel;

    internal static class AuthenticationSchemesHelper
    {
        public static bool DoesAuthTypeMatch(AuthenticationSchemes authScheme, string authType)
        {
            if ((authType == null) || (authType.Length == 0))
            {
                return (authScheme == AuthenticationSchemes.Anonymous);
            }
            if (authScheme == AuthenticationSchemes.Negotiate)
            {
                return ((authType.Equals("ntlm", StringComparison.OrdinalIgnoreCase) || authType.Equals("kerberos", StringComparison.OrdinalIgnoreCase)) || authType.Equals("negotiate", StringComparison.OrdinalIgnoreCase));
            }
            return authScheme.ToString().Equals(authType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSingleton(AuthenticationSchemes v)
        {
            switch (v)
            {
                case AuthenticationSchemes.Digest:
                case AuthenticationSchemes.Negotiate:
                case AuthenticationSchemes.Ntlm:
                case AuthenticationSchemes.Basic:
                case AuthenticationSchemes.Anonymous:
                    return true;
            }
            return false;
        }

        public static bool IsWindowsAuth(AuthenticationSchemes authScheme)
        {
            if (authScheme != AuthenticationSchemes.Negotiate)
            {
                return (authScheme == AuthenticationSchemes.Ntlm);
            }
            return true;
        }

        internal static string ToString(AuthenticationSchemes authScheme)
        {
            switch (authScheme)
            {
                case AuthenticationSchemes.Digest:
                    return "digest";

                case AuthenticationSchemes.Negotiate:
                    return "negotiate";

                case AuthenticationSchemes.Ntlm:
                    return "ntlm";

                case AuthenticationSchemes.Basic:
                    return "basic";

                case AuthenticationSchemes.Anonymous:
                    return "anonymous";
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("authScheme", (int) authScheme, typeof(AuthenticationSchemes)));
        }
    }
}

