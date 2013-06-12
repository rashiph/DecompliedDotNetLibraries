namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class RSAOAEPKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter
    {
        private RSA _rsaKey;

        public RSAOAEPKeyExchangeDeformatter()
        {
        }

        public RSAOAEPKeyExchangeDeformatter(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        [SecuritySafeCritical]
        public override byte[] DecryptKeyExchange(byte[] rgbData)
        {
            if (this._rsaKey == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
            }
            if (this._rsaKey is RSACryptoServiceProvider)
            {
                return ((RSACryptoServiceProvider) this._rsaKey).Decrypt(rgbData, true);
            }
            return Utils.RsaOaepDecrypt(this._rsaKey, SHA1.Create(), new PKCS1MaskGenerationMethod(), rgbData);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        public override string Parameters
        {
            get
            {
                return null;
            }
            set
            {
            }
        }
    }
}

