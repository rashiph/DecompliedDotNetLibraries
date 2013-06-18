namespace System.IdentityModel.Tokens
{
    using System;

    public abstract class SecurityKey
    {
        protected SecurityKey()
        {
        }

        public abstract byte[] DecryptKey(string algorithm, byte[] keyData);
        public abstract byte[] EncryptKey(string algorithm, byte[] keyData);
        public abstract bool IsAsymmetricAlgorithm(string algorithm);
        public abstract bool IsSupportedAlgorithm(string algorithm);
        public abstract bool IsSymmetricAlgorithm(string algorithm);

        public abstract int KeySize { get; }
    }
}

