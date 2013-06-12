namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class TimestampInformation
    {
        private CapiNative.AlgorithmId m_hashAlgorithmId;
        private DateTime m_timestamp;
        private X509Chain m_timestampChain;
        private X509Certificate2 m_timestamper;
        private SignatureVerificationResult m_verificationResult;

        internal TimestampInformation(SignatureVerificationResult error)
        {
            this.m_verificationResult = error;
        }

        [SecurityCritical]
        internal TimestampInformation(X509Native.AXL_AUTHENTICODE_TIMESTAMPER_INFO timestamper)
        {
            this.m_hashAlgorithmId = timestamper.algHash;
            this.m_verificationResult = (SignatureVerificationResult) timestamper.dwError;
            ulong num = (((ulong) timestamper.ftTimestamp.dwHighDateTime) << 0x20) | ((ulong) timestamper.ftTimestamp.dwLowDateTime);
            this.m_timestamp = DateTime.FromFileTimeUtc((long) num);
            if (timestamper.pChainContext != IntPtr.Zero)
            {
                this.m_timestampChain = new X509Chain(timestamper.pChainContext);
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

        public bool IsValid
        {
            get
            {
                if (this.VerificationResult != SignatureVerificationResult.Valid)
                {
                    return (this.VerificationResult == SignatureVerificationResult.CertificateNotExplicitlyTrusted);
                }
                return true;
            }
        }

        public X509Chain SignatureChain
        {
            [StorePermission(SecurityAction.Demand, OpenStore=true, EnumerateCertificates=true)]
            get
            {
                return this.m_timestampChain;
            }
        }

        public X509Certificate2 SigningCertificate
        {
            [StorePermission(SecurityAction.Demand, OpenStore=true, EnumerateCertificates=true)]
            get
            {
                if ((this.m_timestamper == null) && (this.SignatureChain != null))
                {
                    this.m_timestamper = this.SignatureChain.ChainElements[0].Certificate;
                }
                return this.m_timestamper;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return this.m_timestamp.ToLocalTime();
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

