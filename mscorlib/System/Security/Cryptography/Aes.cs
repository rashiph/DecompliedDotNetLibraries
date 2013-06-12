namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
    public abstract class Aes : SymmetricAlgorithm
    {
        private static KeySizes[] s_legalBlockSizes = new KeySizes[] { new KeySizes(0x80, 0x80, 0) };
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(0x80, 0x100, 0x40) };

        protected Aes()
        {
            base.LegalBlockSizesValue = s_legalBlockSizes;
            base.LegalKeySizesValue = s_legalKeySizes;
            base.BlockSizeValue = 0x80;
            base.FeedbackSizeValue = 8;
            base.KeySizeValue = 0x100;
            base.ModeValue = CipherMode.CBC;
        }

        public static Aes Create()
        {
            return Create("AES");
        }

        public static Aes Create(string algorithmName)
        {
            if (algorithmName == null)
            {
                throw new ArgumentNullException("algorithmName");
            }
            return (CryptoConfig.CreateFromName(algorithmName) as Aes);
        }
    }
}

