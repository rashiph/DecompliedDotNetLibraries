namespace System.ServiceModel.Channels
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;

    internal class PeerX509TokenProvider : X509SecurityTokenProvider
    {
        private X509CertificateValidator validator;

        public PeerX509TokenProvider(X509CertificateValidator validator, X509Certificate2 credential) : base(credential)
        {
            this.validator = validator;
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            X509SecurityToken tokenCore = (X509SecurityToken) base.GetTokenCore(timeout);
            if (this.validator != null)
            {
                this.validator.Validate(tokenCore.Certificate);
            }
            return tokenCore;
        }
    }
}

