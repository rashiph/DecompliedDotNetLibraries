namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public abstract class DES : SymmetricAlgorithm
    {
        private static KeySizes[] s_legalBlockSizes = new KeySizes[] { new KeySizes(0x40, 0x40, 0) };
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(0x40, 0x40, 0) };

        protected DES()
        {
            base.KeySizeValue = 0x40;
            base.BlockSizeValue = 0x40;
            base.FeedbackSizeValue = base.BlockSizeValue;
            base.LegalBlockSizesValue = s_legalBlockSizes;
            base.LegalKeySizesValue = s_legalKeySizes;
        }

        [SecuritySafeCritical]
        public static DES Create()
        {
            return Create("System.Security.Cryptography.DES");
        }

        [SecuritySafeCritical]
        public static DES Create(string algName)
        {
            return (DES) CryptoConfig.CreateFromName(algName);
        }

        private static bool IsLegalKeySize(byte[] rgbKey)
        {
            return ((rgbKey != null) && (rgbKey.Length == 8));
        }

        public static bool IsSemiWeakKey(byte[] rgbKey)
        {
            if (!IsLegalKeySize(rgbKey))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
            }
            ulong num = QuadWordFromBigEndian(Utils.FixupKeyParity(rgbKey));
            if (((((num != 0x1fe01fe01fe01feL) && (num != 18303189645120372225L)) && ((num != 0x1fe01fe00ef10ef1L) && (num != 16149873216566784270L))) && (((num != 0x1e001e001f101f1L) && (num != 16141428838415593729L)) && ((num != 0x1ffe1ffe0efe0efeL) && (num != 18311634023271562766L)))) && (((num != 0x11f011f010e010eL) && (num != 0x1f011f010e010e01L)) && ((num != 16212643094166696446L) && (num != 18365959522720284401L))))
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
            ulong num = QuadWordFromBigEndian(Utils.FixupKeyParity(rgbKey));
            if (((num != 0x101010101010101L) && (num != 18374403900871474942L)) && ((num != 0x1f1f1f1f0e0e0e0eL) && (num != 16204198716015505905L)))
            {
                return false;
            }
            return true;
        }

        private static ulong QuadWordFromBigEndian(byte[] block)
        {
            return (ulong) ((((((((block[0] << 0x38) | (block[1] << 0x30)) | (block[2] << 40)) | (block[3] << 0x20)) | (block[4] << 0x18)) | (block[5] << 0x10)) | (block[6] << 8)) | block[7]);
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
                    while (IsWeakKey(base.KeyValue) || IsSemiWeakKey(base.KeyValue));
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
                    throw new ArgumentException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
                }
                if (IsWeakKey(value))
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"), "DES");
                }
                if (IsSemiWeakKey(value))
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_SemiWeak"), "DES");
                }
                base.KeyValue = (byte[]) value.Clone();
                base.KeySizeValue = value.Length * 8;
            }
        }
    }
}

