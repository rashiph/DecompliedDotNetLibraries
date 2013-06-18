namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography;

    internal class InfoCardRSAOAEPKeyExchangeFormatter : RSAOAEPKeyExchangeFormatter
    {
        private RSA m_rsaKey;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InfoCardRSAOAEPKeyExchangeFormatter()
        {
        }

        public InfoCardRSAOAEPKeyExchangeFormatter(AsymmetricAlgorithm key) : base(key)
        {
            this.m_rsaKey = (RSA) key;
        }

        public override byte[] CreateKeyExchange(byte[] rgbData)
        {
            if ((this.m_rsaKey != null) && (this.m_rsaKey is InfoCardRSACryptoProvider))
            {
                return ((InfoCardRSACryptoProvider) this.m_rsaKey).Encrypt(rgbData, true);
            }
            return base.CreateKeyExchange(rgbData);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            base.SetKey(key);
            this.m_rsaKey = (RSA) key;
        }
    }
}

