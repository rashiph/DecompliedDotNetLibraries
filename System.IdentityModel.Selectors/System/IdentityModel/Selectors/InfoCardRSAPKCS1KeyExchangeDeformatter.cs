namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography;

    internal class InfoCardRSAPKCS1KeyExchangeDeformatter : RSAPKCS1KeyExchangeDeformatter
    {
        private RSA m_rsaKey;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InfoCardRSAPKCS1KeyExchangeDeformatter()
        {
        }

        public InfoCardRSAPKCS1KeyExchangeDeformatter(AsymmetricAlgorithm key) : base(key)
        {
            this.m_rsaKey = (RSA) key;
        }

        public override byte[] DecryptKeyExchange(byte[] rgbIn)
        {
            if ((this.m_rsaKey != null) && (this.m_rsaKey is InfoCardRSACryptoProvider))
            {
                return ((InfoCardRSACryptoProvider) this.m_rsaKey).Decrypt(rgbIn, false);
            }
            return base.DecryptKeyExchange(rgbIn);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            base.SetKey(key);
            this.m_rsaKey = (RSA) key;
        }
    }
}

