namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcGenericXmlToken
    {
        public long createDate;
        public long expiryDate;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string xmlToken;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string internalTokenReference;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string externalTokenReference;
    }
}

