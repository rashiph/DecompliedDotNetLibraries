namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class DSASignatureFormatter : AsymmetricSignatureFormatter
    {
        private DSA _dsaKey;
        private string _oid;

        public DSASignatureFormatter()
        {
            this._oid = CryptoConfig.MapNameToOID("SHA1");
        }

        public DSASignatureFormatter(AsymmetricAlgorithm key) : this()
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._dsaKey = (DSA) key;
        }

        public override byte[] CreateSignature(byte[] rgbHash)
        {
            if (rgbHash == null)
            {
                throw new ArgumentNullException("rgbHash");
            }
            if (this._oid == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingOID"));
            }
            if (this._dsaKey == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
            }
            return this._dsaKey.CreateSignature(rgbHash);
        }

        public override void SetHashAlgorithm(string strName)
        {
            if (CryptoConfig.MapNameToOID(strName) != this._oid)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_InvalidOperation"));
            }
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._dsaKey = (DSA) key;
        }
    }
}

