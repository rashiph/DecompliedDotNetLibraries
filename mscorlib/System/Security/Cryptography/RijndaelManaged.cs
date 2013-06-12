namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public sealed class RijndaelManaged : Rijndael
    {
        public RijndaelManaged()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return this.NewEncryptor(rgbKey, base.ModeValue, rgbIV, base.FeedbackSizeValue, RijndaelManagedTransformMode.Decrypt);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return this.NewEncryptor(rgbKey, base.ModeValue, rgbIV, base.FeedbackSizeValue, RijndaelManagedTransformMode.Encrypt);
        }

        public override void GenerateIV()
        {
            base.IVValue = Utils.GenerateRandom(base.BlockSizeValue / 8);
        }

        public override void GenerateKey()
        {
            base.KeyValue = Utils.GenerateRandom(base.KeySizeValue / 8);
        }

        private ICryptoTransform NewEncryptor(byte[] rgbKey, CipherMode mode, byte[] rgbIV, int feedbackSize, RijndaelManagedTransformMode encryptMode)
        {
            if (rgbKey == null)
            {
                rgbKey = Utils.GenerateRandom(base.KeySizeValue / 8);
            }
            if (rgbIV == null)
            {
                rgbIV = Utils.GenerateRandom(base.BlockSizeValue / 8);
            }
            return new RijndaelManagedTransform(rgbKey, mode, rgbIV, base.BlockSizeValue, feedbackSize, base.PaddingValue, encryptMode);
        }
    }
}

