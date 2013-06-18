namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcInfoCardCryptoHandle
    {
        public HandleType type;
        public long expiration;
        public IntPtr cryptoParameters;
        public enum HandleType
        {
            Asymmetric = 1,
            Hash = 4,
            Symmetric = 2,
            Transform = 3
        }
    }
}

