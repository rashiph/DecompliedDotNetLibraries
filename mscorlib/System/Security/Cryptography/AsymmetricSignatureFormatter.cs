namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class AsymmetricSignatureFormatter
    {
        protected AsymmetricSignatureFormatter()
        {
        }

        public virtual byte[] CreateSignature(HashAlgorithm hash)
        {
            if (hash == null)
            {
                throw new ArgumentNullException("hash");
            }
            this.SetHashAlgorithm(hash.ToString());
            return this.CreateSignature(hash.Hash);
        }

        public abstract byte[] CreateSignature(byte[] rgbHash);
        public abstract void SetHashAlgorithm(string strName);
        public abstract void SetKey(AsymmetricAlgorithm key);
    }
}

