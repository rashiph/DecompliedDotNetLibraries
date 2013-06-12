namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class RSAOAEPKeyExchangeFormatter : AsymmetricKeyExchangeFormatter
    {
        private RSA _rsaKey;
        private byte[] ParameterValue;
        private RandomNumberGenerator RngValue;

        public RSAOAEPKeyExchangeFormatter()
        {
        }

        public RSAOAEPKeyExchangeFormatter(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        [SecuritySafeCritical]
        public override byte[] CreateKeyExchange(byte[] rgbData)
        {
            if (this._rsaKey == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
            }
            if (this._rsaKey is RSACryptoServiceProvider)
            {
                return ((RSACryptoServiceProvider) this._rsaKey).Encrypt(rgbData, true);
            }
            return Utils.RsaOaepEncrypt(this._rsaKey, SHA1.Create(), new PKCS1MaskGenerationMethod(), RandomNumberGenerator.Create(), rgbData);
        }

        public override byte[] CreateKeyExchange(byte[] rgbData, Type symAlgType)
        {
            return this.CreateKeyExchange(rgbData);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        public byte[] Parameter
        {
            get
            {
                if (this.ParameterValue != null)
                {
                    return (byte[]) this.ParameterValue.Clone();
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    this.ParameterValue = (byte[]) value.Clone();
                }
                else
                {
                    this.ParameterValue = null;
                }
            }
        }

        public override string Parameters
        {
            get
            {
                return null;
            }
        }

        public RandomNumberGenerator Rng
        {
            get
            {
                return this.RngValue;
            }
            set
            {
                this.RngValue = value;
            }
        }
    }
}

