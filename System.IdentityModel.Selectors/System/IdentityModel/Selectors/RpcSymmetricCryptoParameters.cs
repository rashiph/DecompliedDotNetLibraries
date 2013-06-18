namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcSymmetricCryptoParameters
    {
        public int keySize;
        public int blockSize;
        public int feedbackSize;
    }
}

