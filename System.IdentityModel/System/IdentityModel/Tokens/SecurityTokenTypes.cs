namespace System.IdentityModel.Tokens
{
    using System;

    public static class SecurityTokenTypes
    {
        private const string kerberos = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Kerberos";
        private const string Namespace = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens";
        private const string rsa = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Rsa";
        private const string saml = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Saml";
        private const string userName = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/UserName";
        private const string x509Certificate = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/X509Certificate";

        public static string Kerberos
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Kerberos";
            }
        }

        public static string Rsa
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Rsa";
            }
        }

        public static string Saml
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Saml";
            }
        }

        public static string UserName
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/UserName";
            }
        }

        public static string X509Certificate
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/X509Certificate";
            }
        }
    }
}

