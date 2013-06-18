namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography;

    internal class InfoCardRSAOAEPKeyExchangeDeformatter : RSAOAEPKeyExchangeDeformatter
    {
        private RSA m_rsaKey;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InfoCardRSAOAEPKeyExchangeDeformatter()
        {
        }

        public InfoCardRSAOAEPKeyExchangeDeformatter(AsymmetricAlgorithm key) : base(key)
        {
            this.m_rsaKey = (RSA) key;
        }

        public override byte[] DecryptKeyExchange(byte[] rgbData)
        {
            if ((this.m_rsaKey != null) && (this.m_rsaKey is InfoCardRSACryptoProvider))
            {
                return ((InfoCardRSACryptoProvider) this.m_rsaKey).Decrypt(rgbData, true);
            }
            return base.DecryptKeyExchange(rgbData);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            base.SetKey(key);
            this.m_rsaKey = (RSA) key;
        }
    }
}

