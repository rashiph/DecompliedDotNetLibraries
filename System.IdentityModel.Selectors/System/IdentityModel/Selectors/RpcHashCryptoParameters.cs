namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcHashCryptoParameters
    {
        public int hashSize;
        public RpcTransformCryptoParameters transform;
    }
}

