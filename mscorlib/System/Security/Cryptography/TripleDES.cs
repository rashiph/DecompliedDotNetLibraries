namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class TripleDES : SymmetricAlgorithm
    {
        private static KeySizes[] s_legalBlockSizes = new KeySizes[] { new KeySizes(0x40, 0x40, 0) };
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(0x80, 0xc0, 0x40) };

        protected TripleDES()
        {
            base.KeySizeValue = 0xc0;
            base.BlockSizeValue = 0x40;
            base.FeedbackSizeValue = base.BlockSizeValue;
            base.LegalBlockSizesValue = s_legalBlockSizes;
            base.LegalKeySizesValue = s_legalKeySizes;
        }

        [SecuritySafeCritical]
        public static TripleDES Create()
        {
            return Create("System.Security.Cryptography.TripleDES");
        }

        [SecuritySafeCritical]
        public static TripleDES Create(string str)
        {
            return (TripleDES) CryptoConfig.CreateFromName(str);
        }

        private static bool EqualBytes(byte[] rgbKey, int start1, int start2, int count)
        {
            if (start1 < 0)
            {
                throw new ArgumentOutOfRangeException("start1", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (start2 < 0)
            {
                throw new ArgumentOutOfRangeException("start2", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((start1 + count) > rgbKey.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
            }
            if ((start2 + count) > rgbKey.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
            }
            for (int i = 0; i < count; i++)
            {
                if (rgbKey[start1 + i] != rgbKey[start2 + i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsLegalKeySize(byte[] rgbKey)
        {
            if ((rgbKey == null) || ((rgbKey.Length != 0x10) && (rgbKey.Length != 0x18)))
            {
                return false;
            }
            return true;
        }

        public static bool IsWeakKey(byte[] rgbKey)
        {
            if (!IsLegalKeySize(rgbKey))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
            }
            byte[] buffer = Utils.FixupKeyParity(rgbKey);
            return (EqualBytes(buffer, 0, 8, 8) || ((buffer.Length == 0x18) && EqualBytes(buffer, 8, 0x10, 8)));
        }

        public override byte[] Key
        {
            get
            {
                if (base.KeyValue == null)
                {
                    do
                    {
                        this.GenerateKey();
                    }
                    while (IsWeakKey(base.KeyValue));
                }
                return (byte[]) base.KeyValue.Clone();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!base.ValidKeySize(value.Length * 8))
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
                }
                if (IsWeakKey(value))
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"), "TripleDES");
                }
                base.KeyValue = (byte[]) value.Clone();
                base.KeySizeValue = value.Length * 8;
            }
        }
    }
}

