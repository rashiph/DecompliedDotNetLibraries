namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class PKCS1MaskGenerationMethod : MaskGenerationMethod
    {
        private string HashNameValue = "SHA1";

        [SecuritySafeCritical]
        public override byte[] GenerateMask(byte[] rgbSeed, int cbReturn)
        {
            HashAlgorithm algorithm = (HashAlgorithm) CryptoConfig.CreateFromName(this.HashNameValue);
            byte[] counter = new byte[4];
            byte[] dst = new byte[cbReturn];
            uint num = 0;
            for (int i = 0; i < dst.Length; i += algorithm.Hash.Length)
            {
                Utils.ConvertIntToByteArray(num++, ref counter);
                algorithm.TransformBlock(rgbSeed, 0, rgbSeed.Length, rgbSeed, 0);
                algorithm.TransformFinalBlock(counter, 0, 4);
                byte[] hash = algorithm.Hash;
                algorithm.Initialize();
                if ((dst.Length - i) > hash.Length)
                {
                    Buffer.BlockCopy(hash, 0, dst, i, hash.Length);
                }
                else
                {
                    Buffer.BlockCopy(hash, 0, dst, i, dst.Length - i);
                }
            }
            return dst;
        }

        public string HashName
        {
            get
            {
                return this.HashNameValue;
            }
            set
            {
                this.HashNameValue = value;
                if (this.HashNameValue == null)
                {
                    this.HashNameValue = "SHA1";
                }
            }
        }
    }
}

