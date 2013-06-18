namespace System.ServiceModel.Channels
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class PeerCertificateClientCredentials : SecurityCredentialsManager
    {
        private X509CertificateValidator certificateValidator;
        private X509Certificate2 selfCertificate;

        public PeerCertificateClientCredentials(X509Certificate2 selfCertificate, X509CertificateValidator validator)
        {
            this.selfCertificate = selfCertificate;
            this.certificateValidator = validator;
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new PeerCertificateClientCredentialsSecurityTokenManager(this);
        }

        private class PeerCertificateClientCredentialsSecurityTokenManager : SecurityTokenManager
        {
            private PeerCertificateClientCredentials creds;

            public PeerCertificateClientCredentialsSecurityTokenManager(PeerCertificateClientCredentials creds)
            {
                this.creds = creds;
            }

            public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement requirement)
            {
                if (requirement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requirement");
                }
                if ((requirement.TokenType != SecurityTokenTypes.X509Certificate) || (requirement.KeyUsage != SecurityKeyUsage.Signature))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                return new PeerX509TokenProvider(this.creds.certificateValidator, this.creds.selfCertificate);
            }

            public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
            {
                MessageSecurityTokenVersion version2 = (MessageSecurityTokenVersion) version;
                return new WSSecurityTokenSerializer(version2.SecurityVersion, version2.TrustVersion, version2.SecureConversationVersion, version2.EmitBspRequiredAttributes, null, null, null);
            }
        }
    }
}

