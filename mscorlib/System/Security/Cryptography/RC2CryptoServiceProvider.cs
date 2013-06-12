namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public sealed class RC2CryptoServiceProvider : RC2
    {
        private bool m_use40bitSalt;
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(40, 0x80, 8) };

        [SecuritySafeCritical]
        public RC2CryptoServiceProvider()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            if (!Utils.HasAlgorithm(0x6602, 0))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_AlgorithmNotAvailable"));
            }
            base.LegalKeySizesValue = s_legalKeySizes;
            base.FeedbackSizeValue = 8;
        }

        [SecurityCritical]
        private ICryptoTransform _NewEncryptor(byte[] rgbKey, CipherMode mode, byte[] rgbIV, int effectiveKeySize, int feedbackSize, CryptoAPITransformMode encryptMode)
        {
            int index = 0;
            int[] rgArgIds = new int[10];
            object[] rgArgValues = new object[10];
            if (mode == CipherMode.OFB)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_OFBNotSupported"));
            }
            if ((mode == CipherMode.CFB) && (feedbackSize != 8))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_CFBSizeNotSupported"));
            }
            if (rgbKey == null)
            {
                rgbKey = new byte[base.KeySizeValue / 8];
                Utils.StaticRandomNumberGenerator.GetBytes(rgbKey);
            }
            int bitLength = rgbKey.Length * 8;
            if (!base.ValidKeySize(bitLength))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
            }
            rgArgIds[index] = 0x13;
            if (base.EffectiveKeySizeValue == 0)
            {
                rgArgValues[index] = bitLength;
            }
            else
            {
                rgArgValues[index] = effectiveKeySize;
            }
            index++;
            if (mode != CipherMode.CBC)
            {
                rgArgIds[index] = 4;
                rgArgValues[index] = mode;
                index++;
            }
            if (mode != CipherMode.ECB)
            {
                if (rgbIV == null)
                {
                    rgbIV = new byte[8];
                    Utils.StaticRandomNumberGenerator.GetBytes(rgbIV);
                }
                if (rgbIV.Length < 8)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidIVSize"));
                }
                rgArgIds[index] = 1;
                rgArgValues[index] = rgbIV;
                index++;
            }
            if ((mode == CipherMode.OFB) || (mode == CipherMode.CFB))
            {
                rgArgIds[index] = 5;
                rgArgValues[index] = feedbackSize;
                index++;
            }
            if (!Utils.HasAlgorithm(0x6602, bitLength))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_AlgKeySizeNotAvailable", new object[] { bitLength }));
            }
            return new CryptoAPITransform(0x6602, index, rgArgIds, rgArgValues, rgbKey, base.PaddingValue, mode, base.BlockSizeValue, feedbackSize, this.m_use40bitSalt, encryptMode);
        }

        [SecuritySafeCritical]
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return this._NewEncryptor(rgbKey, base.ModeValue, rgbIV, base.EffectiveKeySizeValue, base.FeedbackSizeValue, CryptoAPITransformMode.Decrypt);
        }

        [SecuritySafeCritical]
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return this._NewEncryptor(rgbKey, base.ModeValue, rgbIV, base.EffectiveKeySizeValue, base.FeedbackSizeValue, CryptoAPITransformMode.Encrypt);
        }

        public override void GenerateIV()
        {
            base.IVValue = new byte[8];
            Utils.StaticRandomNumberGenerator.GetBytes(base.IVValue);
        }

        public override void GenerateKey()
        {
            base.KeyValue = new byte[base.KeySizeValue / 8];
            Utils.StaticRandomNumberGenerator.GetBytes(base.KeyValue);
        }

        public override int EffectiveKeySize
        {
            get
            {
                return base.KeySizeValue;
            }
            set
            {
                if (value != base.KeySizeValue)
                {
                    throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_RC2_EKSKS2"));
                }
            }
        }

        [ComVisible(false)]
        public bool UseSalt
        {
            get
            {
                return this.m_use40bitSalt;
            }
            set
            {
                this.m_use40bitSalt = value;
            }
        }
    }
}

