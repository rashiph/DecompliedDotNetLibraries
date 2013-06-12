namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class AsymmetricSignatureDeformatter
    {
        protected AsymmetricSignatureDeformatter()
        {
        }

        public abstract void SetHashAlgorithm(string strName);
        public abstract void SetKey(AsymmetricAlgorithm key);
        public virtual bool VerifySignature(HashAlgorithm hash, byte[] rgbSignature)
        {
            if (hash == null)
            {
                throw new ArgumentNullException("hash");
            }
            this.SetHashAlgorithm(hash.ToString());
            return this.VerifySignature(hash.Hash, rgbSignature);
        }

        public abstract bool VerifySignature(byte[] rgbHash, byte[] rgbSignature);
    }
}

