namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel.Security.Tokens;

    internal sealed class NonceToken : BinarySecretSecurityToken
    {
        public NonceToken(byte[] key) : this(SecurityUniqueId.Create().Value, key)
        {
        }

        public NonceToken(int keySizeInBits) : base(SecurityUniqueId.Create().Value, keySizeInBits, false)
        {
        }

        public NonceToken(string id, byte[] key) : base(id, key, false)
        {
        }
    }
}

