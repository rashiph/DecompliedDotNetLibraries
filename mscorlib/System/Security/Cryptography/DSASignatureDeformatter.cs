namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class DSASignatureDeformatter : AsymmetricSignatureDeformatter
    {
        private DSA _dsaKey;
        private string _oid;

        public DSASignatureDeformatter()
        {
            this._oid = CryptoConfig.MapNameToOID("SHA1");
        }

        public DSASignatureDeformatter(AsymmetricAlgorithm key) : this()
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._dsaKey = (DSA) key;
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

        public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
        {
            if (rgbHash == null)
            {
                throw new ArgumentNullException("rgbHash");
            }
            if (rgbSignature == null)
            {
                throw new ArgumentNullException("rgbSignature");
            }
            if (this._dsaKey == null)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
            }
            return this._dsaKey.VerifySignature(rgbHash, rgbSignature);
        }
    }
}

