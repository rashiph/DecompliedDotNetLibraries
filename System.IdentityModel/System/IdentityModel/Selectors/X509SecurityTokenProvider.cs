namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;

    public class X509SecurityTokenProvider : SecurityTokenProvider, IDisposable
    {
        private X509Certificate2 certificate;

        public X509SecurityTokenProvider(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            this.certificate = new X509Certificate2(certificate);
        }

        public X509SecurityTokenProvider(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            X509CertificateStore store = new X509CertificateStore(storeName, storeLocation);
            X509Certificate2Collection certificates = null;
            try
            {
                store.Open(OpenFlags.ReadOnly);
                certificates = store.Find(findType, findValue, false);
                if (certificates.Count < 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("CannotFindCert", new object[] { storeName, storeLocation, findType, findValue })));
                }
                if (certificates.Count > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("FoundMultipleCerts", new object[] { storeName, storeLocation, findType, findValue })));
                }
                this.certificate = new X509Certificate2(certificates[0]);
            }
            finally
            {
                System.IdentityModel.SecurityUtils.ResetAllCertificates(certificates);
                store.Close();
            }
        }

        public void Dispose()
        {
            this.certificate.Reset();
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return new X509SecurityToken(this.certificate);
        }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.certificate;
            }
        }
    }
}

