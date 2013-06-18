namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcAsymmetricCryptoParameters
    {
        public int keySize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string keyExchangeAlgorithm;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string signatureAlgorithm;
    }
}

