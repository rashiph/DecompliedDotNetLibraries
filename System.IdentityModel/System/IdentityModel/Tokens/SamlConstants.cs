namespace System.IdentityModel.Tokens
{
    using System;

    public static class SamlConstants
    {
        internal static string[] AcceptedDateTimeFormats = new string[] { "yyyy-MM-ddTHH:mm:ss.fffffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.fffffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffffzzz", "yyyy-MM-ddTHH:mm:ss.fffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffzzz", "yyyy-MM-ddTHH:mm:ss.fffzzz", "yyyy-MM-ddTHH:mm:ss.ffzzz", "yyyy-MM-ddTHH:mm:ss.fzzz", "yyyy-MM-ddTHH:mm:sszzz" };
        internal const string AssertionIdPrefix = "SamlSecurityToken-";
        internal const string GeneratedDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public static string EmailName
        {
            get
            {
                return "EmailName";
            }
        }

        public static string EmailNamespace
        {
            get
            {
                return "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
            }
        }

        public static string HolderOfKey
        {
            get
            {
                return "urn:oasis:names:tc:SAML:1.0:cm:holder-of-key";
            }
        }

        public static int MajorVersionValue
        {
            get
            {
                return 1;
            }
        }

        public static int MinorVersionValue
        {
            get
            {
                return 1;
            }
        }

        public static string Namespace
        {
            get
            {
                return "urn:oasis:names:tc:SAML:1.0:assertion";
            }
        }

        public static string SenderVouches
        {
            get
            {
                return "urn:oasis:names:tc:SAML:1.0:cm:sender-vouches";
            }
        }

        public static string UserName
        {
            get
            {
                return "UserName";
            }
        }

        public static string UserNameNamespace
        {
            get
            {
                return "urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName";
            }
        }
    }
}

