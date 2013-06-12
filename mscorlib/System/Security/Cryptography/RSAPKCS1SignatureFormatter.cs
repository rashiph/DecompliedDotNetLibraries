namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class RSAPKCS1SignatureFormatter : AsymmetricSignatureFormatter
    {
        private RSA _rsaKey;
        private string _strOID;

        public RSAPKCS1SignatureFormatter()
        {
        }

        public RSAPKCS1SignatureFormatter(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        [SecuritySafeCritical]
        public override byte[] CreateSignature(byte[] rgbHash)
        {
            if (rgbHash == null)
            {
                throw new ArgumentNullException("rgbHash");
            }
            if (this._strOID == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingOID"));
            }
            if (this._rsaKey == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
            }
            if (this._rsaKey is RSACryptoServiceProvider)
            {
                return ((RSACryptoServiceProvider) this._rsaKey).SignHash(rgbHash, this._strOID);
            }
            byte[] rgb = Utils.RsaPkcs1Padding(this._rsaKey, CryptoConfig.EncodeOID(this._strOID), rgbHash);
            return this._rsaKey.DecryptValue(rgb);
        }

        public override void SetHashAlgorithm(string strName)
        {
            this._strOID = CryptoConfig.MapNameToOID(strName);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }
    }
}

