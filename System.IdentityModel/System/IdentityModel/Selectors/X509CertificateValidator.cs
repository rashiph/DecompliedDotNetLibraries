namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public abstract class X509CertificateValidator
    {
        private static X509CertificateValidator chainTrust;
        private static X509CertificateValidator none;
        private static X509CertificateValidator ntAuthChainTrust;
        private static X509CertificateValidator peerOrChainTrust;
        private static X509CertificateValidator peerTrust;

        protected X509CertificateValidator()
        {
        }

        public static X509CertificateValidator CreateChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
        {
            if (chainPolicy == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("chainPolicy");
            }
            return new ChainTrustValidator(useMachineContext, chainPolicy, 1);
        }

        public static X509CertificateValidator CreatePeerOrChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
        {
            if (chainPolicy == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("chainPolicy");
            }
            return new PeerOrChainTrustValidator(useMachineContext, chainPolicy);
        }

        public abstract void Validate(X509Certificate2 certificate);

        public static X509CertificateValidator ChainTrust
        {
            get
            {
                if (chainTrust == null)
                {
                    chainTrust = new ChainTrustValidator();
                }
                return chainTrust;
            }
        }

        public static X509CertificateValidator None
        {
            get
            {
                if (none == null)
                {
                    none = new NoneX509CertificateValidator();
                }
                return none;
            }
        }

        internal static X509CertificateValidator NTAuthChainTrust
        {
            get
            {
                if (ntAuthChainTrust == null)
                {
                    ntAuthChainTrust = new ChainTrustValidator(false, null, 6);
                }
                return ntAuthChainTrust;
            }
        }

        public static X509CertificateValidator PeerOrChainTrust
        {
            get
            {
                if (peerOrChainTrust == null)
                {
                    peerOrChainTrust = new PeerOrChainTrustValidator();
                }
                return peerOrChainTrust;
            }
        }

        public static X509CertificateValidator PeerTrust
        {
            get
            {
                if (peerTrust == null)
                {
                    peerTrust = new PeerTrustValidator();
                }
                return peerTrust;
            }
        }

        private class ChainTrustValidator : X509CertificateValidator
        {
            private X509ChainPolicy chainPolicy;
            private uint chainPolicyOID;
            private bool useMachineContext;

            public ChainTrustValidator()
            {
                this.chainPolicyOID = 1;
                this.chainPolicy = null;
            }

            public ChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy, uint chainPolicyOID)
            {
                this.chainPolicyOID = 1;
                this.useMachineContext = useMachineContext;
                this.chainPolicy = chainPolicy;
                this.chainPolicyOID = chainPolicyOID;
            }

            private static string GetChainStatusInformation(X509ChainStatus[] chainStatus)
            {
                if (chainStatus == null)
                {
                    return string.Empty;
                }
                StringBuilder builder = new StringBuilder(0x80);
                for (int i = 0; i < chainStatus.Length; i++)
                {
                    builder.Append(chainStatus[i].StatusInformation);
                    builder.Append(" ");
                }
                return builder.ToString();
            }

            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
                }
                X509CertificateChain chain = new X509CertificateChain(this.useMachineContext, this.chainPolicyOID);
                if (this.chainPolicy != null)
                {
                    chain.ChainPolicy = this.chainPolicy;
                }
                if (!chain.Build(certificate))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("X509ChainBuildFail", new object[] { System.IdentityModel.SecurityUtils.GetCertificateId(certificate), GetChainStatusInformation(chain.ChainStatus) })));
                }
            }
        }

        private class NoneX509CertificateValidator : X509CertificateValidator
        {
            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
                }
            }
        }

        private class PeerOrChainTrustValidator : X509CertificateValidator
        {
            private X509CertificateValidator chain;
            private X509CertificateValidator.PeerTrustValidator peer;

            public PeerOrChainTrustValidator()
            {
                this.chain = X509CertificateValidator.ChainTrust;
                this.peer = (X509CertificateValidator.PeerTrustValidator) X509CertificateValidator.PeerTrust;
            }

            public PeerOrChainTrustValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
            {
                this.chain = X509CertificateValidator.CreateChainTrustValidator(useMachineContext, chainPolicy);
                this.peer = (X509CertificateValidator.PeerTrustValidator) X509CertificateValidator.PeerTrust;
            }

            public override void Validate(X509Certificate2 certificate)
            {
                Exception exception;
                if (certificate == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
                }
                if (!this.peer.TryValidate(certificate, out exception))
                {
                    try
                    {
                        this.chain.Validate(certificate);
                    }
                    catch (SecurityTokenValidationException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(exception.Message + " " + exception2.Message));
                    }
                }
            }
        }

        private class PeerTrustValidator : X509CertificateValidator
        {
            private static bool StoreContainsCertificate(StoreName storeName, X509Certificate2 certificate)
            {
                bool flag;
                X509CertificateStore store = new X509CertificateStore(storeName, StoreLocation.CurrentUser);
                X509Certificate2Collection certificates = null;
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    certificates = store.Find(X509FindType.FindByThumbprint, certificate.GetCertHash(), false);
                    flag = certificates.Count > 0;
                }
                finally
                {
                    System.IdentityModel.SecurityUtils.ResetAllCertificates(certificates);
                    store.Close();
                }
                return flag;
            }

            internal bool TryValidate(X509Certificate2 certificate, out Exception exception)
            {
                DateTime now = DateTime.Now;
                if ((now > certificate.NotAfter) || (now < certificate.NotBefore))
                {
                    exception = new SecurityTokenValidationException(System.IdentityModel.SR.GetString("X509InvalidUsageTime", new object[] { System.IdentityModel.SecurityUtils.GetCertificateId(certificate), now, certificate.NotBefore, certificate.NotAfter }));
                    return false;
                }
                if (!StoreContainsCertificate(StoreName.TrustedPeople, certificate))
                {
                    exception = new SecurityTokenValidationException(System.IdentityModel.SR.GetString("X509IsNotInTrustedStore", new object[] { System.IdentityModel.SecurityUtils.GetCertificateId(certificate) }));
                    return false;
                }
                if (StoreContainsCertificate(StoreName.Disallowed, certificate))
                {
                    exception = new SecurityTokenValidationException(System.IdentityModel.SR.GetString("X509IsInUntrustedStore", new object[] { System.IdentityModel.SecurityUtils.GetCertificateId(certificate) }));
                    return false;
                }
                exception = null;
                return true;
            }

            public override void Validate(X509Certificate2 certificate)
            {
                Exception exception;
                if (certificate == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
                }
                if (!this.TryValidate(certificate, out exception))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
            }
        }
    }
}

