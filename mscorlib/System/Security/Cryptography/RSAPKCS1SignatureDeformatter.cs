namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;

    [ComVisible(true)]
    public class RSAPKCS1SignatureDeformatter : AsymmetricSignatureDeformatter
    {
        private RSA _rsaKey;
        private string _strOID;

        public RSAPKCS1SignatureDeformatter()
        {
        }

        public RSAPKCS1SignatureDeformatter(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        public override void SetHashAlgorithm(string strName)
        {
            this._strOID = CryptoConfig.MapNameToOID(strName, OidGroup.HashAlgorithm);
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._rsaKey = (RSA) key;
        }

        [SecuritySafeCritical]
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
                int calgHash = X509Utils.OidToAlgIdStrict(this._strOID, OidGroup.HashAlgorithm);
                return ((RSACryptoServiceProvider) this._rsaKey).VerifyHash(rgbHash, calgHash, rgbSignature);
            }
            byte[] rhs = Utils.RsaPkcs1Padding(this._rsaKey, CryptoConfig.EncodeOID(this._strOID), rgbHash);
            return Utils.CompareBigIntArrays(this._rsaKey.EncryptValue(rgbSignature), rhs);
        }
    }
}

