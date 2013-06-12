namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class AsymmetricKeyExchangeFormatter
    {
        protected AsymmetricKeyExchangeFormatter()
        {
        }

        public abstract byte[] CreateKeyExchange(byte[] data);
        public abstract byte[] CreateKeyExchange(byte[] data, Type symAlgType);
        public abstract void SetKey(AsymmetricAlgorithm key);

        public abstract string Parameters { get; }
    }
}

