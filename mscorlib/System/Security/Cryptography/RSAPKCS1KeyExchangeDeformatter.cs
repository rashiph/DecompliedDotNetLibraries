namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class RSAPKCS1KeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter
    {
        private RSA _rsaKey;
        private RandomNumberGenerator RngValue;

        public RSAPKCS1KeyExchangeDeformatter()
        {
        }

        public RSAPKCS1KeyExchangeDeformatter(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        [SecuritySafeCritical]
        public override byte[] DecryptKeyExchange(byte[] rgbIn)
        {
            if (this._rsaKey == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
            }
            if (this._rsaKey is RSACryptoServiceProvider)
            {
                return ((RSACryptoServiceProvider) this._rsaKey).Decrypt(rgbIn, false);
            }
            byte[] src = this._rsaKey.DecryptValue(rgbIn);
            int index = 2;
            while (index < src.Length)
            {
                if (src[index] == 0)
                {
                    break;
                }
                index++;
            }
            if (index >= src.Length)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_PKCS1Decoding"));
            }
            index++;
            byte[] dst = new byte[src.Length - index];
            Buffer.InternalBlockCopy(src, index, dst, 0, dst.Length);
            return dst;
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

        public RandomNumberGenerator RNG
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

