namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class MaskGenerationMethod
    {
        protected MaskGenerationMethod()
        {
        }

        [ComVisible(true)]
        public abstract byte[] GenerateMask(byte[] rgbSeed, int cbReturn);
    }
}

