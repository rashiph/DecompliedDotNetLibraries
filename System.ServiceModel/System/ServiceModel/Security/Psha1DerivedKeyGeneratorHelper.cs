namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;

    internal static class Psha1DerivedKeyGeneratorHelper
    {
        internal static byte[] GenerateDerivedKey(byte[] key, byte[] label, byte[] nonce, int derivedKeySize, int position)
        {
            Psha1DerivedKeyGenerator generator = new Psha1DerivedKeyGenerator(key);
            return generator.GenerateDerivedKey(label, nonce, derivedKeySize, position);
        }
    }
}

