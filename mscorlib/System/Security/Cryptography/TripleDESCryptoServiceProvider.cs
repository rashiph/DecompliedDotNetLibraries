namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public sealed class TripleDESCryptoServiceProvider : TripleDES
    {
        [SecuritySafeCritical]
        public TripleDESCryptoServiceProvider()
        {
            if (!Utils.HasAlgorithm(0x6603, 0))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_AlgorithmNotAvailable"));
            }
            base.FeedbackSizeValue = 8;
        }

        [SecurityCritical]
        private ICryptoTransform _NewEncryptor(byte[] rgbKey, CipherMode mode, byte[] rgbIV, int feedbackSize, CryptoAPITransformMode encryptMode)
        {
            int index = 0;
            int[] rgArgIds = new int[10];
            object[] rgArgValues = new object[10];
            int algid = 0x6603;
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
            if (rgbKey.Length == 0x10)
            {
                algid = 0x6609;
            }
            return new CryptoAPITransform(algid, index, rgArgIds, rgArgValues, rgbKey, base.PaddingValue, mode, base.BlockSizeValue, feedbackSize, false, encryptMode);
        }

        [SecuritySafeCritical]
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (TripleDES.IsWeakKey(rgbKey))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"), "TripleDES");
            }
            return this._NewEncryptor(rgbKey, base.ModeValue, rgbIV, base.FeedbackSizeValue, CryptoAPITransformMode.Decrypt);
        }

        [SecuritySafeCritical]
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (TripleDES.IsWeakKey(rgbKey))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"), "TripleDES");
            }
            return this._NewEncryptor(rgbKey, base.ModeValue, rgbIV, base.FeedbackSizeValue, CryptoAPITransformMode.Encrypt);
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
            while (TripleDES.IsWeakKey(base.KeyValue))
            {
                Utils.StaticRandomNumberGenerator.GetBytes(base.KeyValue);
            }
        }
    }
}

