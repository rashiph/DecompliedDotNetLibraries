namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography;

    internal class InfoCardRSAPKCS1SignatureDeformatter : RSAPKCS1SignatureDeformatter
    {
        private RSA m_rsaKey;
        private string m_strOID;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InfoCardRSAPKCS1SignatureDeformatter()
        {
        }

        public InfoCardRSAPKCS1SignatureDeformatter(AsymmetricAlgorithm key) : base(key)
        {
            this.m_rsaKey = (RSA) key;
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

        public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
        {
            if ((((this.m_strOID != null) && (this.m_rsaKey != null)) && ((rgbHash != null) && (rgbSignature != null))) && (this.m_rsaKey is InfoCardRSACryptoProvider))
            {
                return ((InfoCardRSACryptoProvider) this.m_rsaKey).VerifyHash(rgbHash, this.m_strOID, rgbSignature);
            }
            return base.VerifySignature(rgbHash, rgbSignature);
        }
    }
}

