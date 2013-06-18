namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography;

    internal class InfoCardRSAPKCS1SignatureFormatter : RSAPKCS1SignatureFormatter
    {
        private RSA m_rsaKey;
        private string m_strOID;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InfoCardRSAPKCS1SignatureFormatter()
        {
        }

        public InfoCardRSAPKCS1SignatureFormatter(AsymmetricAlgorithm key) : base(key)
        {
            this.m_rsaKey = (RSA) key;
        }

        public override byte[] CreateSignature(byte[] rgbHash)
        {
            if (((this.m_strOID != null) && (this.m_rsaKey != null)) && ((rgbHash != null) && (this.m_rsaKey is InfoCardRSACryptoProvider)))
            {
                return ((InfoCardRSACryptoProvider) this.m_rsaKey).SignHash(rgbHash, this.m_strOID);
            }
            return base.CreateSignature(rgbHash);
        }

        public override void SetHashAlgorithm(string strName)
        {
            base.SetHashAlgorithm(strName);
            this.m_strOID = CryptoConfig.MapNameToOID(strName);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            base.SetKey(key);
            this.m_rsaKey = (RSA) key;
        }
    }
}

