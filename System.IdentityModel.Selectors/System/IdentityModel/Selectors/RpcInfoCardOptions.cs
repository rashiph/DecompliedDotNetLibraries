namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcInfoCardOptions
    {
        public bool UISuppression;
        public int cchKeyLength;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string keyType;
        public int cbKeyValue;
        public byte[] keyValue;
    }
}

