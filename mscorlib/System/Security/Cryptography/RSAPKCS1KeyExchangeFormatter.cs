namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class RSAPKCS1KeyExchangeFormatter : AsymmetricKeyExchangeFormatter
    {
        private RSA _rsaKey;
        private RandomNumberGenerator RngValue;

        public RSAPKCS1KeyExchangeFormatter()
        {
        }

        public RSAPKCS1KeyExchangeFormatter(AsymmetricAlgorithm key)
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
                return ((RSACryptoServiceProvider) this._rsaKey).Encrypt(rgbData, false);
            }
            int num = this._rsaKey.KeySize / 8;
            if ((rgbData.Length + 11) > num)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_Padding_EncDataTooBig", new object[] { num - 11 }));
            }
            byte[] data = new byte[num];
            if (this.RngValue == null)
            {
                this.RngValue = RandomNumberGenerator.Create();
            }
            this.Rng.GetNonZeroBytes(data);
            data[0] = 0;
            data[1] = 2;
            data[(num - rgbData.Length) - 1] = 0;
            Buffer.InternalBlockCopy(rgbData, 0, data, num - rgbData.Length, rgbData.Length);
            return this._rsaKey.EncryptValue(data);
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

        public override string Parameters
        {
            get
            {
                return "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />";
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

