namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class StrongNameSignatureInformation
    {
        private AsymmetricAlgorithm m_publicKey;
        private SignatureVerificationResult m_verificationResult;
        private static readonly string StrongNameHashAlgorithm = CapiNative.GetAlgorithmName(CapiNative.AlgorithmId.Sha1);

        internal StrongNameSignatureInformation(AsymmetricAlgorithm publicKey)
        {
            this.m_verificationResult = SignatureVerificationResult.Valid;
            this.m_publicKey = publicKey;
        }

        internal StrongNameSignatureInformation(SignatureVerificationResult error)
        {
            this.m_verificationResult = error;
        }

        public string HashAlgorithm
        {
            get
            {
                return StrongNameHashAlgorithm;
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
                return (this.m_verificationResult == SignatureVerificationResult.Valid);
            }
        }

        public AsymmetricAlgorithm PublicKey
        {
            get
            {
                return this.m_publicKey;
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

