namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class AuthenticodeSignatureInformation
    {
        private string m_description;
        private Uri m_descriptionUrl;
        private CapiNative.AlgorithmId m_hashAlgorithmId;
        private X509Chain m_signatureChain;
        private X509Certificate2 m_signingCertificate;
        private TimestampInformation m_timestamp;
        private SignatureVerificationResult m_verificationResult;

        internal AuthenticodeSignatureInformation(SignatureVerificationResult error)
        {
            this.m_verificationResult = error;
        }

        internal AuthenticodeSignatureInformation(X509Native.AXL_AUTHENTICODE_SIGNER_INFO signer, X509Chain signatureChain, TimestampInformation timestamp)
        {
            this.m_verificationResult = (SignatureVerificationResult) signer.dwError;
            this.m_hashAlgorithmId = signer.algHash;
            if (signer.pwszDescription != IntPtr.Zero)
            {
                this.m_description = Marshal.PtrToStringUni(signer.pwszDescription);
            }
            if (signer.pwszDescriptionUrl != IntPtr.Zero)
            {
                Uri.TryCreate(Marshal.PtrToStringUni(signer.pwszDescriptionUrl), UriKind.RelativeOrAbsolute, out this.m_descriptionUrl);
            }
            this.m_signatureChain = signatureChain;
            if ((timestamp != null) && (timestamp.VerificationResult != SignatureVerificationResult.MissingSignature))
            {
                if (timestamp.IsValid)
                {
                    this.m_timestamp = timestamp;
                }
                else
                {
                    this.m_verificationResult = SignatureVerificationResult.InvalidTimestamp;
                }
            }
            else
            {
                this.m_timestamp = null;
            }
        }

        public string Description
        {
            get
            {
                return this.m_description;
            }
        }

        public Uri DescriptionUrl
        {
            get
            {
                return this.m_descriptionUrl;
            }
        }

        public string HashAlgorithm
        {
            get
            {
                return CapiNative.GetAlgorithmName(this.m_hashAlgorithmId);
            }
        }

        public int HResult
        {
            get
            {
                return CapiNative.HResultForVerificationResult(this.m_verificationResult);
            }
        }

        public X509Chain SignatureChain
        {
            [StorePermission(SecurityAction.Demand, OpenStore=true, EnumerateCertificates=true)]
            get
            {
                return this.m_signatureChain;
            }
        }

        public X509Certificate2 SigningCertificate
        {
            [StorePermission(SecurityAction.Demand, OpenStore=true, EnumerateCertificates=true)]
            get
            {
                if ((this.m_signingCertificate == null) && (this.SignatureChain != null))
                {
                    this.m_signingCertificate = this.SignatureChain.ChainElements[0].Certificate;
                }
                return this.m_signingCertificate;
            }
        }

        public TimestampInformation Timestamp
        {
            get
            {
                return this.m_timestamp;
            }
        }

        public System.Security.Cryptography.X509Certificates.TrustStatus TrustStatus
        {
            get
            {
                SignatureVerificationResult verificationResult = this.VerificationResult;
                if (verificationResult != SignatureVerificationResult.CertificateNotExplicitlyTrusted)
                {
                    if (verificationResult != SignatureVerificationResult.CertificateExplicitlyDistrusted)
                    {
                        if (verificationResult == SignatureVerificationResult.Valid)
                        {
                            return System.Security.Cryptography.X509Certificates.TrustStatus.Trusted;
                        }
                        return System.Security.Cryptography.X509Certificates.TrustStatus.UnknownIdentity;
                    }
                    return System.Security.Cryptography.X509Certificates.TrustStatus.Untrusted;
                }
                return System.Security.Cryptography.X509Certificates.TrustStatus.KnownIdentity;
            }
        }

        public SignatureVerificationResult VerificationResult
        {
            get
            {
                return this.m_verificationResult;
            }
        }
    }
}

