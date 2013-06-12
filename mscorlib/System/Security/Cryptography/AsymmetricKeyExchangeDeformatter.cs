namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class AsymmetricKeyExchangeDeformatter
    {
        protected AsymmetricKeyExchangeDeformatter()
        {
        }

        public abstract byte[] DecryptKeyExchange(byte[] rgb);
        public abstract void SetKey(AsymmetricAlgorithm key);

        public abstract string Parameters { get; set; }
    }
}

